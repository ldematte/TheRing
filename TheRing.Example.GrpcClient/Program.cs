using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using TheRing.Example.Common;

namespace TheRing.Example.GrpcClient
{

    class ConsoleDumper : ISomething<string>
    {
        public void Foo(int x, double y)
        {
            Console.WriteLine($"Foo {x} {y}");
        }

        public void Bar(string x)
        {
            Console.WriteLine($"Bar {x}");
        }

        public void Baz(ReadOnlyMemory<byte> payload)
        {
            Console.WriteLine($"Baz {payload.Length}");
        }
    }

    static class Program
    {
        static async Task Main()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var channel = GrpcChannel.ForAddress("http://localhost:10042");
            var somethingService = channel.CreateGrpcService<ISomethingService>();

            var x = new SomethingServiceAdapter(somethingService, new ConsoleDumper());

            await x.Start(CancellationToken.None);
        }
    }
}
