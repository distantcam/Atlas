using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class SimpleTests
{
    [Test]
    public void Test()
    {
        var type = AssemblyWeaver.Assembly.GetType("Simple");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map(new SimpleSource { TheString = "Foo" }) as SimpleDestination;

        result.ShouldNotBe(null);
        result.TheString.ShouldBe("Foo");
    }
}