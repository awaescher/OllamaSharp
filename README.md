<div align="center">
 <img alt="ollama" height="200px" src="https://github.com/awaescher/OllamaSharp/blob/main/Ollama.png">
</div>

# OllamaSharp 🦙

OllamaSharp provides .NET bindings for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), simplifying interaction with Ollama both locally and remotely.

## Features

- Ease of use: Interact with Ollama in just a few lines of code.
- API endpoint coverage: Support for all Ollama API endpoints including chats, embeddings, listing models, pulling and creating new models, and more.
- Real-time streaming: Stream responses directly to your application.
- Progress reporting: Get real-time progress feedback on tasks like model pulling.
- Support for [vision models](https://ollama.com/blog/vision-models) and [tools (function calling)](https://ollama.com/blog/tool-support).

## Usage

OllamaSharp wraps every Ollama API endpoint in awaitable methods that fully support response streaming.

The following list shows a few examples to get a glimpse on how easy it is to use. The list is not complete.

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

Icon and name were reused from the amazing [Ollama project](https://github.com/jmorganca/ollama).

**I would like to thank all the contributors who take the time to improve OllamaSharp. First and foremost [mili-tan](https://github.com/mili-tan), who always keeps OllamaSharp in sync with the Ollama API. ❤**


