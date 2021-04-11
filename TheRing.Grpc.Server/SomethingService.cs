using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using TheRing.Example.Common;

namespace TheRing.Grpc.Server
{
    public class SomethingService
    {
        private SomethingService(ISomething<string> subscriber, ISomethingService service)
        {
            Subscriber = subscriber;
            Service = service;
        }

        public ISomethingService Service { get; }
        public ISomething<string> Subscriber { get; }

        public static SomethingService Create()
        {
            var fooQueue = Channel.CreateUnbounded<FooArgs>();
            var barQueue = Channel.CreateUnbounded<BarArgs>();
            var bazQueue = Channel.CreateUnbounded<BazArgs>();

            var subscriber = new ToArgsQueue(fooQueue.Writer, barQueue.Writer, bazQueue.Writer);
            var service = new SomethingServiceImpl(fooQueue.Reader, barQueue.Reader, bazQueue.Reader);

            return new SomethingService(subscriber, service);
        }
        
        private class ToArgsQueue : ISomething<string>
        {
            private readonly ChannelWriter<FooArgs> m_fooChannel;
            private readonly ChannelWriter<BarArgs> m_barChannel;
            private readonly ChannelWriter<BazArgs> m_bazChannel;

            public ToArgsQueue(ChannelWriter<FooArgs> fooChannel, ChannelWriter<BarArgs> barChannel, ChannelWriter<BazArgs> bazChannel)
            {
                m_fooChannel = fooChannel;
                m_barChannel = barChannel;
                m_bazChannel = bazChannel;
            }

            public void Foo(int x, double y)
            {
                m_fooChannel.TryWrite(new FooArgs { x = x, y = y });
            }

            public void Bar(string x)
            {
                m_barChannel.TryWrite(new BarArgs { x = x });
            }

            public void Baz(ReadOnlyMemory<byte> x)
            {
                m_bazChannel.TryWrite(new BazArgs { x = x.ToArray() });
            }
        }

        private class SomethingServiceImpl : ISomethingService
        {
            private readonly ChannelReader<FooArgs> m_fooQueue;
            private readonly ChannelReader<BarArgs> m_barQueue;
            private readonly ChannelReader<BazArgs> m_bazQueue;

            internal SomethingServiceImpl(ChannelReader<FooArgs> fooQueue, ChannelReader<BarArgs> barQueue,
                ChannelReader<BazArgs> bazQueue)
            {
                m_fooQueue = fooQueue;
                m_barQueue = barQueue;
                m_bazQueue = bazQueue;
            }

            public IAsyncEnumerable<FooArgs> SubscribeFooAsync(CancellationToken cancel)
            {
                return ServiceHelper.SubscribeTAsync<FooArgs, ChannelReader<FooArgs>>(m_fooQueue, cancel);
            }

            public IAsyncEnumerable<BarArgs> SubscribeBarAsync(CancellationToken cancel)
            {
                return ServiceHelper.SubscribeTAsync<BarArgs, ChannelReader<BarArgs>>(m_barQueue, cancel);
            }

            public IAsyncEnumerable<BazArgs> SubscribeBazAsync(CancellationToken cancel)
            {
                return ServiceHelper.SubscribeTAsync<BazArgs, ChannelReader<BazArgs>>(m_bazQueue, cancel);
            }
        }
    }
}
