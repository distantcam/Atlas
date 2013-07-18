using System;
using System.Linq;

[Serializable]
public class NonFatalException : Exception
{
    public NonFatalException()
    {
    }

    public NonFatalException(string message)
        : base(message)
    {
    }

    public NonFatalException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected NonFatalException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}