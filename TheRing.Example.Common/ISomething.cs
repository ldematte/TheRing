using System;
using TheRing.Generators;

namespace TheRing
{
    [RingProtocol]
    public interface ISomething
    {
        void Foo(int x, double y);
        void Bar(string x);
        void Baz(ReadOnlyMemory<byte> payload);
    }
}
