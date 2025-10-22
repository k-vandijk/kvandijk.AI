using ai_categorisation.Interfaces;
using ai_categorisation.Models;
using ai_categorisation.Services;
using ai_categorisation.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

DotenvLoader.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ICompletionService, CompletionService>();

using var host = builder.Build();

var service = host.Services.GetRequiredService<ICompletionService>();

// Example typed structured completion
var typed = await service.GetStructuredCompletionAsync<ClassificationResult>(""""
        Categorise: 'Invoice from Contoso for Q3 services'.
        Categories: invoice, receipt, contract, other. Include 1-3 short reasons.Return as JSON.
    """");
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(typed, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

// Example streaming completion
// await foreach (var chunk in service.GetCompletionStreamAsync("Write a short story of 100 words."))
// {
//     Console.Write(chunk); // stream to console / SignalR / HttpResponse.Body etc.
// }
// Console.WriteLine();