using System;

namespace TheRing.Generators
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class RingProtocolAttribute : Attribute
    {
        public string PropertyName { get; set; }
    }
}