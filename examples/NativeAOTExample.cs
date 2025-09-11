using System.Text.Json.Serialization;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Example.NativeAOT;

/// <summary>
/// Example JsonSerializerContext for NativeAOT scenarios with custom types.
/// Include all types that might be serialized in your application.
/// </summary>
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatResponseStream))]
[JsonSerializable(typeof(ChatDoneResponseStream))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Message.ToolCall))]
[JsonSerializable(typeof(Message.Function))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(Function))]
[JsonSerializable(typeof(Parameters))]
[JsonSerializable(typeof(Property))]
// Add your custom types here - this is where you would add types from Semantic Kernel
// For example, if you're using VectorSearchResult and TextSearchResult:
// [JsonSerializable(typeof(Microsoft.Extensions.VectorData.VectorSearchResult<MyApp.ConversationDocumentTextData<System.Guid>>))]
// [JsonSerializable(typeof(Microsoft.SemanticKernel.Data.TextSearchResult))]
// [JsonSerializable(typeof(List<Microsoft.SemanticKernel.Data.TextSearchResult>))]
public partial class CustomJsonContext : JsonSerializerContext
{
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Example 1: Standard usage (no source generation, maximum compatibility)
        var standardClient = new OllamaApiClient("http://localhost:11434", "llama3.2");
        
        // Example 2: NativeAOT usage with custom JsonSerializerContext
        var nativeAotClient = OllamaApiClient.CreateForNativeAOT(
            "http://localhost:11434", 
            "llama3.2", 
            CustomJsonContext.Default);

        // Example 3: Configuration-based approach
        var config = new OllamaApiClient.Configuration
        {
            Uri = new Uri("http://localhost:11434"),
            Model = "llama3.2",
            JsonSerializerContext = CustomJsonContext.Default // Set to null for standard serialization
        };
        var configClient = new OllamaApiClient(config);

        // Both clients work the same way
        var chatRequest = new ChatRequest
        {
            Model = "llama3.2",
            Messages = new List<Message>
            {
                new Message(ChatRole.User, "Hello!")
            }
        };

        await foreach (var response in nativeAotClient.ChatAsync(chatRequest))
        {
            Console.Write(response?.Message?.Content);
        }
    }
}
