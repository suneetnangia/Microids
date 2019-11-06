namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    public interface IIoTDeviceDataEnricher
    {
        DeviceMetadata GetMetadata(string id);
    }
}