namespace Microsoft.OneWeek.Hack.Microids.IoTDevice.DeviceMetadata.Restful
{
    public interface IDeviceMetadataRepository
    {
        DeviceMetadata GetMetadata(string deviceId);
    }
}