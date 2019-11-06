namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using IoTDevice;
    using System.Threading.Tasks;

    public interface IIoTDeviceDataEnricher
    {
        Task<DeviceMetadata> GetMetadataAsync(string id);
    }
}