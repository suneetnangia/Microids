namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using Grpc.Core;
    using IoTDevice;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {
        public IoTDeviceGrpcDataEnricher(TelemetryClient telemetry)
        {
            this.Telemetry = telemetry;
            Channel channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            this.Client = new IoTDevice.IoTDeviceClient(channel);
        }

        private TelemetryClient Telemetry;

        private static string CacheGrpcEndpoint
        {
            get
            {
                string s = System.Environment.GetEnvironmentVariable("CACHE_GRPC_ENDPOINT");
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
            int current = 0;
            while (true)
            {
                try
                {
                    var request = new DeviceInfo() { Id = id };
                    var response = await this.Client.GetMetadataAsync(request);
                    if (FailedToDispatch)
                    {
                        Telemetry.TrackEvent($"gRPC client connected successfully from {System.Environment.MachineName}.");
                        FailedToDispatch = false;
                    }
                    return response;
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine("gRPC client failed to connect...");
                        Telemetry.TrackEvent($"gRPC client failed to connect from {System.Environment.MachineName}.");
                        FailedToDispatch = true;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                current += 1000;
                if (current > CacheGrpcTimeout)
                {
                    Telemetry.TrackEvent($"gRPC client timed out from {System.Environment.MachineName}.");
                    throw new Exception("gRPC client timed out.");
                }
                await Task.Delay(1000);
            }
        }
    }
}