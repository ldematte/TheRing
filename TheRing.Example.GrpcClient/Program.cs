using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using TheRing.Example.Common;

namespace TheRing.Example.GrpcClient
{
    class Program
    {
        static async Task Main()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using (var channel = GrpcChannel.ForAddress("http://localhost:10042"))
            {
                var calculator = channel.CreateGrpcService<ISomethingService>();
                await foreach (var x in calculator.SubscribeBarAsync())
                {
                    Console.WriteLine($"Bar {x.x}");
                }
            }
        }

    }
}
