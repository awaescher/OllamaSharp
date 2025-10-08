[![nuget version](https://img.shields.io/nuget/v/OllamaSharp)](https://www.nuget.org/packages/OllamaSharp)
[![nuget downloads](https://img.shields.io/nuget/dt/OllamaSharp.svg)](https://www.nuget.org/packages/OllamaSharp)
[![Api docs](https://img.shields.io/badge/api_docs-8A2BE2)](https://awaescher.github.io/OllamaSharp)

# OllamaSharp 🦙

OllamaSharp provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), simplifying interactions with Ollama both locally and remotely.

**🏆 [Recommended by Microsoft](https://www.nuget.org/packages/Microsoft.Extensions.AI.Ollama/)**

## Features

- **Ease of use:** Interact with Ollama in just a few lines of code.
- **Reliability**: Powering [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel/pull/7362), [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ollama) and [Microsoft.Extensions.AI](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
- **API coverage:** Covers every single Ollama API endpoint, including chats, embeddings, listing models, pulling and creating new models, and more.
- **Real-time streaming:** Stream responses directly to your application.
- **Progress reporting:** Real-time progress feedback on tasks like model pulling.
- **Tools engine:** [Sophisticated tool support with source generators](https://awaescher.github.io/OllamaSharp/docs/tool-support.html).
- **Multi modality:** Support for [vision models](https://ollama.com/blog/vision-models).
- **Native AOT support:** [Opt-in support for Native AOT](https://awaescher.github.io/OllamaSharp/docs/native-aot-support.html) for improved performance.

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
ollama.SelectedModel = "qwen3:4b";
```

### Native AOT Support

For .NET Native AOT scenarios, create a custom JsonSerializerContext with your types and pass it into the constructor.

```csharp
[JsonSerializable(typeof(MyCustomType))]
public partial class MyJsonContext : JsonSerializerContext { }

// Use the static factory method for NativeAOT
var ollama = new OllamaApiClient(uri, "qwen3:4b", MyJsonContext.Default);
```

See the [Native AOT documentation](./docs/native-aot-support.md) for detailed guidance.

### Listing all models that are available locally

```csharp
var models = await ollama.ListLocalModelsAsync();
```

### Pulling a model and reporting progress

```csharp
await foreach (var status in ollama.PullModelAsync("qwen3:32b"))
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

The `OllamaApiClient` implements both interfaces from Microsoft.Extensions.AI, you just need to cast it accordingly:
 - `IChatClient` for model inference
 - `IEmbeddingGenerator<string, Embedding<float>>` for embedding generation

## Cloud models aka Ollama Turbo

OllamaSharp can be used with [Ollama cloud models](https://ollama.com/cloud) as well. Use the constructor that takes an `HttpClient` and set it up to send the api key as default request header.

```csharp
var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:11434");
client.DefaultRequestHeaders.Add(/* your api key here */);
var ollama = new OllamaApiClient(client);
```

## OllamaSharp vs. Microsoft.Extensions.AI vs. Semantic Kernel

It can be confusing which library to use with AI in C#. The following paragraph should help you decide which library to start with.

Prefer OllamaSharp if ...
 - you plan to use Ollama models only
 - you want to use the native Ollama API, not only chats and embeddings but model management, usage information and more

Prefer Microsoft.Extensions.AI if ...
 - you only need chat and embedding functionality
 - you want to be able to use different providers like Ollama, OpenAI, Hugging Face, etc.

Prefer Semantic Kernel if ...
 - you need the highest flexibility with different providers, plugins, middlewares, caching, memory and more
 - you need advanced prompt techniques like variable substitution and templating
 - you want to build agentic systems

No matter which one you choose, OllamaSharp should always be the bridge to Ollama behind the scenes as recommended by Microsoft [(1)](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai) [(2)](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/chat-local-model) [(3)](https://devblogs.microsoft.com/dotnet/gpt-oss-csharp-ollama/).

## Thanks

**I would like to thank all the contributors who take the time to improve OllamaSharp. First and foremost [mili-tan](https://github.com/mili-tan), who always keeps OllamaSharp in sync with the Ollama API.**

The icon and name were reused from the amazing [Ollama project](https://github.com/jmorganca/ollama).

