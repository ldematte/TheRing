using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TheRing.Common.Helpers
{
    public static class AsyncHelper
    {
        private static async Task<bool> CreateTaskContinuation<T>(ValueTask<bool> valueTask, IAsyncEnumerator<T> enumerator, Action<T> action)
        {
            var b = await valueTask;
            if (b == false)
                return false;
            else
                action(enumerator.Current);
            return true;
        }

        public static async Task Demux<T1, T2>(
            IAsyncEnumerable<T1> source1, Action<T1> source1Receiver,
            IAsyncEnumerable<T2> source2, Action<T2> source2Receiver,
            CancellationToken cancellationToken = default)
        {
            var enumerator1 = source1.GetAsyncEnumerator(cancellationToken);
            var enumerator2 = source2.GetAsyncEnumerator(cancellationToken);
            try
            {
                var tasksToWait = new Task<bool>[2];
                var depletedEnumerators = 0;
                while (depletedEnumerators < 2)
                {
                    if (TryConsumeNext(source1Receiver, 0, tasksToWait, enumerator1))
                        ++depletedEnumerators;

                    if (TryConsumeNext(source2Receiver, 1, tasksToWait, enumerator2))
                        ++depletedEnumerators;
                    
                    if (NeedsWait(tasksToWait))
                    {
                        var completedTask = await Task.WhenAny(tasksToWait);
                        if (await completedTask == false)
                        {
                            ++depletedEnumerators;
                        }
                        // Remove from ToWait
                        RemoveFromWaitList(completedTask, tasksToWait);
                    }
                }
            }
            finally
            {
                try { await enumerator1.DisposeAsync(); } catch { /*suppressed*/ }
                try { await enumerator2.DisposeAsync(); } catch { /*suppressed*/ }
            }
        }

        private static bool NeedsWait(Task<bool>[] tasksToWait)
        {
            for (int i = 0; i < tasksToWait.Length; ++i)
            {
                if (tasksToWait[i] == null)
                    return false;
            }

            return true;
        }

        private static bool TryConsumeNext<T>(Action<T> receiver, int streamIndex, Task<bool>[] tasksToWait, IAsyncEnumerator<T> enumerator)
        {
            bool depleted = false;
            if (tasksToWait[streamIndex] == null)
            {
                var task = enumerator.MoveNextAsync();
                if (task.IsCompleted)
                {
                    if (task.Result == false)
                    {
                        depleted = true;
                    }
                    else
                    {
                        receiver(enumerator.Current);
                    }
                }
                else
                {
                    tasksToWait[streamIndex] = CreateTaskContinuation(task, enumerator, receiver);
                }
            }

            return depleted;
        }

        private static void RemoveFromWaitList(Task<bool> completedTask, Task<bool>[] tasks)
        {
            for (int i = 0; i < tasks.Length; ++i)
            {
                if (tasks[i] != null && completedTask.Id == tasks[i].Id)
                {
                    var current = tasks[i];
                    current.Dispose();
                    tasks[i] = null;
                    return;
                }
            }

            throw new InvalidOperationException("Task not present in list");
        }
    }
}