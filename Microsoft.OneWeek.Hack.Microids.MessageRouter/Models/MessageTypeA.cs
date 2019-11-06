namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;

    public class MessageTypeA : IMessage
    {
        private string Id { get; set; }
        private string Desc { get; set; }
        private DeviceCapability Capabilities { get; set; }

        public MessageTypeA() { }

        public MessageTypeA(string Id, string Desc)
        {
            this.Id = Id;
            this.Desc = Desc;
        }

        public void EnrichMessage(DeviceMetadata metadata)
        {
            if (metadata != null){
                this.Capabilities = metadata.Capability;
            }
        }

        public string GetDeviceId()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Desc: {Desc}, Capabilities: {Capabilities}";
        }
    }

}