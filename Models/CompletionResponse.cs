namespace kvandijk.AI.Models;

public class CompletionResponse
{
    public string Text { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
