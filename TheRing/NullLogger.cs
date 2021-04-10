using System;
using TheRing.Common;

namespace TheRing
{
    internal class NullLogger : IWorkerLogger
    {
        public void UnhandledSubscriberException(Exception ex)
        {
        }

        public void QueueException(Exception exception)
        {
        }
    }
}