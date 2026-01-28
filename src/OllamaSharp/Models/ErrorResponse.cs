using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Ollama server error response message
/// </summary>
public class ErrorResponse
{
	/// <summary>
	/// The error message
	/// </summary>
	[JsonPropertyName(Application.Error)]
	public string Message { get; set; } = string.Empty;
}
