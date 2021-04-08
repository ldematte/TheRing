using System.Collections.Generic;

namespace TheRing.Common
{
    public interface ISubscriberCollection<out T>
    {
        IEnumerable<T> Collection { get; }
    }
}