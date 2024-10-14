using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Represents a request to push a model.
/// </summary>
public class PushModelRequest : OllamaRequest
{
	/// <summary>
	/// Gets or sets the name of the model to push in the form of namespace/model:tag.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to allow insecure connections to the library.
	/// Only use this if you are pulling from your own library during development.
	/// </summary>
	[JsonPropertyName("insecure")]
	public bool Insecure { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to stream the response.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }
}

/// <summary>
/// Represents the response from the /api/push endpoint.
/// </summary>
public class PushModelResponse
{
	/// <summary>
	/// Gets or sets the status of the push operation.
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;

	/// <summary>
	/// Gets or sets the hash of the model file.
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// Gets or sets the total number of bytes to push.
	/// </summary>
	[JsonPropertyName("total")]
	public int Total { get; set; }
}
