using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace TheRing.Common.Helpers
{
    public static class ServiceHelper
    {
        public static async IAsyncEnumerable<T> SubscribeTAsync<T, TChannel>(TChannel channel, [EnumeratorCancellation] CancellationToken cancel)
            where TChannel: ChannelReader<T>
        {
            while (cancel.IsCancellationRequested == false)
            {
                T x = default;
                bool success = false;
                try
                {
                    x = await channel.ReadAsync(cancel);
                    success = true;
                }
                catch (OperationCanceledException) { }
                if (success)
                    yield return x;
                else
                    yield break;
            }
            
        }
    }
}