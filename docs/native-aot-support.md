# Native AOT Support

OllamaSharp supports .NET Native AOT (Ahead-of-Time) compilation for improved startup performance and reduced memory footprint. 

## Default Behavior

By default, OllamaSharp uses standard System.Text.Json serialization without source generation. This provides maximum compatibility with different types and scenarios, including third-party libraries that might serialize complex types into chat messages.

## For Native AOT Scenarios

When using Native AOT, you may need to provide your own `JsonSerializerContext` that includes all the types that will be serialized. This is especially important if you're working with custom types in chat messages or using libraries like Semantic Kernel with complex vector store results.

### Creating a Custom JsonSerializerContext

```csharp
using System.Text.Json.Serialization;
using OllamaSharp.Models;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatResponseStream))]
[JsonSerializable(typeof(ChatDoneResponseStream))]
// Add your custom types here
[JsonSerializable(typeof(Microsoft.Extensions.VectorData.VectorSearchResult<MyApp.ConversationDocumentTextData<Guid>>))]
[JsonSerializable(typeof(Microsoft.SemanticKernel.Data.TextSearchResult))]
[JsonSerializable(typeof(List<Microsoft.SemanticKernel.Data.TextSearchResult>))]
// Add any other types that might be serialized in your messages
public partial class MyCustomJsonContext : JsonSerializerContext
{
}
```

### Using OllamaApiClient with Custom Context

```csharp
// pass your configuration context to the OllamaApiClient
var config = new OllamaApiClient.Configuration
{
    Uri = new Uri("http://localhost:11434"),
    Model = "llama3.2",
    JsonSerializerContext = MyCustomJsonContext.Default
};
var client = new OllamaApiClient(config);
```

## Important Notes

1. **Include All Types**: When creating your custom JsonSerializerContext, make sure to include all types that might be serialized, including:
   - Standard OllamaSharp types (already included in the default context)
   - Your custom message content types
   - Third-party library types (e.g., from Semantic Kernel, vector databases)
   - Collection types (List<T>, IEnumerable<T>, etc.)

2. **Avoid Iterators**: System.Text.Json cannot serialize iterator types like `ListSelectIterator`. Always materialize collections to concrete types (e.g., using `.ToList()`) before serialization.

3. **Backward Compatibility**: The default behavior (without JsonSerializerContext) remains unchanged, so existing code will continue to work without modifications.

## Troubleshooting

### "JsonTypeInfo metadata was not provided" Error

This error occurs when a type is being serialized that wasn't included in your JsonSerializerContext. To fix:

1. Identify the missing type from the error message
2. Add it to your JsonSerializerContext with `[JsonSerializable(typeof(YourType))]`
3. Rebuild your application

### Complex Generic Types

For complex generic types like `System.Linq.Enumerable+ListSelectIterator`, you have two options:

1. **Recommended**: Materialize the collection before serialization:
   ```csharp
   var results = vectorSearchResults.Select(r => new TextSearchResult(...)).ToList();
   ```

2. **Alternative**: Add the specific instantiated generic type to your JsonSerializerContext:
   ```csharp
   [JsonSerializable(typeof(List<Microsoft.SemanticKernel.Data.TextSearchResult>))]
   ```
