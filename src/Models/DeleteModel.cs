using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#delete-a-model
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class DeleteModelRequest
{
	/// <summary>
	/// The name of the model to delete
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}