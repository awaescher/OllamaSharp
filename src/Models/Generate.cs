using System;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-completion
/// </summary>
public class GenerateRequest : OllamaRequest
{
	/// <summary>
	/// The model name (required)
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The prompt to generate a response for
	/// </summary>
	[JsonPropertyName("prompt")]
	public string Prompt { get; set; } = null!;

	/// <summary>
	/// Suffix for Fill-In-the-Middle generate
	/// </summary>
	[JsonPropertyName("suffix")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string Suffix { get; set; } = null!;

	/// <summary>
	/// Additional model parameters listed in the documentation for the
	/// Modelfile such as temperature
	/// </summary>
	[JsonPropertyName("options")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Base64-encoded images (for multimodal models such as llava)
	/// </summary>
	[JsonPropertyName("images")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Images { get; set; }

	/// <summary>
	/// System prompt to (overrides what is defined in the Modelfile)
	/// </summary>
	[JsonPropertyName("system")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? System { get; set; }

	/// <summary>
	/// The full prompt or prompt template (overrides what is defined in the Modelfile)
	/// </summary>
	[JsonPropertyName("template")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Template { get; set; }

	/// <summary>
	/// The context parameter returned from a previous request to /generate,
	/// this can be used to keep a short conversational memory
	/// </summary>
	[JsonPropertyName("context")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public long[]? Context { get; set; }

	/// <summary>
	/// Gets or sets the KeepAlive property, which decides how long a given model should stay loaded.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }

	/// <summary>
	/// Gets or sets the format to return a response in. Currently accepts "json" and JsonSchema or null.
	/// </summary>
	[JsonPropertyName("format")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? Format { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object,
	/// rather than a stream of objects
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; } = true;

	/// <summary>
	/// In some cases you may wish to bypass the templating system and provide
	/// a full prompt. In this case, you can use the raw parameter to disable formatting.
	/// </summary>
	[JsonPropertyName("raw")]
	public bool Raw { get; set; }
}

/// <summary>
/// The response from the /api/generate endpoint when streaming is enabled
/// </summary>
public class GenerateResponseStream
{
	private DateTimeOffset? _createdAt = null!;
	private string? _createdAtString = null!;

	/// <summary>
	/// The model that generated the response
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// Gets or sets the time the response was generated. 
	/// </summary>
	[JsonPropertyName("created_at")]
	public string? CreatedAtString
	{
		get => _createdAtString;
		set
		{
			_createdAtString = value;
			_createdAt = DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var createdAt) ? createdAt : null;
		}
	}

	/// <summary>
	/// Gets or sets the time the response was generated.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CreatedAt
	{
		get => _createdAt;
		set
		{
			_createdAt = value;
			_createdAtString = value?.ToString("o");
		}
	}

	/// <summary>
	/// The response generated by the model
	/// </summary>
	[JsonPropertyName("response")]
	public string Response { get; set; } = null!;

	/// <summary>
	/// Whether the response is complete
	/// </summary>
	[JsonPropertyName("done")]
	public bool Done { get; set; }
}

/// <summary>
/// Represents the final response from the /api/generate endpoint
/// </summary>
public class GenerateDoneResponseStream : GenerateResponseStream
{
	/// <summary>
	/// An encoding of the conversation used in this response, this can be
	/// sent in the next request to keep a conversational memory
	/// </summary>
	[JsonPropertyName("context")]
	public long[] Context { get; set; } = null!;

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