using System.Collections.Generic;
using System.Text.Json.Serialization;

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
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The text to generate embeddings for
	/// </summary>
	[JsonPropertyName("input")]
	public List<string> Input { get; set; } = null!;

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
	public long? KeepAlive { get; set; }

	/// <summary>
	/// Truncates the end of each input to fit within context length.
	/// Returns error if false and context length is exceeded. Defaults to true
	/// </summary>
	[JsonPropertyName("truncate")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Truncate { get; set; }
}

/// <summary>
/// The response from the /api/embed endpoint
/// </summary>
public class EmbedResponse
{
	/// <summary>
	/// An array of embeddings for the input text
	/// </summary>
	[JsonPropertyName("embeddings")]
	public List<float[]> Embeddings { get; set; } = null!;

	/// <summary>
	/// The time spent generating the response
	/// </summary>
	[JsonPropertyName("total_duration")]
	public long? TotalDuration { get; set; }

	/// <summary>
	/// The time spent in nanoseconds loading the model
	/// </summary>
	[JsonPropertyName("load_duration")]
	public long? LoadDuration { get; set; }

	/// <summary>
	/// The number of tokens in the input text
	/// </summary>
	[JsonPropertyName("prompt_eval_count")]
	public int? PromptEvalCount { get; set; }
}