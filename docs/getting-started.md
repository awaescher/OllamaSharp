# Getting Started

The [OllamaSharp](https://github.com/awaescher/OllamaSharp) library provides complete 
coverage of the [Ollama](https://ollama.com/) API through simple, asynchronous
streaming interfaces. The library further adds convenience classes and functions
to simplify common use cases.

Getting started with OllamaSharp only requires a running Ollama server and a 
supported version of [.NET](https://dotnet.microsoft.com/en-us/download).

## Prerequisites

- [Ollama](https://ollama.com/)
- [.NET](https://dotnet.microsoft.com/en-us/download)

## Pulling Your First Model

You can't talk to Ollama without a model. To get started, you can pull a model
from the Ollama repository. The following code snippet demonstrates how to 
connect to an Ollama server and pull a model.

```csharp
using OllamaSharp;

// If you are running Ollama locally on the default port:
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// Pull the model, and print the status of the pull operation.
await foreach (var status in ollama.PullModelAsync("llama3.2-vision"))
    Console.WriteLine($"{status.Percent}% {status.Status}");

Console.WriteLine("Model pulled successfully.");
```

If everything goes well, you should see something like the following output:

```
100% pulling manifest
100% pulling 11f274007f09
100% pulling ece5e659647a
100% pulling 715415638c9c
100% pulling 0b4284c1f870
100% pulling fefc914e46e6
100% pulling fbd313562bb7
100% verifying sha256 digest
100% writing manifest
100% success
Model pulled successfully.
```

## Getting Serenaded by Llamas

Once you have a model, you can start conversing wih Ollama. The following code
snippet demonstrates how to connect to an Ollama server, load a model, and start
a conversation.


```csharp
using OllamaSharp;

var uri = new Uri("http://localhost:11434");
var model = "llama3.2-vision";

var ollama = new OllamaApiClient(uri, model);
    
var request = "Write a deep, beautiful song for me about AI and the future.";

await foreach (var stream in ollama.GenerateAsync(request))
    Console.Write(stream.Response);
```

If all went to plan, you should be swept off your feet by the smooth, dulcet tones
of the Ollama AI.