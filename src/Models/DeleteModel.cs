using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Delete a model and its data.
///
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#delete-a-model">Ollama API docs</see>
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class DeleteModelRequest : OllamaRequest
{
	/// <summary>
	/// The name of the model to delete
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}