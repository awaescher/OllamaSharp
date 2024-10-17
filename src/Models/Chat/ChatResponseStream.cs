using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents a streamed response from a chat model in the Ollama API.
/// </summary>
public class ChatResponseStream
{
	/// <summary>
	/// Gets or sets the model that generated the response.
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// Gets or sets the time the response was generated.
	/// </summary>
	[JsonPropertyName("created_at")]
	public string CreatedAt { get; set; } = null!;

	/// <summary>
	/// Gets or sets the message returned by the model.
	/// </summary>
	[JsonPropertyName("message")]
	public Message Message { get; set; } = null!;

	/// <summary>
	/// Gets or sets a value indicating whether the response is complete.
	/// </summary>
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}
