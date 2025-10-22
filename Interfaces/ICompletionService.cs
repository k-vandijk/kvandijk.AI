namespace ai_categorisation.Interfaces;

public interface ICompletionService
{
    Task<string> GetCompletionAsync(string prompt);
    Task<T> GetStructuredCompletionAsync<T>(string prompt) where T : class;
}