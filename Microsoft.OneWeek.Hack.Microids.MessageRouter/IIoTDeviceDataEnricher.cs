using Microsoft.OneWeek.Hack.Microids.IoTDevice;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public interface IIoTDeviceDataEnricher
    {
        string GetCanonicalId(string id);

        // TODO: Remove coupling with gRPC generated types.
        Metadata GetMetadata(string canonicalId);
    }
}