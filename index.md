---
_layout: landing
---

::::flex-row

:::col

![Ollama Logo](images/logo@0.1x.png) ➕ ![.NET Logo](images/dotnet@0.1x.png)

# Build AI-powered applications with Ollama and .NET 🦙

OllamaSharp provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), simplifying interactions with Ollama both locally and remotely.

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

// messages including their roles and tool calls will automatically
// be tracked within the chat object and are accessible via the Messages property
var chat = new Chat(ollama);
   
Console.WriteLine("You're now talking with Ollama. Hit Ctrl+C to exit.");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    Console.Write("Assistant: ");
    await foreach (var stream in chat.SendAsync(message))
        Console.Write(stream);

    Console.WriteLine("");
}
```

:::

::::