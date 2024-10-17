using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents a message in a chat.
/// </summary>
[DebuggerDisplay("{Role}: {Content}")]
public class Message
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class with the specified role, content, and images.
	/// </summary>
	/// <param name="role">The role of the message, either system, user, or assistant.</param>
	/// <param name="content">The content of the message.</param>
	/// <param name="images">An array of base64-encoded images.</param>
	public Message(ChatRole role, string content, string[]? images)
	{
		Role = role;
		Content = content;
		Images = images;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class with the specified role and images.
	/// </summary>
	/// <param name="role">The role of the message, either system, user, or assistant.</param>
	/// <param name="images">An array of base64-encoded images.</param>
	public Message(ChatRole role, string[] images)
	{
		Role = role;
		Images = images;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class with the specified role and content.
	/// </summary>
	/// <param name="role">The role of the message, either system, user, or assistant.</param>
	/// <param name="content">The content of the message.</param>
	public Message(ChatRole? role, string content)
	{
		Role = role;
		Content = content;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class.
	/// Required for JSON deserialization.
	/// </summary>
	public Message()
	{
	}

	/// <summary>
	/// Gets or sets the role of the message, either system, user, or assistant.
	/// </summary>
	[JsonPropertyName("role")]
	public ChatRole? Role { get; set; }

	/// <summary>
	/// Gets or sets the content of the message.
	/// </summary>
	[JsonPropertyName("content")]
	public string? Content { get; set; }

	/// <summary>
	/// Gets or sets an array of base64-encoded images (for multimodal models such as llava).
	/// </summary>
	[JsonPropertyName("images")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Images { get; set; }

	/// <summary>
	/// Gets or sets a list of tools the model wants to use (for models that support function calls, such as llama3.1).
	/// </summary>
	[JsonPropertyName("tool_calls")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<ToolCall>? ToolCalls { get; set; }

	/// <summary>
	/// Represents a tool call within a message.
	/// </summary>
	public class ToolCall
	{
		/// <summary>
		/// Gets or sets the function to be called by the tool.
		/// </summary>
		[JsonPropertyName("function")]
		public Function? Function { get; set; }
	}

	/// <summary>
	/// Represents a function that can be called by a tool.
	/// </summary>
	public class Function
	{
		/// <summary>
		/// Gets or sets the name of the function.
		/// </summary>
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		/// <summary>
		/// Gets or sets the arguments for the function, represented as a dictionary of argument names and values.
		/// </summary>
		[JsonPropertyName("arguments")]
		public IDictionary<string, object?>? Arguments { get; set; }
	}
}
