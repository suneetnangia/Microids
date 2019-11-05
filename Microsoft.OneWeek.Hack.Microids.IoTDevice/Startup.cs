namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Mocked device repositories for testing gRPC performance.

            var mockedDeviceLookupRepository = new Mock<IDeviceLookupRepository>();
            mockedDeviceLookupRepository.Setup(device => device.GetCanonicalId("001"))
                                    .Returns("001.GB.London.Bld01");

            mockedDeviceLookupRepository.Setup(device => device.GetCanonicalId("002"))
                                    .Returns("002.US.WA.Bld28");

            var mockedDeviceMetadataRepository = new Mock<IDeviceMetadataRepository>();
            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("001.GB.London.Bld01"))
                                    .Returns(new DeviceMetadata{ Fqdn = "001.GB.London.Bld01", Capabilities = DeviceCapability.RotationSpeed | DeviceCapability.Temperature } );

            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("002.US.WA.Bld28"))
                                    .Returns(new DeviceMetadata{ Fqdn = "002.US.WA.Bld28", Capabilities = DeviceCapability.WindSpeed | DeviceCapability.RotationSpeed | DeviceCapability.Temperature } );

            // Configure dependencies for the service.
            services.AddGrpc();
            services.AddSingleton<IDeviceLookupRepository>(mockedDeviceLookupRepository.Object);
            services.AddSingleton<IDeviceMetadataRepository>(mockedDeviceMetadataRepository.Object);
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