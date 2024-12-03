using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Download a model from the ollama library. Cancelled pulls are resumed from
/// where they left off, and multiple calls will share the same download progress.
/// 
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#pull-a-model">Ollama API docs</see>
/// </summary>
public class PullModelRequest : OllamaRequest
{
	/// <summary>
	/// Gets or sets the name of the model to pull.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to allow insecure connections to the library.
	/// Only use this if you are pulling from your own library during development.
	/// </summary>
	[JsonPropertyName("insecure")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Insecure { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to stream the response.
	/// </summary>
	[JsonPropertyName("stream")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Stream { get; set; }
}

/// <summary>
/// Represents the streamed response from the /api/pull endpoint.
/// </summary>
public class PullModelResponse
{
	/// <summary>
	/// Gets or sets the status of the pull operation.
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;

	/// <summary>
	/// Gets or sets the hash of the model file.
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// Gets or sets the total number of bytes to pull.
	/// </summary>
	[JsonPropertyName("total")]
	public long Total { get; set; }

	/// <summary>
	/// Gets or sets the number of bytes pulled so far.
	/// </summary>
	[JsonPropertyName("completed")]
	public long Completed { get; set; }

	/// <summary>
	/// Gets the percentage of the pull operation that has been completed.
	/// </summary>
	[JsonIgnore]
	public double Percent => Total == 0 ? 100.0 : Completed * 100.0 / Total;
}
