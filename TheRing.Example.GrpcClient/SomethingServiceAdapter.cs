using System.Threading;
using System.Threading.Tasks;
using TheRing.Common.Helpers;
using TheRing.Example.Common;

namespace TheRing.Example.GrpcClient
{
    internal class SomethingServiceAdapter
    {
        private readonly ISomething<string> m_observer;
        private readonly ISomethingService m_service;

        public SomethingServiceAdapter(ISomethingService service, ISomething<string> observer)
        {
            m_observer = observer;
            m_service = service;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await AsyncHelper.Demux(
                m_service.SubscribeBarAsync(cancellationToken), args => m_observer.Bar(args.x),
                m_service.SubscribeBazAsync(cancellationToken), args => m_observer.Baz(args.x),
                cancellationToken);
        }
    }
}