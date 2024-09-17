using System;
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
	/// Total duration to process the prompt
	/// </summary>
	[JsonPropertyName("total_duration")]
	public int TotalDuration { get; set; }

	/// <summary>
	/// Duration to load the model
	/// </summary>
	[JsonPropertyName("load_duration")]
	public int LoadDuration { get; set; }

	/// <summary>
	/// Prompt evaluation steps
	/// </summary>
	[JsonPropertyName("prompt_eval_count")]
	public int PromptEvalCount { get; set; }

	/// <summary>
	/// Prompt evaluation duration
	/// </summary>
	[JsonPropertyName("prompt_eval_duration")]
	public int PromptEvalDuration { get; set; }

	/// <summary>
	/// Evaluation duration
	/// </summary>
	[JsonPropertyName("eval_count")]
	public int EvalCount { get; set; }

	/// <summary>
	/// Evaluation duration
	/// </summary>
	[JsonPropertyName("eval_duration")]
	public int EvalDuration { get; set; }
}