namespace Microsoft.OneWeek.Hack.Microids.IoTDevice
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using dotenv.net;

    class Program
    {
        static void Main(string[] args)
        {

            // load configuration
            DotEnv.Config(false);

            CreateHostBuilder(args).Build().Run();
        }

        private static int Port
        {
            get
            {
                string s = System.Environment.GetEnvironmentVariable("PORT");
                if (int.TryParse(s, out int i))
                {
                    return i;
                }
                else
                {
                    return 5000;
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder
                        .ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(Port, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                       .UseStartup<Startup>();
               });
    }
}