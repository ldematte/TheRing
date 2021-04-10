using System;

namespace TheRing.Common
{
    public interface IWorkerLogger
    {
        void UnhandledSubscriberException(Exception ex);
        void QueueException(Exception exception);
    }
}