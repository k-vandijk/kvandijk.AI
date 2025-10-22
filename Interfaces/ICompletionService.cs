using OpenAI.Chat;

namespace ai_categorisation.Interfaces;

public interface ICompletionService
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default);
    Task<string> GetCompletionAsync(string prompt, IEnumerable<ChatMessage> chatHistory, CancellationToken ct = default);

    IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, CancellationToken ct = default);
    IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, IEnumerable<ChatMessage> chatHistory, CancellationToken ct = default);

    Task<T> GetStructuredCompletionAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}