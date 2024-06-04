using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#delete-a-model
/// </summary>
public class DeleteModelRequest
{
	[JsonPropertyName("name")]
	public string Name { get; set; }
}