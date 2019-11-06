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

            var mockedDeviceMetadataRepository = new Mock<IDeviceMetadataRepository>();
            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("001"))
                                    .Returns(new DeviceMetadata { Fqdn = "001.GB.London.Bld01", Capability = DeviceCapability.RotationSpeed });

            mockedDeviceMetadataRepository.Setup(device => device.GetMetadata("002"))
                                    .Returns(new DeviceMetadata { Fqdn = "002.US.WA.Bld28", Capability = DeviceCapability.WindSpeed });

            // Configure dependencies for the service.
            services.AddGrpc();
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