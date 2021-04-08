using System;
using System.Threading;

namespace TheRing.Common
{
    public interface ITaskConsumer<in T>
    {
        void Dequeue(CancellationToken stoppingToken, Action<Action<T>> visitor);
    }
}