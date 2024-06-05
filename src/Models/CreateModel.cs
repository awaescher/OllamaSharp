using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#create-a-model
/// </summary>
public class CreateModelRequest
{
	/// <summary>
	/// Name of the model to create
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = null!;

	/// <summary>
	/// Contents of the Modelfile
	/// See https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md
	/// </summary>
	[JsonPropertyName("modelfile")]
	public string ModelFileContent { get; set; } = null!;

	/// <summary>
	/// Path to the Modelfile (optional)
	/// </summary>
	[JsonPropertyName("path")]
	public string? Path { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object,
	/// rather than a stream of objects (optional)
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }
}

// TODO: Rename to CreateModelResponse or similar _____Response to match the other models
/// <summary>
/// Represents the response from the /api/create endpoint
/// </summary>
public class CreateStatus
{
	/// <summary>
	/// Represents the status of a model creation.
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;
}