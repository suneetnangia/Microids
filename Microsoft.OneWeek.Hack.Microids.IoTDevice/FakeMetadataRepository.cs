using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    public class FakeMetadataRepository : IDeviceMetadataRepository
    {
        private Dictionary<string, DeviceMetadata> dictionary;

        public FakeMetadataRepository()
        {
            dictionary = new Dictionary<string, DeviceMetadata>()
            {
                {"001", new DeviceMetadata { Fqdn = "001.GB.London.Bld01", Capability = DeviceCapability.RotationSpeed } },
                {"002", new DeviceMetadata { Fqdn = "002.US.WA.Bld28", Capability = DeviceCapability.WindSpeed } }
            };

        }
        public DeviceMetadata GetMetadata(string deviceId)
        {
            if (dictionary.ContainsKey(deviceId))
            {
                return dictionary[deviceId];
            }
            else
            {
                return null;
            }
        }
    }
}
