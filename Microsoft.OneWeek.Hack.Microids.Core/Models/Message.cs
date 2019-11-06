namespace Microsoft.OneWeek.Hack.Microids.Core
{
    public abstract class Message
    {

        public abstract string GetDeviceId();
        public abstract void EnrichMessage(DeviceMetadata metadata);

    }
}