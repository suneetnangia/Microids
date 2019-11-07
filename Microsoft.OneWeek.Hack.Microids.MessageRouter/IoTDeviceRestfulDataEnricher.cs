namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;
    using System.Threading.Tasks;
    using Microsoft.OneWeek.Hack.Microids.IoTDevice;

    public class IoTDeviceRestfulDataEnricher : IIoTDeviceDataEnricher
    {
        private IConfiguration config;
        private ITelemetryClient telemetryClient;
        private ILogger<IoTDeviceRestfulDataEnricher> logger;
        private IMetadataApi client;

        public IoTDeviceRestfulDataEnricher(ITelemetryClient telemetryClient, IConfiguration config, ILogger<IoTDeviceRestfulDataEnricher> logger)
        {
            this.telemetryClient = telemetryClient;
            this.config = config;
            this.logger = logger;

            client = RestService.For<IMetadataApi>(config.GetValue<string>("REST_SERVICE_URL"));
        }


        public Task<DeviceMetadata> GetMetadataAsync(string deviceId)
        {
            return client.GetMetadata(deviceId);
        }
    }

    public interface IMetadataApi
    {
        [Get("/metadata?deviceId={deviceId}")]
        Task<DeviceMetadata> GetMetadata(string deviceId);
    }
}
