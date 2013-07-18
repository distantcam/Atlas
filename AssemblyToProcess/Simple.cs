using System;
using System.Linq;
using Atlas;

public class SimpleSource
{
    public string TheString { get; set; }
    public int TheNumber { get; set; }
}

public class SimpleDestination
{
    public string TheString { get; set; }
    public int TheNumber { get; set; }
}

public class Simple
{
    public SimpleDestination Map()
    {
        var source = new SimpleSource { TheString = "Foo", TheNumber = 42 };
        return MapTo<SimpleDestination>.From(source);
    }
}