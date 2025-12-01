using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using kvandijk.AI.Exceptions;
using kvandijk.AI.Interfaces;
using kvandijk.AI.Models;
using NJsonSchema;
using OpenAI;
using OpenAI.Chat;

namespace kvandijk.AI.Services;

public class CompletionService : ICompletionService
{
    private readonly ChatClient _client;

    public CompletionService()
    {
        var url = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new MissingEnvironmentVariableException("OPENAI_ENDPOINT");
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? throw new MissingEnvironmentVariableException("OPENAI_KEY");
        var dep = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT") ?? throw new MissingEnvironmentVariableException("OPENAI_DEPLOYMENT");

        _client = new(
            credential: new ApiKeyCredential(key),
            model: dep,
            options: new OpenAIClientOptions
            {
                Endpoint = new($"{url}"),
            });
    }

    public async Task<CompletionResponse> GetCompletionAsync(string prompt, CancellationToken ct = default)
    {
        ChatMessage[] messages = [new UserChatMessage(prompt)];
        return await GetCompletionAsync(messages, new ChatCompletionOptions(), ct);
    }

    public async Task<CompletionResponse> GetCompletionAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default)
    {
        return await GetCompletionAsync(messages, new ChatCompletionOptions(), ct);
    }

    private async Task<CompletionResponse> GetCompletionAsync(IEnumerable<ChatMessage> messages, ChatCompletionOptions options, CancellationToken ct)
    {
        var completion = await _client.CompleteChatAsync(messages, options, cancellationToken: ct);

        var inputTokens = completion.Value.Usage.InputTokenCount;
        var outputTokens = completion.Value.Usage.OutputTokenCount;

        var raw = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            throw new CompletionEmptyException("Received empty completion from model.");

        return new CompletionResponse
        {
            Text = raw,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
    }

    public async IAsyncEnumerable<CompletionResponse> GetCompletionStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ChatMessage[] messages = [new UserChatMessage(prompt)];
        await foreach (var chunk in GetCompletionStreamAsync(messages, new ChatCompletionOptions(), ct))
        {
            yield return chunk;
        }
    }

    public async IAsyncEnumerable<CompletionResponse> GetCompletionStreamAsync(IEnumerable<ChatMessage> messages, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var chunk in GetCompletionStreamAsync(messages, new ChatCompletionOptions(), ct))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<CompletionResponse> GetCompletionStreamAsync(IEnumerable<ChatMessage> messages, ChatCompletionOptions options, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var updates = _client.CompleteChatStreamingAsync(messages, options, cancellationToken: ct);

        int inputTokens = 0;
        int outputTokens = 0;

        await foreach (var update in updates)
        {
            if (update.Usage is not null)
            {
                inputTokens = update.Usage.InputTokenCount;
                outputTokens = update.Usage.OutputTokenCount;
            }

            if (update.ContentUpdate.Count > 0)
            {
                var delta = update.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(delta))
                {
                    yield return new CompletionResponse
                    {
                        Text = delta,
                        InputTokens = inputTokens,
                        OutputTokens = outputTokens
                    };
                }
            }
        }
    }

    public async Task<StructuredCompletionResponse<T>> GetStructuredCompletionAsync<T>(string prompt, CancellationToken ct = default) where T : class
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

        var completion = await _client.CompleteChatAsync(messages, options, cancellationToken: ct);

        var inputTokens = completion.Value.Usage.InputTokenCount;
        var outputTokens = completion.Value.Usage.OutputTokenCount;

        var raw = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            throw new CompletionEmptyException("Received empty completion from model.");

        var result = JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        if (result is null)
            throw new CompletionDeserializationException($"Could not deserialize model output into {typeof(T).Name}. Raw: {raw}");

        return new StructuredCompletionResponse<T>
        {
            Data = result,
            InputTokens = inputTokens,
            OutputTokens = outputTokens
        };
    }
}