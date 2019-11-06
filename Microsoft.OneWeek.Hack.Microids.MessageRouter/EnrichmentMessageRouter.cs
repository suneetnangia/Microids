namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ApplicationInsights;

    public class EnrichmentMessageRouter
    {
        private IIoTDeviceDataEnricher dataEnricher;
        private IDataSource dataSource;
        private IDataSink dataSink;
        private TelemetryClient telemetryClient;
        private ILogger<EnrichmentMessageRouter> logger;
        private IConfiguration config;

        public EnrichmentMessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher, TelemetryClient telemetryClient, IConfiguration config, ILogger<EnrichmentMessageRouter> logger)
        {
            this.dataSource = dataSource;
            this.dataSink = dataSink;
            this.dataEnricher = dataEnricher;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
            this.config = config;

            // handle messages as they arrive
            this.MessageReceived += async (sender, e) =>
            {

                // get the device id
                var deviceId = e.Message.GetDeviceId();

                // get the metadata
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var metadata = await this.dataEnricher.GetMetadataAsync(deviceId);
                    if (metadata != null)
                    {

                        // enrich the message
                        e.Message.EnrichMessage(metadata);

                        // output the message
                        await this.dataSink.WriteMessageAsync(e.Message);

                        // send the telemetry
                        timer.Stop();
                        telemetryClient.TrackDependency("gRPC call", "IoTClient", "GetMetadataAzync", startTime, timer.Elapsed, true);

                    }
                }
                catch
                {
                    // send the telemetry
                    timer.Stop();
                    telemetryClient.TrackDependency("gRPC call", "IoTClient", "GetMetadataAzync", startTime, timer.Elapsed, false);
                }


            };
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public class MessageReceivedEventArgs : EventArgs
        {
            public IMessage Message { get; set; }
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
                handler?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception raised OnMessageReceived...");
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

        public Task Initiate(CancellationToken ct)
        {
            return Task.Run(() => {
                Timer timer = new Timer(async (_) =>
                {
                    for (int i = 0; i < NumMessagesEachGeneration; i++)
                    {
                        var msg = await this.dataSource.ReadMessageAsync();
                        OnMessageReceived(new MessageReceivedEventArgs()
                        {
                            Message = msg
                        });
                    }
                }, null, GenerateMessagesEvery, GenerateMessagesEvery);

                while (!ct.IsCancellationRequested){
                    Thread.Sleep(TimeSpan.FromSeconds(.250));
                }
            }, ct);
        }

    }
}