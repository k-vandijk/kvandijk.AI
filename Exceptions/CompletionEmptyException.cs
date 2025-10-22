namespace ai_categorisation.Exceptions;

public class CompletionEmptyException : Exception
{
    public CompletionEmptyException(string message) : base(message)
    {
    }

    public CompletionEmptyException()
    {
    }
}