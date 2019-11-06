namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using System.Threading.Tasks;
    using Grpc.Core;

    public class IoTDeviceService : IoTDevice.IoTDeviceBase
    {
        private IDeviceMetadataRepository deviceMetadataRepository;

        public override Task<DeviceMetadata> GetMetadata(DeviceInfo request, ServerCallContext context)
        {
            var deviceMetadata = this.deviceMetadataRepository.GetMetadata(request.Id);
            return Task.FromResult(new DeviceMetadata() { Capability = deviceMetadata.Capability });
        }
    }
}