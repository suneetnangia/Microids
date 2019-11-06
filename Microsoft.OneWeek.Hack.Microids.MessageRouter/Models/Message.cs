namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    public abstract class Message
    {

        public abstract string GetDeviceId();
        public abstract void EnrichMessage(DeviceMetadata metadata);

    }
}