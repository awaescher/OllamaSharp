# Getting Started

[OllamaSharp](https://github.com/awaescher/OllamaSharp) provides .NET bindings for the Ollama API, simplifying interactions with Ollama both locally and remotely. It provides asynchronous streaming, progress reporting and convenience classes and functions to simplify common use cases.

Getting started with OllamaSharp only requires a running Ollama server and a supported version of [.NET](https://dotnet.microsoft.com/en-us/download).

## Prerequisites

- [Ollama](https://ollama.com/)
- [.NET](https://dotnet.microsoft.com/en-us/download)

## Pulling a model

To use Ollama, you will need  to specify a large language model to talk with. You can download a model from the [Ollama model hub](https://ollama.com/models). Below is a code snippet illustrating how to connect to an Ollama server and pull a model from there:

```csharp
using OllamaSharp;

// if you are running Ollama locally on the default port:
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// pull the model, and print the status of the pull operation.
await foreach (var status in ollama.PullModelAsync("llama3.2-vision"))
    Console.WriteLine($"{status.Percent}% {status.Status}");

Console.WriteLine("Model pulled successfully.");
```

This should result in an output like this:

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

## Taking to a model

After obtaining a model, you can begin interacting with Ollama. The following code snippet demonstrates how to connect to an Ollama server, load a model, and initiate a conversation:

```csharp
using OllamaSharp;

var uri = new Uri("http://localhost:11434");
var model = "llama3.2-vision";

var ollama = new OllamaApiClient(uri, model);
    
var request = "Write a deep, beautiful song for me about AI and the future.";

await foreach (var stream in ollama.GenerateAsync(request))
    Console.Write(stream.Response);
```

The model's answer should be streamed directly into your Console window.