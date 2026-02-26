# Chat and Generate

OllamaSharp exposes two main ways to interact with a language model: the high-level `Chat` class and the lower-level `GenerateAsync` / `ChatAsync` methods on `OllamaApiClient`.

## The `Chat` class

The `Chat` class is the recommended starting point for most conversational use cases. It automatically maintains a message history across turns so the model has full context of the conversation.

### Basic chat loop

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2");
var chat = new Chat(ollama);

while (true)
{
    Console.Write("You: ");
    var message = Console.ReadLine()!;

    Console.Write("Assistant: ");
    await foreach (var token in chat.SendAsync(message))
        Console.Write(token);

    Console.WriteLine();
}
```

Every call to `SendAsync` appends the user message and the model's reply to `chat.Messages`, so subsequent turns automatically include the conversation history.

### System prompts

Pass a system prompt to the constructor to give the model a persona or set behavioural constraints:

```csharp
var chat = new Chat(ollama, "You are a helpful assistant that only answers questions about cooking.");

await foreach (var token in chat.SendAsync("How do I make pasta carbonara?"))
    Console.Write(token);
```

### Accessing the message history

The full conversation is stored in `chat.Messages` and can be inspected or serialised at any time:

```csharp
foreach (var msg in chat.Messages)
    Console.WriteLine($"[{msg.Role}] {msg.Content}");
```

### Sending images (multi-modal models)

Vision models such as `llama3.2-vision` accept images alongside text. Pass image data as raw bytes:

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2-vision");
var chat = new Chat(ollama);

var imageBytes = await File.ReadAllBytesAsync("photo.jpg");

await foreach (var token in chat.SendAsync("What do you see in this image?", [imageBytes]))
    Console.Write(token);
```

Or as Base64-encoded strings if that is more convenient:

```csharp
var base64 = Convert.ToBase64String(imageBytes);

await foreach (var token in chat.SendAsync("Describe the image", [base64]))
    Console.Write(token);
```

### Structured / JSON output

Ask the model to respond with valid JSON by passing `"json"` as the `format` argument, or pass a JSON Schema object:

```csharp
await foreach (var token in chat.SendAsync(
    "List the capitals of France, Germany and Italy as JSON",
    tools: null,
    imagesAsBase64: null,
    format: "json"))
{
    Console.Write(token);
}
```

### Sending a message in a specific role

`SendAsAsync` lets you inject messages under any role — useful for priming the conversation or simulating prior turns:

```csharp
// Inject a previous assistant turn before the user speaks
await foreach (var token in chat.SendAsAsync(ChatRole.Assistant, "I already know your name is Alex."))
    Console.Write(token);

await foreach (var token in chat.SendAsAsync(ChatRole.User, "What is my name?"))
    Console.Write(token);
```

### Thinking / reasoning models

For reasoning models (e.g. `deepseek-r1`, `qwen3`, `phi4-reasoning`) you can request "think tokens". When `Think` is `true`, the model's internal reasoning is surfaced through the `OnThink` event and kept separate from the visible answer:

```csharp
var chat = new Chat(ollama) { Think = true };

chat.OnThink += (_, thoughts) => Console.Write($"[thinking] {thoughts}");

await foreach (var token in chat.SendAsync("What is the square root of 144?"))
    Console.Write(token);
```

See the [Ollama release notes](https://github.com/ollama/ollama/releases/tag/v0.9.0) for supported models.

### Model-level options

Fine-tune inference parameters via the `Options` property:

```csharp
var chat = new Chat(ollama)
{
    Options = new RequestOptions
    {
        Temperature = 0.7f,
        TopP = 0.9f,
        NumCtx = 4096,
    }
};
```

---

## Using tools (function calling)

Many models support tools. See the [Tool Support](tool-support.md) page for a detailed walkthrough. The short version is:

```csharp
// Define a tool with the [OllamaTool] attribute (requires source generator)
public class MyTools
{
    /// <summary>Gets the current weather for a city.</summary>
    /// <param name="city">Name of the city</param>
    [OllamaTool]
    public static string GetWeather(string city) => $"Sunny and 22°C in {city}.";
}

// Pass tool instances alongside the message
var chat = new Chat(ollama);

await foreach (var token in chat.SendAsync("What's the weather in Berlin?", [new GetWeatherTool()]))
    Console.Write(token);
```

Tool calls and their results are fed back into `chat.Messages` automatically.

---

## `GenerateAsync` — single-turn completions

`GenerateAsync` maps directly to the `/api/generate` Ollama endpoint. Unlike `Chat`, it does **not** maintain history between calls — each call is self-contained.

### Streaming a completion to the console

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2");

await foreach (var chunk in ollama.GenerateAsync("Why is the sky blue?"))
    Console.Write(chunk.Response);
```

### Providing context manually

If you need multi-turn behaviour without the `Chat` class you can pass the context tokens returned by a previous response:

```csharp
GenerateResponseStream? lastResponse = null;

await foreach (var chunk in ollama.GenerateAsync("Tell me a joke"))
{
    Console.Write(chunk.Response);
    lastResponse = chunk;
}

// Use the context from the previous turn
var request = new GenerateRequest
{
    Prompt = "Explain why that was funny",
    Context = lastResponse?.Context,
};

await foreach (var chunk in ollama.GenerateAsync(request))
    Console.Write(chunk.Response);
```

### Generating with an image

```csharp
var imageBytes = await File.ReadAllBytesAsync("chart.png");

var request = new GenerateRequest
{
    Prompt = "Summarise this chart",
    Images = [imageBytes],
};

await foreach (var chunk in ollama.GenerateAsync(request))
    Console.Write(chunk.Response);
```

---

## `ChatAsync` — low-level chat

`ChatAsync` maps directly to the `/api/chat` Ollama endpoint and gives full control over the request. The `Chat` class uses it internally. Prefer the `Chat` class unless you need precise control over the request.

```csharp
var request = new ChatRequest
{
    Model = "llama3.2",
    Stream = true,
    Messages =
    [
        new Message(ChatRole.System, "You are a concise assistant."),
        new Message(ChatRole.User, "What is the capital of France?"),
    ],
};

await foreach (var chunk in ollama.ChatAsync(request))
    Console.Write(chunk?.Message.Content);
```

---

## Generating embeddings

Use `EmbedAsync` to produce vector embeddings for semantic search, clustering and similar tasks:

```csharp
var ollama = new OllamaApiClient("http://localhost:11434", "nomic-embed-text");

var response = await ollama.EmbedAsync("The quick brown fox");
float[] vector = response.Embeddings[0];
```

Multiple inputs can be embedded in a single round-trip:

```csharp
var response = await ollama.EmbedAsync(new EmbedRequest
{
    Model = "nomic-embed-text",
    Input = ["First sentence", "Second sentence"],
});
```
