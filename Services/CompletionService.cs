using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ai_categorisation.Exceptions;
using ai_categorisation.Interfaces;
using NJsonSchema;
using OpenAI;
using OpenAI.Chat;

namespace ai_categorisation.Services;

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

    public async Task<string> GetCompletionAsync(string prompt)
    {
        var completion = await _client.CompleteChatAsync(new[]
        {
            new UserChatMessage(prompt)
        });

        var raw = completion.Value.Content.FirstOrDefault()?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            throw new CompletionEmptyException("Received empty completion from model.");

        return raw;
    }

    public async IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var updates = _client.CompleteChatStreamingAsync(
        [
            new UserChatMessage(prompt)
        ]);

        await foreach (var update in updates.WithCancellation(cancellationToken))
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

    public async Task<T> GetStructuredCompletionAsync<T>(string prompt) where T : class
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

        var completion = await _client.CompleteChatAsync(
            [
                new SystemChatMessage("Follow the provided JSON schema exactly. Return ONLY minified JSON."),
                new UserChatMessage(prompt)
            ],
            options
        );

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

        return result;
    }
}