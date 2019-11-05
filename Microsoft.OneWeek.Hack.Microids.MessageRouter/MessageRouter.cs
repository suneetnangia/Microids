namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System.Threading;
    using Microsoft.OneWeek.Hack.Microids.IoTDevice;

    public class MessageRouter : EnrichmentMessageRouter
    {
        public MessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher) 
            : base(dataSource, dataSink, dataEnricher)
        {
        }

        public override void Initiate(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                // Reads next available message from IDataSource and process it.
                this.ReadMessage();
            }
        }

        protected override string EnrichMessage(string message, string deviceId, Metadata deviceMetadata)
        {
            // TODO: create enriched message from the parameters provided and return for destination sink.
            throw new System.NotImplementedException();
        }

        protected override string ReturnDeviceId(string message)
        {
            // TODO: extract device Id from the message and return.
            throw new System.NotImplementedException();
        }
    }
}