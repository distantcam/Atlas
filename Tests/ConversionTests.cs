using System;
using NUnit.Framework;

[TestFixture]
public class ConversionTests
{
    [Test]
    public void UsesToString()
    {
        var type = AssemblyWeaver.Assembly.GetType("Conversions");
        var instance = (dynamic)Activator.CreateInstance(type);

        var result = instance.Map();

        Assert.AreEqual("42", result.ConvertToString);
    }
}