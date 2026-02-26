# Model Management

`OllamaApiClient` exposes the full Ollama model-management API. All operations are available on the `IOllamaApiClient` interface and have convenient extension-method overloads for common cases.

## Listing local models

```csharp
var ollama = new OllamaApiClient("http://localhost:11434");

IEnumerable<Model> models = await ollama.ListLocalModelsAsync();

foreach (var model in models.OrderBy(m => m.Name))
    Console.WriteLine(model.Name);
```

## Listing running models

Query which models are currently loaded in memory:

```csharp
IEnumerable<RunningModel> running = await ollama.ListRunningModelsAsync();

foreach (var model in running)
    Console.WriteLine($"{model.Name} — expires {model.ExpiresAt}");
```

## Pulling a model

Pull a model from the [Ollama model hub](https://ollama.com/models) and report download progress:

```csharp
await foreach (var status in ollama.PullModelAsync("llama3.2"))
    Console.WriteLine($"{status.Percent:0}%  {status.Status}");
```

`PullModelAsync` streams `PullModelResponse` objects, each containing a `Status` string and a `Percent` value (0–100).

## Pushing a model

Push a locally created model to a registry (requires a valid Ollama account):

```csharp
await foreach (var status in ollama.PushModelAsync("myuser/my-custom-model:latest"))
    Console.WriteLine(status?.Status);
```

## Copying a model

Create a local copy of a model under a new name:

```csharp
await ollama.CopyModelAsync("llama3.2", "llama3.2-backup");
```

## Deleting a model

```csharp
await ollama.DeleteModelAsync("llama3.2-backup");
```

## Showing model information

Retrieve detailed metadata for a locally available model, including its Modelfile, parameters and template:

```csharp
ShowModelResponse info = await ollama.ShowModelAsync("llama3.2");

Console.WriteLine(info.ModelInfo);
Console.WriteLine(info.Parameters);
```

## Creating a custom model

Build a new model from an existing one with a custom system prompt or other Modelfile instructions:

```csharp
await foreach (var status in ollama.CreateModelAsync(new CreateModelRequest
{
    Model = "my-assistant",
    From = "llama3.2",
    System = "You are a helpful assistant that only speaks like a pirate.",
}))
{
    Console.WriteLine(status?.Status);
}
```

## Generating embeddings

Although embeddings are primarily used in the context of RAG (retrieval-augmented generation) or semantic search, they are managed through the same client. See also the [Chat and Generate](chat-and-generate.md) page for usage alongside chat models.

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "nomic-embed-text");

EmbedResponse response = await ollama.EmbedAsync("The quick brown fox jumps over the lazy dog");
float[] vector = response.Embeddings[0];

Console.WriteLine($"Embedding dimension: {vector.Length}");
```

## Checking server availability

```csharp
bool running = await ollama.IsRunningAsync();
Console.WriteLine(running ? "Ollama is running" : "Ollama is not available");
```

## Getting the Ollama version

```csharp
string version = await ollama.GetVersionAsync();
Console.WriteLine($"Ollama version: {version}");
```

## Working with blobs

Blobs (Binary Large Objects) are used when creating models from local files. First check whether a blob already exists on the server before uploading:

```csharp
var digest = "sha256:29fdb92e57cf...";

if (!await ollama.IsBlobExistsAsync(digest))
{
    var bytes = await File.ReadAllBytesAsync("my-model-weights.bin");
    await ollama.PushBlobAsync(digest, bytes);
}
```

Then reference the digest in a `CreateModelRequest`:

```csharp
await foreach (var status in ollama.CreateModelAsync(new CreateModelRequest
{
    Model = "my-local-model",
    Files = new Dictionary<string, string> { ["my-model-weights.bin"] = digest },
}))
{
    Console.WriteLine(status?.Status);
}
```
