
A utilities and services package for OpenAI in .NET (streaming, structured, etc.). 

# Installing  the package

Install the package using the following command:

```bash
dotnet add package kvandijk.AI
```

# Setup

## Environment variables

Create a deployment in Azure OpenAI and set the following environment variables in your development environment or hosting platform:

- `OPENAI_ENDPOINT`: Your OpenAI endpoint URL.
- `OPENAI_DEPLOYMENT`: Your deployment name for the OpenAI model.
- `OPENAI_EMBEDDING_DEPLOYMENT`: Your deployment name for the OpenAI embedding model.
- `OPENAI_KEY`: Your OpenAI API key.

## Program.cs

Add the following line to your `Program.cs` file to register the completion service:

```cs
builder.Services.AddCompletionService();
```

# How to use

## Example streaming completion

```cs
await foreach (var chunk in service.GetCompletionStreamAsync("Write a short story of 100 words."))
{
	Console.Write(chunk);
}
```

## Example structured completion
```cs
var typed = await service.GetStructuredCompletionAsync<ClassificationResult>(""""
		Categorise: 'Invoice from Contoso for Q3 services'.
		Categories: invoice, receipt, contract, other. Include 1-3 short reasons.Return as JSON.
	"""");

Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(typed, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
```
