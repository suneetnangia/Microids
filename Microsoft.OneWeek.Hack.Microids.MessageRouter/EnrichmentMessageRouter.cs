namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading;
    using Microsoft.Extensions.Configuration;

    public class EnrichmentMessageRouter
    {
        private IIoTDeviceDataEnricher dataEnricher;
        private IDataSource dataSource;
        private IDataSink dataSink;

        private IConfiguration config;

        public EnrichmentMessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher, IConfiguration config)
        {
            this.dataSource = dataSource;
            this.dataSink = dataSink;
            this.dataEnricher = dataEnricher;

            this.config = config;

            // handle messages as they arrive
            this.MessageReceived += async (sender, e) =>
            {

                // get the device id
                var deviceId = e.Message.GetDeviceId();

                // get the metadata
                var metadata = await this.dataEnricher.GetMetadataAsync(deviceId);
                if (metadata != null)
                {

                    // enrich the message
                    e.Message.EnrichMessage(metadata);

                    // output the message
                    await this.dataSink.WriteMessageAsync(e.Message);

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
                Console.WriteLine("Exception raised OnMessageReceived...");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
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

        public void Initiate(CancellationToken ct)
        {
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
            while (!ct.IsCancellationRequested)
            {
                // let the timer run
            }
            timer.Change(0, 0);
            timer.Dispose();
        }

    }
}