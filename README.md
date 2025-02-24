<a href="https://www.nuget.org/packages/OllamaSharp"><img src="https://img.shields.io/nuget/v/OllamaSharp" alt="nuget version"></a>
<a href="https://www.nuget.org/packages/OllamaSharp"><img src="https://img.shields.io/nuget/dt/OllamaSharp.svg" alt="nuget downloads"></a>
<a href="https://awaescher.github.io/OllamaSharp"><img src="https://img.shields.io/badge/api_docs-8A2BE2" alt="Api docs"></a>
  
<p align="center">
  <img alt="ollama" height="200px" src="https://github.com/awaescher/OllamaSharp/blob/main/Ollama.png">
</p>

# OllamaSharp 🦙

OllamaSharp provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), simplifying interactions with Ollama both locally and remotely.

## Features

- **Ease of use:** Interact with Ollama in just a few lines of code.
- **Reliability**: Powering [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel/pull/7362), [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ollama) and [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
- **API coverage:** Covers every single Ollama API endpoint, including chats, embeddings, listing models, pulling and creating new models, and more.
- **Real-time streaming:** Stream responses directly to your application.
- **Progress reporting:** Real-time progress feedback on tasks like model pulling.
- **Tools engine:** [Sophisticated tool support with source generators](https://awaescher.github.io/OllamaSharp/docs/tool-support.html).
- **Multi modality:** Support for [vision models](https://ollama.com/blog/vision-models).

## Usage

OllamaSharp wraps each Ollama API endpoint in awaitable methods that fully support response streaming.

The following list shows a few simple code examples.

ℹ **Try our full featured [demo application](./demo) that's included in this repository**

### Initializing

```csharp
// set up the client
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// select a model which should be used for further operations
ollama.SelectedModel = "llama3.1:8b";
```

### Listing all models that are available locally

```csharp
var models = await ollama.ListLocalModelsAsync();
```

### Pulling a model and reporting progress

```csharp
await foreach (var status in ollama.PullModelAsync("llama3.1:405b"))
    Console.WriteLine($"{status.Percent}% {status.Status}");
```

### Generating a completion directly into the console

```csharp
await foreach (var stream in ollama.GenerateAsync("How are you today?"))
    Console.Write(stream.Response);
```

### Building interactive chats

```csharp
// messages including their roles and tool calls will automatically be tracked within the chat object
// and are accessible via the Messages property

var chat = new Chat(ollama);

while (true)
{
    var message = Console.ReadLine();
    await foreach (var answerToken in chat.SendAsync(message))
        Console.Write(answerToken);
}
```

## Usage with Microsoft.Extensions.AI

Microsoft built an abstraction library to streamline the usage of different AI providers. This is a really interesting concept if you plan to build apps that might use different providers, like ChatGPT, Claude and local models with Ollama.

I encourage you to read their accouncement [Introducing Microsoft.Extensions.AI Preview – Unified AI Building Blocks for .NET](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/).

OllamaSharp is the first full implementation of their `IChatClient` and `IEmbeddingGenerator` that makes it possible to use Ollama just like every other chat provider.

To do this, simply use the `OllamaApiClient` as `IChatClient` instead of `IOllamaApiClient`. 

```csharp
// install package Microsoft.Extensions.AI.Abstractions

private static IChatClient CreateChatClient(Arguments arguments)
{
  if (arguments.Provider.Equals("ollama", StringComparison.OrdinalIgnoreCase))
    return new OllamaApiClient(arguments.Uri, arguments.Model);
  else
    return new OpenAIChatClient(new OpenAI.OpenAIClient(arguments.ApiKey), arguments.Model); // ChatGPT or compatible
}
```

> [!NOTE]
> `IOllamaApiClient` provides many Ollama specific methods that `IChatClient` and `IEmbeddingGenerator` miss. Because these are abstractions, `IChatClient` and `IEmbeddingGenerator` will never implement the full Ollama API specification. However, `OllamaApiClient` implements three interfaces: the native `IOllamaApiClient` and Microsoft `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>` which allows you to cast it to any of these two interfaces as you need them at any time.

## Credits

The icon and name were reused from the amazing [Ollama project](https://github.com/jmorganca/ollama).

**I would like to thank all the contributors who take the time to improve OllamaSharp. First and foremost [mili-tan](https://github.com/mili-tan), who always keeps OllamaSharp in sync with the Ollama API. ❤**


