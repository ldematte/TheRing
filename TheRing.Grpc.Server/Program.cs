using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheRing.Example.Common;

namespace TheRing.Example.GrpcServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            await host.StartAsync();

            var applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

            var cancellationToken = applicationLifetime == null
                ? CancellationToken.None
                : applicationLifetime.ApplicationStopping;

            var something = host.Services.GetRequiredService<ISomething<string>>();

            await Task.Run(async () =>
            {
                var i = 0;
                while (cancellationToken.IsCancellationRequested == false)
                {
                    something.Bar($"Ciao {i++}");
                    await Task.Delay(3000, cancellationToken);
                }
            }, cancellationToken);

            await host.WaitForShutdownAsync(cancellationToken);
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(10042, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                })
                .UseStartup<Startup>();
        }
    }
}
