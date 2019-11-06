namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using Grpc.Core;
    using IoTDevice;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Configuration;
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

        private int cacheGrpcTimeout;

        private int CacheGrpcTimeout
        {
            get
            {
                if (cacheGrpcTimeout == 0)
                {
                    string s = System.Environment.GetEnvironmentVariable("CACHE_GRPC_TIMEOUT");
                    if (int.TryParse(s, out int i))
                    {
                        cacheGrpcTimeout = i;
                    }
                    else
                    {
                        cacheGrpcTimeout = 30000;
                    }
                }
                return cacheGrpcTimeout;
            }
        }

        private IoTDevice.IoTDeviceClient Client { get; set; }

        private bool FailedToDispatch { get; set; } = false;

        public async Task<DeviceMetadata> GetMetadataAsync(string id)
        {
            if (FailedToDispatch) return null; // drop new messages if there is a dispatch problem
            bool success = false;
            int current = 0;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    var request = new DeviceInfo() { Id = id };
                    var response = await this.Client.GetMetadataAsync(request);
                    if (FailedToDispatch)
                    {
                        telemetryClient.TrackEvent($"gRPC client connected successfully from {System.Environment.MachineName}.");
                        FailedToDispatch = false;
                    }
                    success = true;
                    return response;
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine("gRPC client failed to connect...");
                        telemetryClient.TrackEvent($"gRPC client failed to connect from {System.Environment.MachineName}.");
                        FailedToDispatch = true;
                    }
                    else
                    {
                        telemetryClient.TrackException(ex);
                        throw ex;
                    }
                }
                finally
                {
                    timer.Stop();
                    telemetryClient.TrackDependency("gRPC call", "IoTClient", "GetMetadataAzync", startTime, timer.Elapsed, success);
                }
                current += 1000;
                if (current > CacheGrpcTimeout)
                {
                    telemetryClient.TrackEvent($"gRPC client timed out from {System.Environment.MachineName}.");
                    throw new Exception("gRPC client timed out.");
                }
                await Task.Delay(1000);
            }
        }
    }
}