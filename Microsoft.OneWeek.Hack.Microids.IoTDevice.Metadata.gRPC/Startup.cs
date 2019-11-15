namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OneWeek.Hack.Microids.Common;

    public class Startup
    {
        private static string LogLevel
        {
            get
            {
                return Environment.GetEnvironmentVariable("LOG_LEVEL");
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure dependencies for the service.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            ITelemetryClient telemetry = new AppInsightsTelemetryClient(configuration);
            services.AddSingleton<ITelemetryClient>(telemetry);
            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddProvider(new SingleLineConsoleLoggerProvider());
            })
            .Configure<LoggerFilterOptions>(options =>
            {
                if (Enum.TryParse(LogLevel, out Microsoft.Extensions.Logging.LogLevel level))
                {
                    options.MinLevel = level;
                }
                else
                {
                    options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                }
            });

            services.AddApplicationInsightsTelemetry(configuration.GetValue<string>("APPINSIGHTS_KEY"));
            services.AddGrpc();
            services.AddSingleton<IDeviceMetadataRepository, FakeMetadataRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<IoTDeviceService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}