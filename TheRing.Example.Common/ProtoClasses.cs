using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;

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
}
