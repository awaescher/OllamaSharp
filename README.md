# OllamaSharp 🦙

OllamaSharp is a .NET binding for the [Ollama API](https://github.com/jmorganca/ollama/blob/main/docs/api.md), making it easy to interact with Ollama using your favorite .NET languages.

## Features

- Intuitive API client: Set up and interact with Ollama in just a few lines of code.
- Support for various Ollama operations: Including streaming completions (chatting), listing local models, pulling new models, show model information, creating new models, copying models, deleting models, pushing models, and generating embeddings.
- Real-time streaming: Stream responses directly to your application.
- Progress reporting: Get real-time progress feedback on tasks like model pulling.

## Usage

Here's a simple example to get you started:

```csharp
// set up the client
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// select a model which should be used for further operations
ollama.SelectedModel = "llama2";

// list all local models
var models = await ollama.ListLocalModels();

// pull a model and report progress
await ollama.PullModel("mistral", status => Console.WriteLine($"({status.Percent}%) {status.Status}"));

// stream a completion and write to the console
// keep reusing the context to keep the chat topic going
ConversationContext context = null;
context = await ollama.StreamCompletion("How are you today?", context, stream => Console.Write(stream.Response));

// build an interactive full-featured chat with a few lines of code with the /chat api from Ollama 0.1.14
// messages including their roles will automatically be tracked within the chat object
var chat = ollama.Chat(stream => Console.WriteLine(stream.Message?.Content ?? ""));
while (true)
{
    var message = Console.ReadLine();
    await chat.Send(message);
}
```

## Credits

Icon and name were reused from the amazing [Ollama project](https://github.com/jmorganca/ollama).
