namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    public interface IMessage
    {
        string GetDeviceId();
        void EnrichMessage(DeviceMetadata metadata);

    }
}