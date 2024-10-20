<p align="center">
 <img alt="ollama" height="200px" src="https://github.com/awaescher/OllamaSharp/blob/main/Ollama.png">
</p>

<p align="center">
  
  <!-- NuGet Downloads -->
  <a href="https://www.nuget.org/packages/OllamaSharp">
    <img src="https://img.shields.io/nuget/dt/OllamaSharp.svg" alt="NuGet Downloads">
  </a>
  
  <!-- License -->
  <a href="LICENSE">
    <img src="https://img.shields.io/github/license/awaescher/OllamaSharp.svg" alt="License">
  </a>
  
  <!-- GitHub Stars -->
  <a href="https://github.com/awaescher/OllamaSharp/stargazers">
    <img src="https://img.shields.io/github/stars/awaescher/OllamaSharp.svg" alt="GitHub Stars">
  </a>
  
  <!-- GitHub Issues -->
  <a href="https://github.com/awaescher/OllamaSharp/issues">
    <img src="https://img.shields.io/github/issues/awaescher/OllamaSharp.svg" alt="GitHub Issues">
  </a>
  
  <!-- GitHub Last Commit -->
  <a href="https://github.com/awaescher/OllamaSharp/commits/main">
    <img src="https://img.shields.io/github/last-commit/awaescher/OllamaSharp.svg" alt="GitHub Last Commit">
  </a>
  
  <!-- .NET Version -->
  <a href="#">
    <img src="https://img.shields.io/badge/.NET-6.0%2B-512BD4" alt=".NET Version">
  </a>
  
  <!-- Contributors -->
  <a href="https://github.com/awaescher/OllamaSharp/graphs/contributors">
    <img src="https://img.shields.io/github/contributors/awaescher/OllamaSharp.svg" alt="Contributors">
  </a>
  
</p>

# OllamaSharp 🦙

OllamaSharp provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), simplifying interactions with Ollama both locally and remotely.

## Features

- Ease of use: Interact with Ollama in just a few lines of code.
- API endpoint coverage: Support for all the Ollama API endpoints, including chats, embeddings, listing models, pulling and creating new models, and more.
- Real-time streaming: Stream responses directly to your application.
- Progress reporting: Get real-time progress feedback on tasks like model pulling.
- Support for [vision models](https://ollama.com/blog/vision-models) and [tools (function calling)](https://ollama.com/blog/tool-support).

## Usage

OllamaSharp wraps each Ollama API endpoint in awaitable methods that fully support response streaming.

The following list shows a few simple code examples.

ℹ **Try our full-featured Ollama API client app [OllamaSharpConsole](https://github.com/awaescher/OllamaSharpConsole) to interact with your Ollama instance.**

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
var models = await ollama.ListLocalModels();
```

### Pulling a model and reporting progress

```csharp
await foreach (var status in ollama.PullModel("llama3.1:405b"))
    Console.WriteLine($"{status.Percent}% {status.Status}");
```

### Generating a completion directly into the console

```csharp
await foreach (var stream in ollama.Generate("How are you today?"))
    Console.Write(stream.Response);
```

### Building interactive chats

```csharp
var chat = new Chat(ollama);
while (true)
{
    var message = Console.ReadLine();
    await foreach (var answerToken in chat.Send(message))
        Console.Write(answerToken);
}
// messages including their roles and tool calls will automatically be tracked within the chat object
// and are accessible via the Messages property
```

## Credits

The icon and name were reused from the amazing [Ollama project](https://github.com/jmorganca/ollama).

**I would like to thank all the contributors who take the time to improve OllamaSharp. First and foremost [mili-tan](https://github.com/mili-tan), who always keeps OllamaSharp in sync with the Ollama API. ❤**


