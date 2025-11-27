using kvandijk.AI.Interfaces;
using kvandijk.AI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace kvandijk.AI.Extensions;

public static class EmbeddingServiceExtensions
{
    public static IServiceCollection AddEmbeddingService(this IServiceCollection services)
    {
        services.AddScoped<IEmbeddingService, EmbeddingService>();
        return services;
    }
}