namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading;
    using Microsoft.OneWeek.Hack.Microids.Core;

    public class EnrichmentMessageRouter
    {
        private IIoTDeviceDataEnricher dataEnricher;
        private IDataSource dataSource;
        private IDataSink dataSink;

        public EnrichmentMessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher)
        {
            this.dataSource = dataSource;
            this.dataSink = dataSink;
            this.dataEnricher = dataEnricher;

            // handle messages as they arrive
            this.MessageReceived += (sender, e) =>
            {

                // get the device id
                var deviceId = e.Message.GetDeviceId();

                // get the metadata
                var metadata = this.dataEnricher.GetMetadata(deviceId);

                // enrich the message
                e.Message.EnrichMessage(metadata);

                // output the message
                this.dataSink.WriteMessage(e.Message);

            };
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public class MessageReceivedEventArgs : EventArgs
        {
            public Message Message { get; set; }
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
                string s = System.Environment.GetEnvironmentVariable("GENERATE_MESSAGES_EVERY");
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
                string s = System.Environment.GetEnvironmentVariable("NUM_MESSAGES_EACH_GENERATION");
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
            Timer timer = new Timer((_) =>
               {
                   for (int i = 0; i < NumMessagesEachGeneration; i++)
                   {
                       var msg = this.dataSource.ReadMessage();
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