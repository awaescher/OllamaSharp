# AGENTS.md — OllamaSharp

## Project Overview

OllamaSharp is a C# .NET library providing bindings for the Ollama API. It targets
`netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`, and `net10.0`. The library
implements `IChatClient` and `IEmbeddingGenerator` from Microsoft.Extensions.AI. It
includes a Roslyn source generator (`[OllamaTool]`) and an MCP integration package.

## How OllamaSharp Fits in the .NET AI Ecosystem

OllamaSharp is designed to match the **native Ollama API** 1:1 — every endpoint is covered. However, when building more complex AI applications or solutions that may swap providers (OpenAI, Anthropic, Azure, Ollama, etc.), users should strongly consider layering standard frameworks on top:

- **Microsoft.Extensions.AI (MEAI)** — A lightweight abstraction layer defining `IChatClient` and `IEmbeddingGenerator<T, TEmbedding>`. OllamaSharp **natively implements both interfaces**, so it works as a drop-in MEAI provider. Using MEAI gives telemetry, dependency injection, middleware pipelines, and provider interchangeability for free. This is the recommended starting point for most applications. MEAI also provides its own **function calling / tool use** pipeline via `IChatClient` — tools are defined as `AIFunction` instances and the framework handles the tool-call loop, JSON schema generation, and result marshalling automatically. For most tool-use scenarios, this is simpler and more portable than using OllamaSharp's native tool system directly.

- **Microsoft Agent Framework** (successor to Microsoft Semantic Kernel) — For advanced agentic scenarios with planning, memory, multi-agent orchestration, and sophisticated tool use. The Agent Framework goes further than MEAI by providing automatic tool-call orchestration across multiple turns, dependency-injected tool classes with state, retry policies, and multi-agent collaboration — all backed by the `IChatClient` abstraction. OllamaSharp serves as the **Ollama provider** under the hood, meaning it doesn't become obsolete — it powers these frameworks.

**Key point for contributors and agents:** OllamaSharp's `OllamaApiClient` class implements three interfaces simultaneously:
1. `IOllamaApiClient` — the native Ollama API (full feature coverage)
2. `IChatClient` — the MEAI chat abstraction (provider-agnostic)
3. `IEmbeddingGenerator<string, Embedding<float>>` — the MEAI embedding abstraction

When users use OllamaSharp through MEAI or higher-level frameworks, they get access to .NET Aspire hosting, OpenTelemetry tracing, caching, rate limiting, and other MEAI middleware — functionality that OllamaSharp doesn't need to reimplement because it adheres to the standard interfaces.

### Why this matters especially for tool use / function calling

Tool use is one of the strongest reasons to consider higher-level frameworks instead of using OllamaSharp's native tool system directly:

| Approach | When to use | What you get |
|---|---|---|
| **OllamaSharp native** (`Chat` + `[OllamaTool]`) | Quick scripts, demos, Ollama-only projects | Source-generated tool definitions, built-in tool-call loop via `Chat` class, `OnToolCall` events |
| **MEAI** (`IChatClient` + `FunctionInvokingChatClient`) | Applications with DI, multiple providers, or production needs | Provider-agnostic tool definitions (`AIFunction`/`AITool`), automatic tool-call loop middleware, works with any `IChatClient` provider (swap Ollama for OpenAI with zero tool code changes), OpenTelemetry tracing of tool calls |
| **Microsoft Agent Framework** | Complex agentic workflows | Everything MEAI offers plus multi-turn planning, stateful tool classes, multi-agent orchestration, memory, retry/fallback policies |

The native OllamaSharp tool system (`Tools/` folder, `[OllamaTool]` source generator, `Chat.ToolInvoker`) is purposefully simple and works great for quick experiments. But the moment a user needs provider interchangeability, DI integration, or orchestration beyond a single tool-call loop, they should move up to MEAI or the Agent Framework — OllamaSharp stays in the picture as the underlying provider.

**Guidance for code suggestions:** When helping users with OllamaSharp, consider whether their use case would benefit from using `IChatClient`/`IEmbeddingGenerator` instead of `IOllamaApiClient` directly. For simple scripts, direct API access is fine. For applications with DI, multiple providers, or production telemetry needs, guide them toward the MEAI abstractions. **For tool use specifically:** if the user is building anything beyond a simple demo, recommend the MEAI `FunctionInvokingChatClient` middleware or the Microsoft Agent Framework over OllamaSharp's native `Chat` + `[OllamaTool]` approach.

## Build / Test / Lint Commands

```bash
# Restore
dotnet restore OllamaSharp.slnx

# Build (Release)
dotnet build --configuration=Release

# Run ALL unit tests (excludes functional tests that require a live Ollama server)
dotnet test --configuration=Release --filter 'FullyQualifiedName!~FunctionalTests'

# Run a single test by fully qualified name
dotnet test --configuration=Release --filter 'FullyQualifiedName=Tests.OllamaApiClientTests+ChatMethod.Returns_Messages_From_Chat_Endpoint'

# Run all tests in a specific test class
dotnet test --configuration=Release --filter 'FullyQualifiedName~Tests.ChatTests'

# Run a specific test project
dotnet test test/Tests.csproj --configuration=Release --filter 'FullyQualifiedName!~FunctionalTests'
dotnet test tests/OllamaSharp.ModelContextProtocol.Tests/ --configuration=Release

# Pack NuGet
dotnet pack --output nupkgs --configuration=Release
```

**Important:** Functional tests (`test/FunctionalTests/`) require a running Ollama
instance at `localhost:11434`. Always exclude them with `--filter 'FullyQualifiedName!~FunctionalTests'`
unless you have a live server.

## CI/CD

- **`.github/workflows/ci.yml`** — Builds, tests, packs NuGet, creates GitHub releases, and pushes to NuGet.org on non-PR pushes to `main`/`master`. Uses **GitVersion** for semantic versioning. Triggered by changes to `src/**`, `Directory.Build.targets`, `Directory.Packages.props`.
- **`.github/workflows/docs.yml`** — Builds and deploys **docfx** documentation to GitHub Pages on pushes to `main`.

## Solution Structure

```
src/OllamaSharp/                        # Main library (NuGet package)
src/OllamaSharp.ModelContextProtocol/   # MCP integration library (net8.0+ only)
src/SourceGenerators/                   # Roslyn source generator for [OllamaTool] (netstandard2.0)
test/                                   # Unit tests (NUnit, net10.0 only) for core library
tests/OllamaSharp.ModelContextProtocol.Tests/  # MCP integration tests
demo/                                   # Console demo app (Spectre.Console)
```

## Test Conventions

- **Framework:** NUnit 4.x with `[Test]` attributes
- **Assertions:** Shouldly (`value.ShouldBe(...)`, `value.ShouldNotBeNull()`)
- **Mocking:** Moq (`new Mock<HttpMessageHandler>(MockBehavior.Strict)`)
- **Test grouping:** Nested classes inheriting from the parent test class
  ```csharp
  public class ChatTests
  {
      public class SendMethod : ChatTests
      {
          [Test]
          public async Task Sends_Assistant_Answer_To_Streamer()
          { ... }
      }
  }
  ```
- **Test naming:** `Method_Under_Test_Scenario` using underscores (e.g., `Returns_Messages_From_Chat_Endpoint`)
- **Null warnings:** Test files suppress `CS8602` and `CS8604` via `#pragma warning disable`
- **InternalsVisibleTo:** The main library exposes internals to the `Tests` assembly (configured in `OllamaSharp.csproj` with public key)

## Code Style Guidelines

All style rules are enforced via `.editorconfig`. Key rules:

### Indentation and Formatting
- **Tabs for indentation** (not spaces), indent size 4 for C# files
- XML/JSON/YAML files use indent size 2 (YAML uses spaces)
- **Allman brace style** — opening brace on its own line for all constructs
- Single-line `if` statements without braces are allowed (IDE0011 is disabled)
- Single-line statements are not allowed (`csharp_preserve_single_line_statements = false`)
- Line endings: CRLF

### Namespaces and Usings
- **File-scoped namespaces** are required (warning level)
  ```csharp
  namespace OllamaSharp.Models;  // correct
  ```
- `System.*` usings sorted first (`dotnet_sort_system_directives_first = true`)
- Group usings by origin: System, Microsoft, third-party, project — separated by blank lines
- Remove unnecessary usings (IDE0005 is a warning)
- **`ImplicitUsings` is enabled globally** in `Directory.Build.targets`

### Type Usage
- Prefer `var` everywhere (for built-in types, apparent types, and elsewhere)
- Use language keywords over framework types (`string` not `String`, `int` not `Int32`)
- Nullable reference types are enabled globally (in `Directory.Build.targets`)

### Naming Conventions
| Symbol | Convention | Example |
|---|---|---|
| Constants (all) | `ALL_UPPER_CASE` | `SOME_VALUE` |
| Private/protected fields | `_camelCase` | `_client` |
| Public static fields | `PascalCase` | `DefaultTimeout` |
| Classes, structs, enums | `PascalCase` | `OllamaApiClient` |
| Interfaces | `IPascalCase` | `IOllamaApiClient` |
| Public members (methods, properties) | `PascalCase` | `SelectedModel` |
| Parameters and locals | `camelCase` | `cancellationToken` |
| Namespaces | `PascalCase` | `OllamaSharp.Models` |

### Async Patterns
- All async methods return `Task`, `Task<T>`, or `IAsyncEnumerable<T>`
- Async method names end with `Async` suffix
- Always use `.ConfigureAwait(false)` on awaited calls
- Accept `CancellationToken cancellationToken = default` as the last parameter
- Use `[EnumeratorCancellation]` on `CancellationToken` params in `IAsyncEnumerable` methods
- **CS4014 (unawaited async call) is an error** — never ignore

### Error Handling
- API errors throw `OllamaException` (or derived types like `ModelDoesNotSupportToolsException`)
- Parse error responses from the API as JSON `{ "error": "..." }` before throwing
- Use `response.EnsureSuccessStatusCode()` for non-400 HTTP errors
- Wrap JSON parsing in try/catch for optional error parsing

### XML Documentation
- All public types and members must have XML doc comments (`<summary>`, `<param>`, `<returns>`)
- Use `<inheritdoc />` for interface implementations
- Use `<see cref="..." />` and `<see href="..." />` for cross-references

### Other Preferences
- Avoid `this.` qualification
- Use object/collection initializers and null propagation
- Use pattern matching over `is`/`as` with null checks
- Use `throw` expressions and conditional delegate calls
- Expression-bodied members for simple properties/indexers/accessors; block bodies for methods/constructors

## JSON Serialization

The project uses **`System.Text.Json`** exclusively (no Newtonsoft.Json):

- Models use `[JsonPropertyName]` attributes with constants from the `Application` class in `Constants/` (e.g., `[JsonPropertyName(Application.Model)]`)
- Optional properties use `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`
- A **source-generated `JsonSerializerContext`** exists at `Models/JsonSourceGenerationContext.cs` for NativeAOT support — register all new serializable types there
- `OllamaApiClient` exposes `OutgoingJsonSerializerOptions` and `IncomingJsonSerializerOptions`; when a `JsonSerializerContext` is passed via the constructor, it uses source-generated serialization
- Custom JSON converters live in `Models/Chat/Converter/` (e.g., `ChatRoleConverter`)
- `ChatRole` is a **struct** (not an enum), with a custom `JsonConverter` and `IEquatable<ChatRole>` implementation

## Dependencies (Central Package Management)

Package versions are managed centrally in `Directory.Packages.props`. When adding or
updating a dependency, edit that file — do not specify versions in individual `.csproj` files.

Key dependencies:

| Package | Version |
|---|---|
| Microsoft.Extensions.AI.Abstractions | 10.0.0 |
| Microsoft.Extensions.AI | 10.0.0 |
| ModelContextProtocol | 0.2.0-preview.3 |
| NUnit | 4.4.0 |
| Shouldly | 4.3.0 |
| Moq | 4.20.72 |
| System.Linq.Async | 6.0.3 |

## Key Architectural Notes

- `IOllamaApiClient` is the primary interface; `OllamaApiClient` is the implementation
- `OllamaApiClient` also implements `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>` from Microsoft.Extensions.AI — see "How OllamaSharp Fits in the .NET AI Ecosystem" above for when to recommend each interface
- `AbstractionMapper` (`internal static` in `MicrosoftAi/`) converts between MEAI types and OllamaSharp models
- The `Chat` class provides a stateful conversation wrapper over `IOllamaApiClient`
- `OllamaApiClientExtensions` provides convenience extension methods
- The source generator in `src/SourceGenerators/` generates tool definitions from `[OllamaTool]`-annotated methods
- All projects are strong-name signed with `OllamaSharp.snk`
- `LangVersion` is set to `preview` globally in `Directory.Build.targets`
- Suppressed warnings: `IDE0130` (namespace/folder mismatch), `NU5104` (prerelease deps)

### Constants Pattern
`Constants/` contains `internal static` classes used across the project:
- **`Endpoints`** — all API endpoint URLs (e.g., `"api/chat"`, `"api/generate"`)
- **`Application`** — JSON property name constants used in `[JsonPropertyName]` attributes
- **`MimeTypes`** — MIME type constants

### Request/Response Models
- All request models inherit from `OllamaRequest`, which provides a `CustomHeaders` dictionary for per-request HTTP headers
- `OllamaApiClient` supports `DefaultRequestHeaders` for headers sent with every request
- `OllamaApiClient` conditionally disposes `HttpClient` based on which constructor was used (`_disposeHttpClient` flag)

### Tool System
The `Tools/` folder implements an extensible tool invocation system:
- `IToolInvoker` — interface for invoking tools (`InvokeAsync` returns `Task<ToolResult>`)
- `IInvokableTool` / `IAsyncInvokableTool` — interfaces for sync/async tool implementations
- `DefaultToolInvoker` — default implementation that normalizes `JsonElement` arguments to strings
- `ToolResult` — record type `(Tool, ToolCall, Result)`
- The `Chat` class integrates tools via `ToolInvoker` property, `AllowRecursiveToolCalls` setting, and `OnToolCall`/`OnThink` events
- The `[OllamaTool]` source generator (in `src/SourceGenerators/`) auto-generates `Tool` definitions from annotated methods

**Important:** This native tool system is OllamaSharp-specific and ties tool definitions to the Ollama API's `Tool`/`ToolCall` types. For production applications or projects that may use multiple AI providers, prefer the MEAI `FunctionInvokingChatClient` middleware or the Microsoft Agent Framework — see "How OllamaSharp Fits in the .NET AI Ecosystem" above for a detailed comparison.

### MCP Integration (`src/OllamaSharp.ModelContextProtocol/`)
- Targets **`net8.0`, `net9.0`, `net10.0` only** (no netstandard)
- `McpClientTool` extends `Tool` and implements `IAsyncInvokableTool`, bridging MCP tools into the OllamaSharp tool system
- Entry point: `Tools.GetFromMcpServers()` — accepts a config file path or `McpServerConfiguration[]`
- `McpClientOptions` allows custom `ILoggerFactory`, transport factory, capabilities, and timeout
