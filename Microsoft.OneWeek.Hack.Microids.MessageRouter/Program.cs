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

    public class Program
    {
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
                    .Build();

                // Bootstrap services using dependency injection.
                var services = new ServiceCollection();
                services.AddSingleton<EnrichmentMessageRouter>();
                services.AddSingleton<IDataSource>(new TestGeneratorDataSource());
                services.AddSingleton<IDataSink>(new BlackHoleDataSink());
                services.AddSingleton<IIoTDeviceDataEnricher>(new IoTDeviceGrpcDataEnricher());
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<TelemetryClient>(ConstructTelemetryClient(configuration));

                // Dispose method of ServiceProvider will dispose all disposable objects constructed by it as well.
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Get a new message router object.
                    var messagerouter = serviceProvider.GetService<EnrichmentMessageRouter>();

                    var httpClient = new System.Net.Http.HttpClient();
                    var resp = await httpClient.GetAsync("https://www.microsoft.com");
                    Console.WriteLine(await resp.Content.ReadAsStringAsync());

                    messagerouter.Initiate(cts.Token);
                    await WhenCancelled(cts.Token);
                }
            }
        }

        private static string AppInsightsKey
        {
            get
            {
                string s = System.Environment.GetEnvironmentVariable("APPINSIGHTS_KEY");
                return s;
            }
        }

        private static TelemetryClient ConstructTelemetryClient(IConfiguration config)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = AppInsightsKey;
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

        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }
    }
}