using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

public class ChatResponse
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

	/// <summary>
	/// The reason for the completion of the chat
	/// </summary>
	[JsonPropertyName("done_reason")]
	public string? DoneReason { get; set; }

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