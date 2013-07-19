using System;
using System.Linq;

namespace Atlas
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IgnoreMapAttribute : Attribute { }
}