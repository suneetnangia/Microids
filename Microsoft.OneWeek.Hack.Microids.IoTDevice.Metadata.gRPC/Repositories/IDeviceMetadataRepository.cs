namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    public interface IDeviceMetadataRepository
    {
        DeviceMetadata GetMetadata(string deviceId);
    }
}