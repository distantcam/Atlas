using System;
using System.Linq;
using Atlas;

public class MissingMatchesSource
{
    public string TheString { get; set; }
    public int TheNumber { get; set; }
}

public class MissingMatchesDestination
{
    public float TheNumber { get; set; }
}

public class MissingMatches
{
    public MissingMatchesDestination Map()
    {
        var source = new MissingMatchesSource { TheString = "Foo", TheNumber = 42 };
        return MapTo<MissingMatchesDestination>.From(source);
    }
}