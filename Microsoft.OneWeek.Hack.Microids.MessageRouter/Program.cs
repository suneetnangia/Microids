namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyInjection;
    using dotenv.net;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microids.Common;

    public class Program
    {

        public static async Task Main(string[] args)
        {
            // load configuration
            DotEnv.Config(false);

            // Wait until the app unloads or is cancelled by external triggers, use it for exceptional scnearios only.
            using (var cts = new CancellationTokenSource())
            {
                // Wait until the app unloads or is cancelled
                AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
                Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local.settings.json", true)
                    .AddEnvironmentVariables()
                    .Build();

                // Bootstrap services using dependency injection.
                var services = new ServiceCollection();
                services.AddLogging(configure =>
                {
                    configure.AddProvider(new SingleLineConsoleLoggerProvider());
                })
                .Configure<LoggerFilterOptions>(options =>
                {
                    string logLevel = configuration.GetValue<string>("LOG_LEVEL");
                    if (Enum.TryParse(logLevel, out Microsoft.Extensions.Logging.LogLevel level))
                    {
                        options.MinLevel = level;
                    }
                    else
                    {
                        options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                    }
                });
                ITelemetryClient telemetry = new AppInsightsTelemetryClient(configuration);
                services.AddSingleton<ITelemetryClient>(telemetry);
                services.AddSingleton<EnrichmentMessageRouter>();
                services.AddSingleton<IDataSource>(new TestGeneratorDataSource());
                var provider = services.BuildServiceProvider();
                services.AddSingleton<IDataSink>(new BlackHoleDataSink(provider.GetService<ILogger<BlackHoleDataSink>>()));

                var protocol = configuration.GetValue<string>("PROTOCOL");
                if (string.Compare(protocol, "GRPC", true) == 0)
                    services.AddSingleton<IIoTDeviceDataEnricher>(new IoTDeviceGrpcDataEnricher(telemetry, configuration, provider.GetService<ILogger<IoTDeviceGrpcDataEnricher>>()));
                else
                    services.AddSingleton<IIoTDeviceDataEnricher>(new IoTDeviceRestfulDataEnricher(telemetry, configuration, provider.GetService<ILogger<IoTDeviceRestfulDataEnricher>>()));

                services.AddSingleton<IConfiguration>(configuration);

                // Dispose method of ServiceProvider will dispose all disposable objects constructed by it as well.
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Get a new message router object.
                    var messagerouter = serviceProvider.GetService<EnrichmentMessageRouter>();
                    var telemetryClient = serviceProvider.GetService<ITelemetryClient>();
                    telemetryClient.TrackEvent($"MessageRouter starting up on {Environment.MachineName}");
                    await messagerouter.Initiate(cts.Token);
                }
            }
        }

    }
}