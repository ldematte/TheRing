using System;
using System.Threading;

namespace TheRing.Common
{
    public class Worker<T>
    {
        private readonly ITaskConsumer<T> m_taskQueue;
        private readonly ISubscriberCollection<T> m_subscribers;
        private readonly IWorkerLogger m_logger;

        public Worker(ITaskConsumer<T> taskQueue, ISubscriberCollection<T> subscribers, IWorkerLogger logger)
        {
            m_taskQueue = taskQueue;
            m_subscribers = subscribers;
            m_logger = logger;
        }

        public void Run(CancellationToken cancellationToken)
        {
            try
            {
                m_taskQueue.Dequeue(cancellationToken, action =>
                {

                    foreach (var x in m_subscribers.Collection)
                    {
                        try
                        {
                            action(x);
                        }
                        catch (Exception ex)
                        {
                            m_logger.UnhandledSubscriberException(ex);
                        }
                    }
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                m_logger.QueueException(ex);
            }
        }
    }
}