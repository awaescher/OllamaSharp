using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Generate embeddings from a model.
///
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-embeddings">Ollama API docs</see>
/// </summary>
public class EmbedRequest : OllamaRequest
{
	/// <summary>
	/// The name of the model to generate embeddings from
	/// </summary>
	[JsonPropertyName(Application.Model)]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The text to generate embeddings for
	/// </summary>
	[JsonPropertyName(Application.Input)]
	public List<string> Input { get; set; } = null!;

	/// <summary>
	/// Additional model parameters listed in the documentation for the Modelfile
	/// such as temperature.
	/// </summary>
	[JsonPropertyName(Application.Options)]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Gets or sets the KeepAlive property, which decides how long a given
	/// model should stay loaded.
	/// </summary>
	[JsonPropertyName(Application.KeepAlive)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }

	/// <summary>
	/// Truncates the end of each input to fit within context length.
	/// Returns error if false and context length is exceeded. Defaults to true
	/// </summary>
	[JsonPropertyName(Application.Truncate)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Truncate { get; set; }

	/// <summary>
	/// Number of dimensions for the embedding.
	/// </summary>
	[JsonPropertyName(Application.Dimensions)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Dimensions { get; set; }
}

/// <summary>
/// The response from the /api/embed endpoint
/// </summary>
public class EmbedResponse
{
	/// <summary>
	/// An array of embeddings for the input text
	/// </summary>
	[JsonPropertyName(Application.Embeddings)]
	public List<float[]> Embeddings { get; set; } = null!;

	/// <summary>
	/// The time spent generating the response
	/// </summary>
	[JsonPropertyName(Application.TotalDuration)]
	public long? TotalDuration { get; set; }

	/// <summary>
	/// The time spent in nanoseconds loading the model
	/// </summary>
	[JsonPropertyName(Application.LoadDuration)]
	public long? LoadDuration { get; set; }

	/// <summary>
	/// The number of tokens in the input text
	/// </summary>
	[JsonPropertyName(Application.PromptEvalCount)]
	public int? PromptEvalCount { get; set; }
}