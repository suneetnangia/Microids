namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using System.Threading.Tasks;
    using Grpc.Core;

    public class IoTDeviceService : IoTDevice.IoTDeviceBase
    {
        private IDeviceMetadataRepository deviceMetadataRepository;

        public IoTDeviceService(IDeviceMetadataRepository deviceMetadataRepository)
        {
            this.deviceMetadataRepository = deviceMetadataRepository;
        }

        public override Task<Metadata> GetMetadata(DeviceInfo request, ServerCallContext context)
        {
            var deviceMetadata = this.deviceMetadataRepository.GetMetadata(request.Id);
            return Task.FromResult(new Metadata() { Capability = (int)deviceMetadata.Capabilities });
        }
    }
}