using OpenAI.Chat;

namespace kvandijk.AI.Interfaces;

public interface ICompletionService
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default);
    Task<string> GetCompletionAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default);

    IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, CancellationToken ct = default);
    IAsyncEnumerable<string> GetCompletionStreamAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default);

    Task<T> GetStructuredCompletionAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}