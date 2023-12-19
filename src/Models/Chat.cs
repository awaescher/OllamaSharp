using System.Diagnostics;
using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-chat-completion
/// </summary>
public class ChatRequest
{
	/// <summary>
	/// The model name (required)
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; }

	/// <summary>
	/// The messages of the chat, this can be used to keep a chat memory
	/// </summary>
	[JsonPropertyName("messages")]
	public Message[] Messages { get; set; }

	/// <summary>
	/// Additional model parameters listed in the documentation for the Modelfile such as temperature
	/// </summary>
	[JsonPropertyName("options")]
	public string Options { get; set; }

	/// <summary>
	/// The full prompt or prompt template (overrides what is defined in the Modelfile)
	/// </summary>
	[JsonPropertyName("template")]
	public string Template { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object, rather than a stream of objects
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; } = true;
}

public class ChatResponseStream
{
	[JsonPropertyName("model")]
	public string Model { get; set; }

	[JsonPropertyName("created_at")]
	public string CreatedAt { get; set; }

	[JsonPropertyName("message")]
	public Message Message { get; set; }

	[JsonPropertyName("done")]
	public bool Done { get; set; }
}

public class ChatDoneResponseStream : ChatResponseStream
{
	[JsonPropertyName("total_duration")]
	public long TotalDuration { get; set; }

	[JsonPropertyName("load_duration")]
	public long LoadDuration { get; set; }

	[JsonPropertyName("prompt_eval_count")]
	public int PromptEvalCount { get; set; }

	[JsonPropertyName("prompt_eval_duration")]
	public long PromptEvalDuration { get; set; }

	[JsonPropertyName("eval_count")]
	public int EvalCount { get; set; }

	[JsonPropertyName("eval_duration")]
	public long EvalDuration { get; set; }
}

[DebuggerDisplay("{Role}: {Content}")]
public class Message
{
	/// <summary>
	/// The role of the message, either system, user or assistant
	/// </summary>
	[JsonPropertyName("role")]
	public string Role { get; set; }

	/// <summary>
	/// The content of the message
	/// </summary>
	[JsonPropertyName("content")]
	public string Content { get; set; }

	/// <summary>
	/// Base64-encoded images (for multimodal models such as llava)
	/// </summary>
	[JsonPropertyName("images")]
	public string[] Images { get; set; }
}
