namespace Microsoft.OneWeek.Hack.Microids.IoTDevice.DeviceMetadata.Restful
{
    public class DeviceMetadata
    {
        public string Id { get; set; }
        public string Fqdn { get; set; }
        public DeviceCapability Capability { get; set; }
    }
}