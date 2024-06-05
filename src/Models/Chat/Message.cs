using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents a message in a chat
/// </summary>
[DebuggerDisplay("{Role}: {Content}")]
public class Message
{
	public Message(ChatRole role, string content, string[]? images)
	{
		Role = role;
		Content = content;
		Images = images;
	}

	public Message(ChatRole role, string[] images)
	{
		Role = role;
		Images = images;
	}

	public Message(ChatRole? role, string content)
	{
		Role = role;
		Content = content;
	}

	// We need this for json deserialization
	public Message()
	{
	}

	/// <summary>
	/// The role of the message, either system, user or assistant
	/// </summary>
	[JsonPropertyName("role")]
	public ChatRole? Role { get; set; }

	/// <summary>
	/// The content of the message
	/// </summary>
	[JsonPropertyName("content")]
	public string? Content { get; set; }

	/// <summary>
	/// Base64-encoded images (for multimodal models such as llava)
	/// </summary>
	[JsonPropertyName("images")]
	public string[]? Images { get; set; }
}