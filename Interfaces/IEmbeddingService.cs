namespace kvandijk.AI.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string input, CancellationToken ct = default);
}