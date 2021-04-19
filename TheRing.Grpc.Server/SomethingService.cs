using System;
using TheRing.Common.Grpc;
using TheRing.Common.Grpc.Server;
using TheRing.Example.Common;

namespace TheRing.Example.GrpcServer
{
    // TODO: this will be auto-generated
    public class SomethingService : IGrpcServerService<ISomething<string>>
    {
        public SomethingService(ISomething<string> subscriber, IServerServiceDefinition serviceDefinition)
        {
            ServiceDefinition = serviceDefinition;
            Subscriber = subscriber;
        }

        public IServerServiceDefinition ServiceDefinition { get; }
        public ISomething<string> Subscriber { get; }
    }

    public class SomethingServiceFactory: IGrpcServerServiceFactory<ISomething<string>>
    {
        public IGrpcServerService<ISomething<string>> Create()
        {
            var fooForwarder = new Forwarder<FooArgs>();
            var barForwarder = new Forwarder<BarArgs>();
            var bazForwarder = new Forwarder<BazArgs>();

            var subscriber = new ToArgsQueue(fooForwarder, barForwarder, bazForwarder);

            var builder = new ServerServiceDefinitionBuilder();
            GrpcServerHelpers.AddServerStreamingMethod(builder, GrpcServerHelpers.CreateServerStreamingMethod<FooArgs>(nameof(SomethingService), "Foo"), fooForwarder);
            GrpcServerHelpers.AddServerStreamingMethod(builder, GrpcServerHelpers.CreateServerStreamingMethod<BarArgs>(nameof(SomethingService), "Bar"), barForwarder);
            GrpcServerHelpers.AddServerStreamingMethod(builder, GrpcServerHelpers.CreateServerStreamingMethod<BazArgs>(nameof(SomethingService), "Baz"), bazForwarder);

            return new SomethingService(subscriber, builder.Build());
        }
        
        private class ToArgsQueue : ISomething<string>
        {
            private readonly Forwarder<FooArgs> m_fooChannel;
            private readonly Forwarder<BarArgs> m_barChannel;
            private readonly Forwarder<BazArgs> m_bazChannel;

            public ToArgsQueue(Forwarder<FooArgs> fooChannel, Forwarder<BarArgs> barChannel, Forwarder<BazArgs> bazChannel)
            {
                m_fooChannel = fooChannel;
                m_barChannel = barChannel;
                m_bazChannel = bazChannel;
            }

            public void Foo(int x, double y)
            {
                m_fooChannel.Forward(new FooArgs { x = x, y = y });
            }

            public void Bar(string x)
            {
                m_barChannel.Forward(new BarArgs { x = x });
            }

            public void Baz(ReadOnlyMemory<byte> x)
            {
                m_bazChannel.Forward(new BazArgs { x = x.ToArray() });
            }
        }
    }
}
