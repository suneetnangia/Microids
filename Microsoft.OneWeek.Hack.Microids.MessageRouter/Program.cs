namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyInjection;
    using dotenv.net;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microids.Common;

    public class Program
    {

        public const string AppInsightsConfigKey = "APPINSIGHTS_KEY";

        // TODO: Async 
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
                var telemetry = ConstructTelemetryClient(configuration);
                services.AddSingleton<TelemetryClient>(telemetry);
                services.AddSingleton<EnrichmentMessageRouter>();
                services.AddSingleton<IDataSource>(new TestGeneratorDataSource());
                var provider = services.BuildServiceProvider();
                services.AddSingleton<IDataSink>(new BlackHoleDataSink(provider.GetService<ILogger<BlackHoleDataSink>>()));
                services.AddSingleton<IIoTDeviceDataEnricher>(new IoTDeviceGrpcDataEnricher(telemetry, configuration, provider.GetService<ILogger<IoTDeviceGrpcDataEnricher>>()));
                services.AddSingleton<IConfiguration>(configuration);

                // Dispose method of ServiceProvider will dispose all disposable objects constructed by it as well.
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Get a new message router object.
                    var messagerouter = serviceProvider.GetService<EnrichmentMessageRouter>();

                    var telemetryClient = serviceProvider.GetService<TelemetryClient>();
                    telemetryClient.TrackTrace($"MessageRouter starting up on {Environment.MachineName}");

                    await messagerouter.Initiate(cts.Token);
                }
            }
        }

        private static TelemetryClient ConstructTelemetryClient(IConfiguration config)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = config.GetValue<string>(AppInsightsConfigKey);

            // TODO: Remove this before going to production
            Console.WriteLine($"InstrumentationKey={configuration.InstrumentationKey}");
            configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
            InitializeDependencyTracking(configuration);
            return new TelemetryClient(configuration);
        }

        private static DependencyTrackingTelemetryModule InitializeDependencyTracking(TelemetryConfiguration configuration)
        {
            var module = new DependencyTrackingTelemetryModule();

            // prevent Correlation Id to be sent to certain endpoints. You may add other domains as needed.
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.windows.net");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.chinacloudapi.cn");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.cloudapi.de");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.usgovcloudapi.net");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("localhost");
            module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("127.0.0.1");

            // enable known dependency tracking, note that in future versions, we will extend this list. 
            // please check default settings in https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/develop/Src/DependencyCollector/DependencyCollector/ApplicationInsights.config.install.xdt

            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
            module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");

            // initialize the module
            module.Initialize(configuration);

            return module;
        }
    }
}