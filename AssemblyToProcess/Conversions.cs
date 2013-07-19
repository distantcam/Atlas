using System;
using System.Linq;
using Atlas;

public class ConversionsSource
{
    public int ConvertToString { get; set; }
}

public class ConversionsDestination
{
    public string ConvertToString { get; set; }
}

public class Conversions
{
    private void Example(ConversionsSource source, ConversionsDestination destination)
    {
        destination.ConvertToString = source.ConvertToString.ToString();
    }

    public ConversionsDestination Map()
    {
        var source = new ConversionsSource { ConvertToString = 42 };
        var destination = new ConversionsDestination();

        Mapper.Map(source, destination);

        return destination;
    }
}