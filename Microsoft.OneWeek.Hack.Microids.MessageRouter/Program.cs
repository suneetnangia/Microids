namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Runtime.Loader;
    using Microsoft.Extensions.DependencyInjection;
    using dotenv.net;

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

                // Bootstrap services using dependency injection.
                var services = new ServiceCollection();
                services.AddSingleton<EnrichmentMessageRouter>();
                services.AddSingleton<IDataSource>(new TestGeneratorDataSource());
                services.AddSingleton<IDataSink>(new BlackHoleDataSink());
                services.AddSingleton<IIoTDeviceDataEnricher>(new IoTDeviceGrpcDataEnricher());

                // Dispose method of ServiceProvider will dispose all disposable objects constructed by it as well.
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Get a new message router object.
                    var messagerouter = serviceProvider.GetService<EnrichmentMessageRouter>();
                    messagerouter.Initiate(cts.Token);

                    await WhenCancelled(cts.Token);
                }
            }
        }

        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }
    }
}