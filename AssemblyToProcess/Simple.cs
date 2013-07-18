using System;
using System.Linq;
using Atlas;

public class SimpleSource
{
    public string TheString { get; set; }
}

public class SimpleDestination
{
    public string TheString { get; set; }
}

public class Simple
{
    public SimpleDestination Map(SimpleSource source)
    {
        return MapTo<SimpleDestination>.From(source);
    }
}