using System.Text.Json.Serialization;

public class StreamedResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

public class DoneStreamedResponse : StreamedResponse
{
	[JsonPropertyName("context")]
	public long[] Context { get; set; }

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

