using System;
using System.Linq;
using Atlas;

public class IgnoreSource
{
    [IgnoreMap]
    public string TheString { get; set; }
    public int TheNumber { get; set; }

    private string privateField;
    [IgnoreMap]
    public string Gate
    {
        get { return privateField; }
        set { privateField = value; }
    }
}

public class IgnoreDestination
{
    public int TheNumber { get; set; }

    private string privateField;
    [IgnoreMap]
    public string Gate
    {
        get { return privateField; }
        set { privateField = value; }
    }
}

public class Ignore
{
    public IgnoreDestination Map()
    {
        var source = new IgnoreSource { TheString = "Foo", TheNumber = 42, Gate = "Invisible" };
        var destination = new IgnoreDestination();

        Mapper.Map(source, destination);

        return destination;
    }
}