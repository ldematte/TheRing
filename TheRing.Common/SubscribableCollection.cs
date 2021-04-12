using System.Collections.Generic;
using System.Collections.Immutable;

namespace TheRing.Common
{
    public class SubscribableCollection<T> : ISubscriberCollection<T>
    {
        // Use an immutable list so it is thread safe
        private ImmutableList<T> m_subscribers = ImmutableList<T>.Empty;

        public IEnumerable<T> Collection => m_subscribers;

        public void Add(T subscriber)
        {
            m_subscribers = m_subscribers.Add(subscriber);
        }
    }
}