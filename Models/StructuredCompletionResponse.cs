namespace kvandijk.AI.Models;

public class StructuredCompletionResponse<T> : ICompletionResponse where T : class
{
    public T Data { get; set; } = null!;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}