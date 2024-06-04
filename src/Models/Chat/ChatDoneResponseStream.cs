using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

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