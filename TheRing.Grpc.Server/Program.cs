using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using TheRing.Common;
using TheRing.Example.Common;

namespace TheRing.Grpc.Server
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            //var queue = new BlockingCollectionTaskQueue<ISomething>();
            var something = X.Create();
            //var somethingService = new SomethingService(queue);


            var builder = CreateHostBuilder(args, something.Service);
            var host = builder.Build();

            await host.StartAsync();

            Task.Run(async () =>
            {
                while (true)
                {
                    //queue.Add(x => x.Bar("Ciao"));
                    something.Subscriber.Bar("Ciao");
                    await Task.Delay(3000);
                }
            });

            //somethingService.Run(CancellationToken.None);

            await host.WaitForShutdownAsync();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args, ISomethingService somethingService)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ISomethingService>(somethingService);
                })
                .ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(10042, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                })
                .UseStartup<Startup>();
        }
    }
}
