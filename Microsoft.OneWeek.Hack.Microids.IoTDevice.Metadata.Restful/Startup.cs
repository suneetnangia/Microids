using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
             // Mocked device repositories for testing gRPC performance.
            var mockedDeviceMetadataRepository = new Mock<IDeviceMetadataRepository>();
            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("001"))
                                    .Returns(new DeviceMetadata { Fqdn = "001.GB.London.Bld01", Capabilities = DeviceCapability.RotationSpeed | DeviceCapability.Temperature });

            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("002"))
                                    .Returns(new DeviceMetadata { Fqdn = "002.US.WA.Bld28", Capabilities = DeviceCapability.WindSpeed | DeviceCapability.RotationSpeed | DeviceCapability.Temperature });

            // Configure dependencies for the service.
            services.AddSingleton<IDeviceMetadataRepository>(mockedDeviceMetadataRepository.Object);
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
