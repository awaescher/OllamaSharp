using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

public class ChatResponseStream
{
	/// <summary>
	/// The model that generated the response
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The time the response was generated
	/// </summary>
	[JsonPropertyName("created_at")]
	public string CreatedAt { get; set; } = null!;

	/// <summary>
	/// The message returned by the model
	/// </summary>
	[JsonPropertyName("message")]
	public Message Message { get; set; } = null!;

	/// <summary>
	/// Whether the response is complete
	/// </summary>
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}