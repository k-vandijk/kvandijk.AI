namespace ai_categorisation.Exceptions;

public class CompletionDeserializationException : Exception
{
    public CompletionDeserializationException(string message) : base(message)
    {
    }
    
    public CompletionDeserializationException()
    {
    }
}