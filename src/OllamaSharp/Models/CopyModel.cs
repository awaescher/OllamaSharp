using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Copy a model. Creates a model with another name from an existing model.
/// <see href="https://ollama.ai/docs/api/#copy-a-model">Ollama API docs</see>
/// </summary>
public class CopyModelRequest : OllamaRequest
{
	/// <summary>
	/// The source model name
	/// </summary>
	[JsonPropertyName(Application.Source)]
	public string Source { get; set; } = null!;

	/// <summary>
	/// The destination model name
	/// </summary>
	[JsonPropertyName(Application.Destination)]
	public string Destination { get; set; } = null!;
}