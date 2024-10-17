using System.Collections.Generic;

namespace OllamaSharp.Models;

/// <summary>
/// Base class for requests to Ollama
/// </summary>
public abstract class OllamaRequest
{
	public Dictionary<string, string> CustomHeaders { get; } = new();
}
