namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Hosting;
    using dotenv.net;

    class Program
    {
        static void Main(string[] args)
        {

            // load configuration
            DotEnv.Config(false);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               });
    }
}