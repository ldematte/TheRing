using System.Threading;
using System.Threading.Tasks;

namespace TheRing.Common
{
    public static class TaskWorker
    {
        public static Task RunOnTask<T>(this Worker<T> worker, CancellationToken cancellationToken)
        {
            return Task.Run(() => worker.Run(cancellationToken), cancellationToken);
        }
    }
}