using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TheRing.Common
{
    // Same process, same code, shared memory - shared data structure (both pub and sub)
    public class BlockingCollectionTaskQueue<T> : ITaskProducer<T>, ITaskConsumer<T>
    {
        private readonly BlockingCollection<Action<T>> m_queue = new BlockingCollection<Action<T>>();
        public void Add(Action<T> action)
        {
            try
            {
                m_queue.Add(action);
            }
            catch
            {
                // ignored
            }
        }

        public void Dequeue(CancellationToken stoppingToken, Action<Action<T>> visitor)
        {
            try
            {
                foreach (var x in m_queue.GetConsumingEnumerable(stoppingToken))
                {
                    visitor(x);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
