using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-embeddings
/// </summary>
public class GenerateEmbeddingRequest
{
	/// <summary>
	/// The name of the model to generate embeddings from
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The text to generate embeddings for
	/// </summary>
	[JsonPropertyName("prompt")]
	public string Prompt { get; set; } = null!;

	/// <summary>
	/// Additional model parameters listed in the documentation for the Modelfile
	/// such as temperature.
	/// </summary>
	[JsonPropertyName("options")]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Gets or sets the KeepAlive property, which decides how long a given
	/// model should stay loaded.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }
}

/// <summary>
/// The response from the /api/embeddings endpoint
/// </summary>
public class GenerateEmbeddingResponse
{
	/// <summary>
	/// An array of embeddings for the input text
	/// </summary>
	[JsonPropertyName("embedding")]
	public double[] Embedding { get; set; } = null!;
}