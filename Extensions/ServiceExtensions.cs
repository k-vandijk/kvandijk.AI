using kvandijk.AI.Interfaces;
using kvandijk.AI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace kvandijk.AI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCompletionService(this IServiceCollection services)
    {
        services.AddScoped<ICompletionService, CompletionService>();
        return services;
    }
}