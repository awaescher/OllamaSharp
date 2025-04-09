using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Ollama server error response message
/// </summary>
public class ErrorResponse
{
	[JsonPropertyName(Application.Error)]
	public string Message { get; set; }
}
