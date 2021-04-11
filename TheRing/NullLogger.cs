using System;
using TheRing.Common;

namespace TheRing.Example.Inproc
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