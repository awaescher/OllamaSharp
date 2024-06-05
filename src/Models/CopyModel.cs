using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#copy-a-model
/// </summary>
public class CopyModelRequest
{
	/// <summary>
	/// The source model name
	/// </summary>
	[JsonPropertyName("source")]
	public string Source { get; set; } = null!;

	/// <summary>
	/// The destination model name
	/// </summary>
	[JsonPropertyName("destination")]
	public string Destination { get; set; } = null!;
}