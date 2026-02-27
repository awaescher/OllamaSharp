# Advanced Configuration

This page covers configuration options that go beyond the basics of setting up the client.

## Constructor overloads

`OllamaApiClient` offers several constructors depending on your needs:

| Constructor | When to use |
|---|---|
| `OllamaApiClient(string uri, string model = "")` | Quick setup with a URI string |
| `OllamaApiClient(Uri uri, string model = "")` | Quick setup with a `Uri` object |
| `OllamaApiClient(Configuration config)` | Advanced setup with Native AOT support |
| `OllamaApiClient(HttpClient client, string model = "", JsonSerializerContext? ctx = null)` | Full control over the HTTP pipeline |

### Using a shared `HttpClient`

If your application already manages an `HttpClient` (e.g. via `IHttpClientFactory`), pass it directly:

```csharp
var httpClient = httpClientFactory.CreateClient("ollama");
httpClient.BaseAddress = new Uri("http://localhost:11434");

var ollama = new OllamaApiClient(httpClient, "qwen3.5:35b-a3b");
```

> [!NOTE]
> When you pass your own `HttpClient`, OllamaSharp will **not** dispose it. When OllamaSharp creates its own `HttpClient` internally (URI-based constructors or `Configuration`), it takes ownership and disposes it when you call `Dispose()`.

## Custom HTTP headers

### Default headers (every request)

Set headers that are sent with every request via `DefaultRequestHeaders`:

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "qwen3.5:35b-a3b");

ollama.DefaultRequestHeaders["Authorization"] = "Bearer sk-...";
ollama.DefaultRequestHeaders["X-Custom-Header"] = "my-value";
```

### Per-request headers

All request models inherit from `OllamaRequest` and expose a `CustomHeaders` dictionary. These are merged with the default headers for that single request:

```csharp
var request = new ChatRequest
{
    Model = "qwen3.5:35b-a3b",
    Messages = [new Message(ChatRole.User, "Hello!")],
};
request.CustomHeaders["X-Request-Id"] = Guid.NewGuid().ToString();

await foreach (var chunk in ollama.ChatAsync(request))
    Console.Write(chunk?.Message.Content);
```

> [!TIP]
> Per-request headers override default headers with the same key.

## JSON serialisation options

`OllamaApiClient` exposes two `JsonSerializerOptions` instances:

| Property | Purpose |
|---|---|
| `OutgoingJsonSerializerOptions` | Used when serialising request bodies to JSON |
| `IncomingJsonSerializerOptions` | Used when deserialising response bodies from JSON |

These are read-only properties but you can mutate their settings if you need to register custom converters or change serialisation behaviour:

```csharp
ollama.OutgoingJsonSerializerOptions.Converters.Add(new MyCustomConverter());
```

For Native AOT scenarios, pass a `JsonSerializerContext` through the `Configuration` constructor instead. See [Native AOT Support](native-aot-support.md).

## KeepAlive — controlling model lifetime

The `KeepAlive` property on request models controls how long a model stays loaded in GPU/CPU memory after a request completes. The value is a duration string understood by Ollama:

| Value | Meaning |
|---|---|
| `"5m"` | Keep the model loaded for 5 minutes |
| `"1h"` | Keep the model loaded for 1 hour |
| `"0s"` | Unload immediately after the request |
| *(omitted)* | Use the Ollama server default |

```csharp
var request = new ChatRequest
{
    Model = "qwen3.5:35b-a3b",
    KeepAlive = "10m",
    Messages = [new Message(ChatRole.User, "Hello!")],
};
```

To unload a model immediately without sending a chat/generate request, use the convenience extension:

```csharp
await ollama.RequestModelUnloadAsync("qwen3.5:35b-a3b");
```

## Listing running models

Check which models are currently loaded and when they will expire:

```csharp
var running = await ollama.ListRunningModelsAsync();

foreach (var model in running)
    Console.WriteLine($"{model.Name} — VRAM: {model.SizeVram} bytes — expires {model.ExpiresAt}");
```

## Disposing the client

`OllamaApiClient` implements `IDisposable`. If you created the client with a URI or `Configuration`, call `Dispose()` to release the internal `HttpClient`:

```csharp
using var ollama = new OllamaApiClient("http://localhost:11434", "qwen3.5:35b-a3b");
// ... use ollama ...
// HttpClient is disposed automatically at end of scope
```

If you passed your own `HttpClient`, OllamaSharp will **not** dispose it — you manage its lifetime.
