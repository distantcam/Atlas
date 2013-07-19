using System;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class SimpleTests
{
    [Test]
    public void PropertyToPropertyMap()
    {
        var type = AssemblyWeaver.Assembly.GetType("Simple");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreEqual("Foo", result.TheString);
        Assert.AreEqual(42, result.TheNumber);
        Assert.AreEqual("OfDreams", result.AField);
    }

    [Test]
    public void FieldToFieldMap()
    {
        var type = AssemblyWeaver.Assembly.GetType("Simple");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreEqual("OfDreams", result.AField);
    }

    [Test]
    public void PropertyToFieldMap()
    {
        var type = AssemblyWeaver.Assembly.GetType("Simple");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreEqual("PTF", result.PropertyToField);
    }

    [Test]
    public void FieldToPropertyMap()
    {
        var type = AssemblyWeaver.Assembly.GetType("Simple");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreEqual("FTP", result.FieldToProperty);
    }
}