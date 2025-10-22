namespace kvandijk.AI.Exceptions;

public class CompletionDeserializationException : Exception
{
    public CompletionDeserializationException(string message) : base(message)
    {
    }
    
    public CompletionDeserializationException()
    {
    }
}