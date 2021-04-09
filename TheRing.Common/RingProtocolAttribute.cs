using System;

namespace TheRing.Common
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class RingProtocolAttribute : Attribute
    {
    }
}