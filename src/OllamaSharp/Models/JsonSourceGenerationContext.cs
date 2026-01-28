using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models.Chat;

namespace OllamaSharp.Models;

/// <summary>
/// JsonSourceGenerationContext
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(DeleteModelRequest))]
[JsonSerializable(typeof(ListModelsResponse))]
[JsonSerializable(typeof(ListRunningModelsResponse))]
[JsonSerializable(typeof(EmbedRequest))]
[JsonSerializable(typeof(EmbedResponse))]
[JsonSerializable(typeof(ShowModelRequest))]
[JsonSerializable(typeof(ShowModelResponse))]
[JsonSerializable(typeof(CreateModelRequest))]
[JsonSerializable(typeof(CreateModelResponse))]
[JsonSerializable(typeof(PullModelRequest))]
[JsonSerializable(typeof(PullModelResponse))]
[JsonSerializable(typeof(PushModelRequest))]
[JsonSerializable(typeof(PushModelResponse))]
[JsonSerializable(typeof(GenerateDoneResponseStream))]
[JsonSerializable(typeof(GenerateResponseStream))]
[JsonSerializable(typeof(GenerateRequest))]
[JsonSerializable(typeof(Message.ToolCall))]
[JsonSerializable(typeof(Message.Function), TypeInfoPropertyName = "MessageFunction")]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(Function))]
[JsonSerializable(typeof(Parameters))]
[JsonSerializable(typeof(Property))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(OllamaFunctionResultContent))]
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatDoneResponseStream))]
[JsonSerializable(typeof(ChatResponseStream))]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
