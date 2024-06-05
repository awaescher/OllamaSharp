using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#pull-a-model
/// </summary>
public class PullModelRequest
{
	/// <summary>
	/// The name of the model to pull in the form of <namespace>/<model>:<tag>
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}

/// <summary>
/// The streamed response from the /api/pull endpoint
/// </summary>
public class PullModelResponse
{
	/// <summary>
	/// The status of the pull operation
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;

	/// <summary>
	/// The hash of the model file
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// The total number of bytes to pull
	/// </summary>
	[JsonPropertyName("total")]
	public long Total { get; set; }

	/// <summary>
	/// The number of bytes pulled so far
	/// </summary>
	[JsonPropertyName("completed")]
	public long Completed { get; set; }

	/// <summary>
	/// The percentage of the pull operation that has been completed
	/// </summary>
	[JsonIgnore]
	public double Percent => Total == 0 ? 100.0 : Completed * 100.0 / Total;
}