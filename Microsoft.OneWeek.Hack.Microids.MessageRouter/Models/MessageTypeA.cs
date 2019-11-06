namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    public class MessageTypeA : Message
    {

        public string Id { get; set; }
        public string Desc { get; set; }
        public DeviceCapability Capabilities { get; set; }

        public override void EnrichMessage(DeviceMetadata metadata)
        {
            this.Capabilities = metadata.Capability;
        }

        public override string GetDeviceId()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Desc: {Desc}, Capabilities: {Capabilities}";
        }
    }

}