namespace ai_categorisation.Interfaces;

public interface ICompletionService
{
    Task<string> GetCompletionAsync(string prompt);
    IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, CancellationToken cancellationToken = default);
    Task<T> GetStructuredCompletionAsync<T>(string prompt) where T : class;
}