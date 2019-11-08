namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ApplicationInsights;
    using Microsoft.OneWeek.Hack.Microids.Common;

    public class EnrichmentMessageRouter
    {
        private IIoTDeviceDataEnricher dataEnricher;
        private IDataSource dataSource;
        private IDataSink dataSink;
        private ITelemetryClient telemetryClient;
        private ILogger<EnrichmentMessageRouter> logger;
        private IConfiguration config;
        private int waiting = 0;

        public EnrichmentMessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher, ITelemetryClient telemetryClient, IConfiguration config, ILogger<EnrichmentMessageRouter> logger)
        {
            this.dataSource = dataSource;
            this.dataSink = dataSink;
            this.dataEnricher = dataEnricher;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
            this.config = config;

            // handle messages as they arrive
            this.MessageBatchReceived += (sender, e) =>
            {

                // process with parallelism
                Parallel.ForEach(e.Messages, async (message) =>
                {

                    // get the device id
                    var deviceId = message.GetDeviceId();

                    // get the metadata
                    bool success = false;
                    var startTime = DateTime.UtcNow;
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        var metadata = await this.dataEnricher.GetMetadataAsync(deviceId);
                        if (metadata != null)
                        {

                            // enrich the message
                            message.EnrichMessage(metadata);

                            // output the message
                            await this.dataSink.WriteMessageAsync(message);
                            success = true;

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "exception in MessageBatchReceived...");
                    }
                    finally
                    {
                        Interlocked.Decrement(ref waiting);
                        timer.Stop();
                        logger.LogDebug($"gRPC call took {timer.Elapsed.Milliseconds} ms");
                        telemetryClient.TrackDependency("gRPC call", "IoTClient", "GetMetadataAzync", startTime, timer.Elapsed, success);
                    }

                });

            };
        }

        public event EventHandler<MessageBatchReceivedEventArgs> MessageBatchReceived;

        public class MessageBatchReceivedEventArgs : EventArgs
        {
            public IEnumerable<IMessage> Messages { get; set; }
        }

        protected virtual void OnMessageBatchReceived(MessageBatchReceivedEventArgs e)
        {
            try
            {
                EventHandler<MessageBatchReceivedEventArgs> handler = MessageBatchReceived;
                handler?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception raised OnMessageBatchReceived...");
            }
        }

        private int GenerateMessagesEvery
        {
            get
            {
                string s = config.GetValue<string>("GENERATE_MESSAGES_EVERY");
                if (int.TryParse(s, out int i))
                {
                    return i;
                }
                else
                {
                    return 1000;
                }
            }
        }

        private int NumMessagesEachGeneration
        {
            get
            {
                string s = config.GetValue<string>("NUM_MESSAGES_EACH_GENERATION");
                if (int.TryParse(s, out int i))
                {
                    return i;
                }
                else
                {
                    return 1;
                }
            }
        }

        private int RestrictMessagesAtBufferSize
        {
            get
            {
                string s = config.GetValue<string>("RESTRICT_MESSAGES_AT_BUFFER_SIZE");
                if (int.TryParse(s, out int i))
                {
                    return i;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int MaxWaitToAddMessages
        {
            get
            {
                string s = config.GetValue<string>("MAX_WAIT_TO_ADD_MESSAGES");
                if (int.TryParse(s, out int i))
                {
                    return i;
                }
                else
                {
                    return 30000;
                }
            }
        }

        public Task Initiate(CancellationToken ct)
        {
            return Task.Run(() =>
            {

                // timer to generate messages
                Timer generate = new Timer(async (_) =>
                {
                    try
                    {

                        // generate the messages
                        var msgs = new List<IMessage>();
                        for (int i = 0; i < NumMessagesEachGeneration; i++)
                        {
                            var msg = await this.dataSource.ReadMessageAsync();
                            Interlocked.Increment(ref waiting);
                            msgs.Add(msg);
                        }

                        // wait until buffer is flushed
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        while (waiting > RestrictMessagesAtBufferSize)
                        {
                            if (stopwatch.ElapsedMilliseconds > MaxWaitToAddMessages) throw new TimeoutException("MAX_WAIT_TO_ADD_MESSAGES was exceeded.");
                            await Task.Delay(10);
                        }
                        stopwatch.Stop();

                        // release the batch
                        OnMessageBatchReceived(new MessageBatchReceivedEventArgs()
                        {
                            Messages = msgs
                        });

                    }
                    catch (TimeoutException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "there was an exception in the generate messages loop...");
                    }
                }, null, GenerateMessagesEvery, GenerateMessagesEvery);

                // timer to report status every 10 seconds
                Timer report = new Timer((_) =>
                {
                    logger.LogDebug($"buffer: {waiting}; threads: {Process.GetCurrentProcess().Threads.Count}");
                }, null, 10000, 10000);

                // wait for cancellation
                while (!ct.IsCancellationRequested)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(.250));
                }

                // dispose
                generate.Dispose();
                report.Dispose();
            }, ct);
        }

    }
}