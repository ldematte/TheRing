using System.Collections.Generic;

namespace TheRing.Common
{
    public static class SingleSubscriber 
    {
        private class SingleSubscriberImpl<T> : ISubscriberCollection<T>
        {
            private readonly T m_subscriber;
            internal SingleSubscriberImpl(T subscriber)
            {
                m_subscriber = subscriber;
            }

            public IEnumerable<T> Collection => Yield();

            private IEnumerable<T> Yield()
            {
                yield return m_subscriber;
            }
        }

        public static ISubscriberCollection<T> Create<T>(T subscriber)
        {
            return new SingleSubscriberImpl<T>(subscriber);
        }
    }
}