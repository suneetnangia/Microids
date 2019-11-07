namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using Grpc.Core;
    using IoTDevice;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;

    // Enricher to call gRPC based data service.
    // Refactor the gRPC client sample code here.
    // TODO: Implement IDisposable pattern.
    public class IoTDeviceGrpcDataEnricher : IIoTDeviceDataEnricher
    {

        private IConfiguration config;
        private ITelemetryClient telemetryClient;
        private ILogger<IoTDeviceGrpcDataEnricher> logger;

        public IoTDeviceGrpcDataEnricher(ITelemetryClient telemetryClient, IConfiguration config, ILogger<IoTDeviceGrpcDataEnricher> logger)
        {
            this.telemetryClient = telemetryClient;
            this.config = config;
            this.logger = logger;

            Channel channel = new Channel(CacheGrpcEndpoint, ChannelCredentials.Insecure);
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
                    string s = config.GetValue<string>("CACHE_GRPC_TIMEOUT");
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
                        logger.LogDebug("gRPC client connected successfully.");
                        telemetryClient.TrackEvent($"gRPC client connected successfully from {System.Environment.MachineName}.");
                        FailedToDispatch = false;
                    }
                    return response;
                }
                catch (RpcException ex)
                {
                    if (ex.StatusCode == StatusCode.Unavailable)
                    {
                        logger.LogWarning("gRPC client failed to connect...");
                        telemetryClient.TrackEvent($"gRPC client failed to connect from {System.Environment.MachineName}.");
                        FailedToDispatch = true;
                    }
                    else
                    {
                        telemetryClient.TrackException(ex);
                        throw ex;
                    }
                }
                current += 1000;
                if (current > CacheGrpcTimeout)
                {
                    logger.LogError("gRPC client timed out.");
                    telemetryClient.TrackEvent($"gRPC client timed out from {System.Environment.MachineName}.");
                    throw new Exception("gRPC client timed out.");
                }
                await Task.Delay(1000);
            }
        }

        /*
                public override async Task RouteChat(
                    IAsyncStreamReader<DeviceInfo> requestStream,
                    IServerStreamWriter<DeviceMetadata> responseStream,
                    Grpc.Core.ServerCallContext context)
                {
                    while (await requestStream.MoveNext())
                    {
                        var note = requestStream.Current;
                        List<RouteNote> prevNotes = AddNoteForLocation(note.Location, note);
                        foreach (var prevNote in prevNotes)
                        {
                            await responseStream.WriteAsync(prevNote);
                        }
                    }
                }
         */
    }
}