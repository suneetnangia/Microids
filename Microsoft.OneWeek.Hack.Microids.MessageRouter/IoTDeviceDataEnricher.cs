namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {
        public IoTDeviceGrpcDataEnricher()
        {
        }

        public DeviceMetadata GetMetadata(string id)
        {
            // TODO: use real method
            return new DeviceMetadata() { Capability = DeviceCapability.Temperature };
        }
    }
}