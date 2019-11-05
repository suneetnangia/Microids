namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    public interface IDeviceLookupRepository
    {
        string GetCanonicalId(string deviceId);
    }
}