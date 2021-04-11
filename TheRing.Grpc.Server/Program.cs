using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheRing.Example.Common;

namespace TheRing.Grpc.Server
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var something = SomethingService.Create();

            var builder = CreateHostBuilder(args, something.Service);
            var host = builder.Build();

            await host.StartAsync();

            var applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

            var cancellationToken = applicationLifetime == null
                ? CancellationToken.None
                : applicationLifetime.ApplicationStopping;

            await Task.Run(async () =>
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    something.Subscriber.Bar("Ciao");
                    await Task.Delay(3000, cancellationToken);
                }
            }, cancellationToken);

            await host.WaitForShutdownAsync(cancellationToken);
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
