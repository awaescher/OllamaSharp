# Getting started

[OllamaSharp](https://github.com/awaescher/OllamaSharp) provides .NET bindings for the Ollama API, simplifying interactions with Ollama both locally and remotely. It provides asynchronous streaming, progress reporting and convenience classes and functions to simplify common use cases.

Getting started with OllamaSharp only requires a running Ollama server and a supported version of [.NET](https://dotnet.microsoft.com/en-us/download).

## Prerequisites

- [Ollama](https://ollama.com/)
- [.NET](https://dotnet.microsoft.com/en-us/download)

## Using Ollama with OllamaSharp

To use Ollama from your code base, you'll need to create and initialize an instance of the `OllamaApiClient`. This client wraps each Ollama API endpoint in awaitable methods that fully support response streaming.

``` csharp
// set up the client
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// select a model which should be used for further operations
ollama.SelectedModel = "llama3.1:8b";
```

Once your client is initialized, you can list local models, pull new models from the [Ollama model hub](https://ollama.com/models) and build interactive chats with them.


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

Note that `IOllamaApiClient` provides many Ollama specific methods that `IChatClient` and `IEmbeddingGenerator` miss.

Because these are abstractions, `IChatClient` and `IEmbeddingGenerator` will never implement the full Ollama API specification. However, `OllamaApiClient` implements three interfaces: the native `IOllamaApiClient` and Microsoft `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>` which allows you to cast it to any of these two interfaces as you need them at any time.
