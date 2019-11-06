namespace Microsoft.OneWeek.Hack.Microids.Core
{
    using System;

    [Flags]
    public enum DeviceCapability
    {
        None = 0,
        Temperature = 1,
        WindSpeed = 2,
        LuxLevel = 4,
        RotationSpeed = 8
    }
}