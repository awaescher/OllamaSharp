using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents the final message in a stream of responses from the /api/chat endpoint.
/// </summary>
public class ChatDoneResponseStream : ChatResponseStream
{
	/// <summary>
	/// The time spent generating the response
	/// </summary>
	[JsonPropertyName(Application.TotalDuration)]
	public long TotalDuration { get; set; }

	/// <summary>
	/// The time spent in nanoseconds loading the model
	/// </summary>
	[JsonPropertyName(Application.LoadDuration)]
	public long LoadDuration { get; set; }

	/// <summary>
	/// The number of tokens in the prompt
	/// </summary>
	[JsonPropertyName(Application.PromptEvalCount)]
	public int PromptEvalCount { get; set; }

	/// <summary>
	/// The time spent in nanoseconds evaluating the prompt
	/// </summary>
	[JsonPropertyName(Application.PromptEvalDuration)]
	public long PromptEvalDuration { get; set; }

	/// <summary>
	/// The number of tokens in the response
	/// </summary>
	[JsonPropertyName(Application.EvalCount)]
	public int EvalCount { get; set; }

	/// <summary>
	/// The time in nanoseconds spent generating the response
	/// </summary>
	[JsonPropertyName(Application.EvalDuration)]
	public long EvalDuration { get; set; }

	/// <summary>
	/// The reason for the completion of the chat
	/// </summary>
	[JsonPropertyName(Application.DoneReason)]
	public string? DoneReason { get; set; }
}