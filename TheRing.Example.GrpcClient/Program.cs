using System;
using System.Collections.Generic;
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


            //while (true)
            //{
            //    try
            //    {
            //        await Foo(AsyncInt(), AsyncInt());
            //    }
            //    catch (MyException e)
            //    {
            //        Console.WriteLine(e);
            //    }

            //    await Task.Delay(2000);
            //}
        }

        static async Task Foo(IAsyncEnumerable<int> enumerable1, IAsyncEnumerable<int> enumerable2)
        {
            var enumerator1 = enumerable1.GetAsyncEnumerator();
            var enumerator2 = enumerable2.GetAsyncEnumerator();
            try
            {
                var task1 = enumerator1.MoveNextAsync();
                var task2 = enumerator2.MoveNextAsync();

                Task.WaitAny(new[] {Helper(task1), Helper(task2)});
            }
            finally
            {
                await enumerator1.DisposeAsync();
                await enumerator2.DisposeAsync();
            }
        }

        private static async Task Helper(ValueTask<bool> task1)
        {
            await task1;
            
        }

        private class MyException: Exception {}

        static async IAsyncEnumerable<int> AsyncInt()
        {
            await Task.Delay(5000);
            throw new MyException();
            yield break;
        }
    }
}
