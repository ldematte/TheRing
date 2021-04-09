using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TheRing.Example.Common
{
    // void Foo(int x, double y);
    // void Bar(string x);
    // void Baz(ReadOnlyMemory<byte> payload);

    [DataContract]
    public class FooArgs
    {
        [DataMember(Order = 1)]
        public int x;
        [DataMember(Order = 2)]
        public double y;
    }

    [DataContract]
    public class BarArgs
    {
        [DataMember(Order = 1)]
        public string x;
    }

    [DataContract]
    public class BazArgs
    {
        [DataMember(Order = 1)]
        public byte[] x;
    }

    [ServiceContract]
    public interface ISomethingService
    {
        [OperationContract]
        IAsyncEnumerable<FooArgs> SubscribeFooAsync(CancellationToken cancellationToken = default);

        [OperationContract]
        IAsyncEnumerable<BarArgs> SubscribeBarAsync(CancellationToken cancellationToken = default);

        [OperationContract]
        IAsyncEnumerable<BazArgs> SubscribeBazAsync(CancellationToken cancellationToken = default);
    }

    public class ToArgsQueue : ISomething<string>
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
}
