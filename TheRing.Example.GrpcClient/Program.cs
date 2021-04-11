using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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



    class Program
    {
        static async Task Main()
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using (var channel = GrpcChannel.ForAddress("http://localhost:10042"))
            {
                var somethingService = channel.CreateGrpcService<ISomethingService>();

                var x = new SomethingServiceAdapter(somethingService, new ConsoleDumper());

               
            }
        }

    }

    public static class AsyncHelpers
    {
        private static async Task<bool> Helper<T>(ValueTask<bool> valueTask, IAsyncEnumerator<T> enumerator, Action<T> action)
        {
            var b = await valueTask;
            if (b == false)
                return false;
            else
                action(enumerator.Current);
            return true;
        }

        public static async void Demux<T1, T2>(
            IAsyncEnumerable<T1> source1, Action<T1> source1Receiver,
            IAsyncEnumerable<T2> source2, Action<T2> source2Receiver,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var enumerator1 = source1.GetAsyncEnumerator(cancellationToken);
            var enumerator2 = source2.GetAsyncEnumerator(cancellationToken);
            try
            {
                var depletedEnumerators = 0;
                while (depletedEnumerators < 2)
                {
                    var task1 = enumerator1.MoveNextAsync();
                    if (task1.IsCompleted)
                    {
                        if (task1.Result == false)
                            ++depletedEnumerators;
                        else
                            source1Receiver(enumerator1.Current);
                        continue;
                    }

                    var task2 = enumerator2.MoveNextAsync();
                    if (task2.IsCompleted)
                    {
                        if (task2.Result == false)
                            ++depletedEnumerators;
                        else
                            source2Receiver(enumerator2.Current);
                        continue;
                    }


                    var tasks = new[] { Helper(task1, enumerator1, source1Receiver), Helper(task1, enumerator1, source1Receiver) };
                    var cont = await Task.WhenAny(tasks);
                    if (await cont == false)
                        ++depletedEnumerators;
                }
            }
            finally
            {
                await enumerator1.DisposeAsync();
                await enumerator2.DisposeAsync();
            }
        }
    }

    internal class SomethingServiceAdapter
    {
        public SomethingServiceAdapter(ISomethingService somethingService, ConsoleDumper consoleDumper)
        {
            somethingService.SubscribeBarAsync();
        }
    }
}
