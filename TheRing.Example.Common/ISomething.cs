using System;
using TheRing.Common;
using TheRing.Generators;

namespace TheRing
{
    public interface IBase<T>
    {
        void Foo(T x, double y);
    }

    [RingProtocol]
    public interface ISomething<T>: IBase<int> where T: IComparable
    //public interface ISomething: IBase<int> 
    {
        void Bar(string x);
        void Baz(ReadOnlyMemory<byte> payload);
    }
}
