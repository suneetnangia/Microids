namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using Grpc.Core;
    using IoTDevice;
    using System.Threading.Tasks;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {
        public IoTDeviceGrpcDataEnricher()
        {
            //CACHE_GRPC_ENDPOINT
            Channel channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            this.Client = new IoTDevice.IoTDeviceClient(channel);
        }

        private IoTDevice.IoTDeviceClient Client { get; set; }

        public Task<DeviceMetadata> GetMetadataAsync(string id)
        {
            var request = new DeviceInfo() { Id = id };
            return this.Client.GetMetadataAsync(request).ResponseAsync;
        }
    }
}