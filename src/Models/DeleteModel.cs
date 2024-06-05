using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#delete-a-model
/// </summary>
public class DeleteModelRequest
{
	/// <summary>
	/// Model name to delete.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = null!;
}