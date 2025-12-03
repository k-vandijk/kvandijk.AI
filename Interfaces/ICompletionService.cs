using kvandijk.AI.Models;
using OpenAI.Chat;

namespace kvandijk.AI.Interfaces;

public interface ICompletionService
{
    Task<CompletionResponse> GetCompletionAsync(string prompt, CancellationToken ct = default);
    Task<CompletionResponse> GetCompletionAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default);

    IAsyncEnumerable<CompletionResponse> GetCompletionStreamAsync(string prompt, CancellationToken ct = default);
    IAsyncEnumerable<CompletionResponse> GetCompletionStreamAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default);

    Task<StructuredCompletionResponse<T>> GetStructuredCompletionAsync<T>(string prompt, CancellationToken ct = default) where T : class;
    Task<StructuredCompletionResponse<T>> GetStructuredCompletionAsync<T>(IEnumerable<ChatMessage> messages, CancellationToken ct = default) where T : class;
}