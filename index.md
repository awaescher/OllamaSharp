---
_layout: landing
---

::::flex-row

:::col

![Ollama Logo](images/logo@0.1x.png) ➕ ![.NET Logo](images/dotnet@0.1x.png)

# Build AI-powered applications with Ollama and .NET 🦙

*OllamaSharp* provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md),
simplifying interactions with Ollama both locally and remotely.

Provides support for interacting with Ollama directly, or through the [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
and [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel/pull/7362) libraries.
:::
:::col

### Add OllamaSharp to your project
```bash
dotnet add package OllamaSharp
```

### Start talking to Ollama
```csharp
using OllamaSharp;

var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri, "llama3.2");
var chat = new Chat(ollama);
   
Console.WriteLine("You're now talking with Ollama. Hit Ctrl+C to exit.");

while(true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    var resposne = await chat.SendAsync(input).StreamToEndAsnyc();
    Console.WriteLine($"Ollama: {response}");
}
```
:::

::::