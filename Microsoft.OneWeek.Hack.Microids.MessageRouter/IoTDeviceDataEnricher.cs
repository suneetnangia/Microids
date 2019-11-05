namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using Microsoft.OneWeek.Hack.Microids.IoTDevice;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {
        public IoTDeviceGrpcDataEnricher(Uri dataServiceUri)
        {       
        }

        public string GetCanonicalId(string id)
        {
            throw new System.NotImplementedException();
        }

        public Metadata GetMetadata(string canonicalId)
        {
            throw new System.NotImplementedException();
        }
    }
}