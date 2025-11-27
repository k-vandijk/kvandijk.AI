using System.ClientModel;
using kvandijk.AI.Exceptions;
using kvandijk.AI.Interfaces;
using OpenAI;

namespace kvandijk.AI.Services;

public class EmbeddingService : IEmbeddingService
{
    public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken ct = default)
    {
        var key = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? throw new MissingEnvironmentVariableException("OPENAI_KEY");
        var url = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new MissingEnvironmentVariableException("OPENAI_ENDPOINT");
        var dep = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_DEPLOYMENT") ?? throw new MissingEnvironmentVariableException("OPENAI_EMBEDDING_DEPLOYMENT");

        var client = new OpenAIClient(
            credential: new ApiKeyCredential(key),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(url)
            }
        );

        var embeddingClient = client.GetEmbeddingClient(dep);

        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(input, cancellationToken: ct);

        var vector = embeddingResult.Value.ToFloats();

        return vector.ToArray();
    }
}