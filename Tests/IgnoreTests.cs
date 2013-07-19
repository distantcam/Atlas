using System;
using NUnit.Framework;

[TestFixture]
public class IgnoreTests
{
    [Test]
    public void DoesNotMapPrivate()
    {
        var type = AssemblyWeaver.Assembly.GetType("Ignore");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreNotEqual("Invisible", result.Gate);
    }
}