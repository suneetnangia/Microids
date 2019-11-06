namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using Grpc.Core;
    using IoTDevice;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Threading.Tasks;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {
        private IConfiguration config;
        private TelemetryClient telemetryClient;

        public IoTDeviceGrpcDataEnricher(IConfiguration config, TelemetryClient telemetryClient)
        {
            this.config = config;
            this.telemetryClient = telemetryClient;

            //CACHE_GRPC_ENDPOINT
            Channel channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            this.Client = new IoTDevice.IoTDeviceClient(channel);
        }

        private string CacheGrpcEndpoint
        {
            get
            {
                string s = config.GetValue<string>("CACHE_GRPC_ENDPOINT");
                if (string.IsNullOrEmpty(s)) return "localhost:5000";
                return s;
            }
        }

        private IoTDevice.IoTDeviceClient Client { get; set; }

        public Task<DeviceMetadata> GetMetadataAsync(string id)
        {
            var request = new DeviceInfo() { Id = id };

            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                return this.Client.GetMetadataAsync(request).ResponseAsync;
            }
            catch (Exception ex)
            {
                success = false;
                telemetryClient.TrackException(ex);
                throw new Exception("Operation went wrong", ex);
            }
            finally
            {
                timer.Stop();
                telemetryClient.TrackDependency("gRPC call", "IoTClient", "GetMetadataAzync", startTime, timer.Elapsed, success);
            }
        }
    }
}