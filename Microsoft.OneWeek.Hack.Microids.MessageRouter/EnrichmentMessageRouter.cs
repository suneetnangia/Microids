namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System.Threading;
    using Microsoft.OneWeek.Hack.Microids.IoTDevice;

    // TODO: Potentially can be optimised using generics.
    public abstract class EnrichmentMessageRouter
    {
        private IIoTDeviceDataEnricher dataEnricher;
        private IDataSource dataSource;
        private IDataSink dataSink;

        public EnrichmentMessageRouter(IDataSource dataSource, IDataSink dataSink, IIoTDeviceDataEnricher dataEnricher)
        {
            this.dataSource = dataSource;
            this.dataSink = dataSink;
            this.dataEnricher = dataEnricher;
        }

        public void ReadMessage()
        {
            var message = this.dataSource.ReadMessage();
            var deviceId = this.ReturnDeviceId(message);
            var deviceCanonicalId = this.dataEnricher.GetCanonicalId(deviceId);
            var deviceMetada = this.dataEnricher.GetMetadata(deviceCanonicalId);

            var enrichedMessage = this.EnrichMessage(message, deviceCanonicalId, deviceMetada);
            this.dataSink.WriteMessage(enrichedMessage);
        }

        protected abstract string ReturnDeviceId(string message);
        
        protected abstract string EnrichMessage(string message, string deviceId, Metadata deviceMetada);

        public abstract void Initiate(CancellationToken ct);
    }
}