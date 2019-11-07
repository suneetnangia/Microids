using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class AppInsightsTelemetryClient : ITelemetryClient
    {

        public AppInsightsTelemetryClient(IConfiguration config)
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = config.GetValue<string>("APPINSIGHTS_KEY");

            // TODO: Remove this before going to production
            Console.WriteLine($"InstrumentationKey={configuration.InstrumentationKey}");
            configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
            InitializeDependencyTracking(configuration);
            this.telemetryClient = new TelemetryClient(configuration);

        }

        private TelemetryClient telemetryClient;

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

        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTime start, TimeSpan elapsed, bool success)
        {
            this.telemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, start, elapsed, success);
        }

        public void TrackEvent(string message)
        {
            this.telemetryClient.TrackEvent(message);
        }

        public void TrackException(Exception ex)
        {
            this.telemetryClient.TrackException(ex);
        }
    }
}