namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using System.Threading.Tasks;
    using Grpc.Core;

    public class IoTDeviceService : IoTDevice.IoTDeviceBase
    {
        private IDeviceMetadataRepository deviceMetadataRepository;
        private IDeviceLookupRepository deviceLookupRepository;

        public IoTDeviceService(IDeviceLookupRepository deviceLookupRepository, IDeviceMetadataRepository deviceMetadataRepository)
        {
            this.deviceLookupRepository = deviceLookupRepository;
            this.deviceMetadataRepository = deviceMetadataRepository;
        }

        public override Task<CanonicalId> GetCanonicalId(DeviceInfo request, ServerCallContext context)
        {
            var canonicalId = this.deviceLookupRepository.GetCanonicalId(request.Id);
            return Task.FromResult(new CanonicalId{Id = canonicalId } );
        }

        public override Task<Metadata> GetMetadata(DeviceInfo request, ServerCallContext context)
        {
            var deviceMetadata = this.deviceMetadataRepository.GetMetadata(request.Id);
            return Task.FromResult(new Metadata() { Capability = (int) deviceMetadata.Capabilities });
        }
    }
}