using System;
using System.Threading;

namespace TheRing.Common
{
    public class ThreadWorker<T>
    {
        private readonly Thread m_thread;
        private readonly CancellationTokenSource m_cancellationTokenSource;

        public ThreadWorker(Worker<T> worker)
        {
            m_cancellationTokenSource = new CancellationTokenSource();
            m_thread = new Thread(() => worker.Run(m_cancellationTokenSource.Token));
            m_thread.Name = $"Worker+{nameof(T)}";
            m_thread.Start();
        }

        public void Stop()
        {
            try
            {
                m_cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException) { }

            if (Thread.CurrentThread.ManagedThreadId != m_thread.ManagedThreadId)
                m_thread.Join();
        }
    }
}