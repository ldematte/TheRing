using System.Threading;
using TheRing.Common;

namespace TheRing
{
    class Service<T>
    {
        private readonly ITaskConsumer<T> m_taskQueue;
        private readonly ISubscriberCollection<T> m_subscribers;

        public Service(ITaskConsumer<T> taskQueue, ISubscriberCollection<T> subscribers)
        {
            m_taskQueue = taskQueue;
            m_subscribers = subscribers;
        }

        public void Run(CancellationToken cancellationToken)
        {
            m_taskQueue.Dequeue(cancellationToken, action =>
            {
                // TODO: inner and outer try catch
                // TODO: visit instead of foreach?
                foreach (var x in m_subscribers.Collection)
                {
                    action(x);
                }
            });
        }

        // Run in separate thread/task/whatever

    }
}