using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Upload a model to a model library. Requires registering for ollama.ai and
/// adding a public key first.
///
/// <see href="https://github.com/ollama/ollama/blob/main/docs/api.md#push-a-model">Ollama API docs</see>
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
