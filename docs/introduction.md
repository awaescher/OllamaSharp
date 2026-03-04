# Introduction

[Ollama](https://ollama.com/) is a [Go](https://go.dev/)-based, open-source server for interacting with local large language models using Georgi Gerganov's [llama.cpp](https://github.com/ggerganov/llama.cpp) library. Ollama provides first-class support for various models, including [qwen3.5](https://ollama.com/library/qwen3.5), [DeepSeek R1](https://ollama.com/library/deepseek-r1), [mistral](https://ollama.com/library/mistral), and many more. It provides support for pulling, running, creating, pushing, and interacting with models.

[OllamaSharp](https://github.com/awaescher/OllamaSharp) provides .NET bindings for the Ollama API, simplifying interactions with Ollama both locally and remotely. It provides asynchronous streaming, progress reporting and convenience classes and functions to simplify common use cases.

## Key capabilities

- **Full Ollama API coverage** — every endpoint is wrapped in an async, streaming-capable method
- **Chat conversations** — the `Chat` class tracks message history, tool calls and thinking across turns
- **Tool / function calling** — define tools with a simple `[OllamaTool]` attribute and let the source generator handle the rest
- **Microsoft.Extensions.AI** — implements `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>` for drop-in compatibility with the M.E.AI ecosystem
- **Model management** — pull, push, copy, delete, show and list models
- **Multi-modal** — send images to vision models
- **Structured output** — request JSON or JSON Schema responses
- **Reasoning models** — surface "think tokens" from models like DeepSeek R1 and Qwen3
- **Native AOT** — opt-in support via a custom `JsonSerializerContext`
- **MCP integration** — bridge model context protocol tools into OllamaSharp via the companion package

## Target frameworks

OllamaSharp targets `netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0` and `net10.0`, so it runs on virtually every modern .NET workload.

## Next steps

Head over to [Getting started](getting-started.md) to install the package and write your first chat.