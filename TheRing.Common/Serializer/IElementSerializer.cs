using System;

namespace TheRing.Common.Serializer
{
    public interface IElementSerializer
    {
    }

    public interface IElementSerializer<T> : IElementSerializer
    {
        int Serialize(T value, Span<byte> destination);
    }
}