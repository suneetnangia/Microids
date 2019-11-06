using System;

namespace Microsoft.OneWeek.Hack.Microids.IoTDevice.DeviceMetadata.Restful
{
    public enum DeviceCapability
    {
        None = 0,
        Temperature = 1,
        WindSpeed = 2,
        LuxLevel = 4,
        RotationSpeed = 8
    }
}