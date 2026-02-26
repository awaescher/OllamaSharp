using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Represents the base class for requests to the Ollama API.
/// </summary>
public abstract class OllamaRequest
{
	/// <summary>
	/// Gets the custom headers to include with the request.
	/// </summary>
	[JsonIgnore]
	public Dictionary<string, string> CustomHeaders { get; } = [];
}
