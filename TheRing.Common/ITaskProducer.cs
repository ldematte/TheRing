using System;

namespace TheRing.Common
{
    public interface ITaskProducer<out T>
    {
        void Add(Action<T> action);
    }
}