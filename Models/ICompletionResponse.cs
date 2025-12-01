namespace kvandijk.AI.Models;

public interface ICompletionResponse
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}