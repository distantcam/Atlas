using System;
using System.Linq;
using Atlas;

public class ConversionsSource
{
    public int ConvertToString { get; set; }
    public int Primitive { get; set; }
    public int ToNull { get; set; }
}

public class ConversionsDestination
{
    public string ConvertToString { get; set; }
    public float Primitive { get; set; }
    public int? ToNull { get; set; }
}

public class Conversions
{
    public ConversionsDestination Map()
    {
        var source = new ConversionsSource { ConvertToString = 42, Primitive = 13, ToNull = 1337 };
        var destination = new ConversionsDestination();

        Mapper.Map(source, destination);

        return destination;
    }
}