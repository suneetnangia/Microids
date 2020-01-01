using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OneWeek.Hack.Microids.Common;
using Moq;

namespace Microsoft.OneWeek.Hack.Microids.IoTDevice.DeviceMetadata.Restful
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure dependencies for the service.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            ITelemetryClient telemetry = new AppInsightsTelemetryClient(configuration);

            // Configure dependencies for the service.
            services.AddApplicationInsightsTelemetry(configuration.GetValue<string>("APPINSIGHTS_KEY"));
            services.AddSingleton<IDeviceMetadataRepository, FakeMetadataRepository>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
