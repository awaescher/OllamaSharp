using System.Collections.Generic;
using System.Text;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// A message builder that can build messages from streamed chunks
/// </summary>
public class MessageBuilder
{
	private readonly StringBuilder _contentBuilder = new();

	/// <summary>
	/// Appends a message chunk to build the final message
	/// </summary>
	/// <param name="chunk">The message chunk to append to the final message</param>
	public void Append(ChatResponseStream? chunk)
	{
		if (chunk?.Message is null)
			return;

		_contentBuilder.Append(chunk.Message.Content ?? "");
		Role = chunk.Message.Role;

		if (chunk.Message.Images is not null)
			Images.AddRange(chunk.Message.Images);

		if (chunk.Message.ToolCalls is not null)
			ToolCalls.AddRange(chunk.Message.ToolCalls);
	}

	/// <summary>
	/// Builds the message out of the streamed chunks that were appended before
	/// </summary>
	public Message ToMessage()
	{
		return new Message
		{
			Content = _contentBuilder.ToString(),
			Images = Images.ToArray(),
			Role = Role,
			ToolCalls = ToolCalls
		};
	}

	/// <summary>
	/// The role of the message, either system, user or assistant
	/// </summary>
	public ChatRole? Role { get; set; }

	/// <summary>
	/// Base64-encoded images (for multimodal models such as llava)
	/// </summary>
	public List<string> Images { get; set; } = [];

	/// <summary>
	/// A list of tools the model wants to use (for models that support function calls, such as llama3.1)
	/// </summary>
	public List<Message.ToolCall> ToolCalls { get; set; } = [];

	/// <summary>
	/// Gets whether the message builder received message chunks yet
	/// </summary>
	public bool HasValue => _contentBuilder.Length > 0 || ToolCalls.Count > 0;
}
