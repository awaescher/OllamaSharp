using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents the final message in a stream of responses from the /api/chat endpoint.
/// </summary>
public class ChatDoneResponseStream : ChatResponseStream
{
	/// <summary>
	/// The time spent generating the response
	/// </summary>
	[JsonPropertyName("total_duration")]
	public long TotalDuration { get; set; }

	/// <summary>
	/// The time spent in nanoseconds loading the model
	/// </summary>
	[JsonPropertyName("load_duration")]
	public long LoadDuration { get; set; }

	/// <summary>
	/// The number of tokens in the prompt
	/// </summary>
	[JsonPropertyName("prompt_eval_count")]
	public int PromptEvalCount { get; set; }

	/// <summary>
	/// The time spent in nanoseconds evaluating the prompt
	/// </summary>
	[JsonPropertyName("prompt_eval_duration")]
	public long PromptEvalDuration { get; set; }

	/// <summary>
	/// The number of tokens in the response
	/// </summary>
	[JsonPropertyName("eval_count")]
	public int EvalCount { get; set; }

	/// <summary>
	/// The time in nanoseconds spent generating the response
	/// </summary>
	[JsonPropertyName("eval_duration")]
	public long EvalDuration { get; set; }
}