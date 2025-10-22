using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using kvandijk.AI.Exceptions;
using kvandijk.AI.Interfaces;
using NJsonSchema;
using OpenAI;
using OpenAI.Chat;

namespace kvandijk.AI.Services;

public class CompletionService : ICompletionService
{
    private readonly ChatClient _client;

    public CompletionService()
    {
        var url = Environment.GetEnvironmentVariable("OPENAI_URL") ?? throw new ArgumentNullException("OPENAI_URL");
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? throw new ArgumentNullException("OPENAI_KEY");
        var dep = Environment.GetEnvironmentVariable("OPENAI_DEP") ?? throw new ArgumentNullException("OPENAI_DEP");

        _client = new(
            credential: new ApiKeyCredential(key),
            model: dep,
            options: new OpenAIClientOptions
            {
                Endpoint = new($"{url}"),
            });
    }

    public async Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
    {
        ChatMessage[] messages = [new UserChatMessage(prompt)];
        return await GetCompletionAsync(messages, new ChatCompletionOptions(), ct);
    }

    public async Task<string> GetCompletionAsync(string prompt, IEnumerable<ChatMessage> chatHistory, CancellationToken ct = default)
    {
        ChatMessage[] messages = chatHistory.Append(new UserChatMessage(prompt)).ToArray();
        return await GetCompletionAsync(messages, new ChatCompletionOptions(), ct);
    }

    private async Task<string> GetCompletionAsync(ChatMessage[] messages, ChatCompletionOptions options, CancellationToken ct)
    {
        var completion = await _client.CompleteChatAsync(messages, options, cancellationToken: ct);

        var raw = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            throw new CompletionEmptyException("Received empty completion from model.");

        return raw;
    }

    public async IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ChatMessage[] messages = [new UserChatMessage(prompt)];
        await foreach (var chunk in GetCompletionStreamAsync(messages, new ChatCompletionOptions(), ct))
        {
            yield return chunk;
        }
    }

    public async IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, IEnumerable<ChatMessage> chatHistory, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ChatMessage[] messages = chatHistory.Append(new UserChatMessage(prompt)).ToArray();
        await foreach (var chunk in GetCompletionStreamAsync(messages, new ChatCompletionOptions(), ct))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<string> GetCompletionStreamAsync(ChatMessage[] messages, ChatCompletionOptions options, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var updates = _client.CompleteChatStreamingAsync(messages, options, cancellationToken: ct);

        await foreach (var update in updates.WithCancellation(ct))
        {
            // Each update may contain 0..n content deltas; yield the text deltas
            if (update.ContentUpdate.Count > 0)
            {
                var delta = update.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
            }
        }
    }

    public async Task<T> GetStructuredCompletionAsync<T>(string prompt, CancellationToken ct = default) where T : class
    {
        var schema = JsonSchema.FromType<T>();
        var schemaJson = schema.ToJson();

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: typeof(T).Name,
                jsonSchema: BinaryData.FromString(schemaJson),
                jsonSchemaIsStrict: false
            )
        };

        ChatMessage[] messages = [
            new SystemChatMessage("Follow the provided JSON schema exactly. Return ONLY minified JSON."),
            new UserChatMessage(prompt)
        ];
        
        var raw = await GetCompletionAsync(messages, options, ct);

        var result = JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        if (result is null)
            throw new CompletionDeserializationException($"Could not deserialize model output into {typeof(T).Name}. Raw: {raw}");

        return result;
    }
}