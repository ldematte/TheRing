using System;
using System.Threading;
using TheRing.Common;
using TheRing.Example.Common;

namespace TheRing.Example.Inproc
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
        static void Main()
        {
            var queue = new BlockingCollectionTaskQueue<ISomething<string>>();

            var service = new Worker<ISomething<string>>(queue, SingleSubscriber.Create(new ConsoleDumper()), 
                new NullLogger());

            service.RunOnTask(CancellationToken.None);

            var y = queue.ToAsync();

            while (true)
            {
                var s = Console.ReadLine();
                y.Bar(s);
            }

        }
    }
}
