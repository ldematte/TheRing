using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using TheRing.Example.Common;

namespace TheRing.Grpc.Server
{
    public class X
    {
        private X(ISomething<string> subscriber, ISomethingService service)
        {
            Subscriber = subscriber;
            Service = service;
        }

        public ISomethingService Service { get; }
        public ISomething<string> Subscriber { get; }

        public static X Create()
        {
            var fooQueue = Channel.CreateUnbounded<FooArgs>();
            var barQueue = Channel.CreateUnbounded<BarArgs>();
            var bazQueue = Channel.CreateUnbounded<BazArgs>();

            var subscriber = new ToArgsQueue(fooQueue.Writer, barQueue.Writer, bazQueue.Writer);
            var service = new SomethingService(fooQueue.Reader, barQueue.Reader, bazQueue.Reader);

            return new X(subscriber, service);
        }
    }

    public class SomethingService : ISomethingService
    {
        private readonly ChannelReader<FooArgs> m_fooQueue;
        private readonly ChannelReader<BarArgs> m_barQueue;
        private readonly ChannelReader<BazArgs> m_bazQueue;

        internal SomethingService(ChannelReader<FooArgs> fooQueue, ChannelReader<BarArgs> barQueue, ChannelReader<BazArgs> bazQueue)
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
