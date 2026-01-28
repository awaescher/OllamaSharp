using System.Globalization;
using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Generate a response for a given prompt with a provided model. This is a
/// streaming endpoint, so there will be a series of responses. The final
/// response object will include statistics and additional data from the request.
///
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-completion">Ollama API docs</see>
/// </summary>
public class GenerateRequest : OllamaRequest
{
	/// <summary>
	/// The model name (required)
	/// </summary>
	[JsonPropertyName(Application.Model)]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The prompt to generate a response for
	/// </summary>
	[JsonPropertyName(Application.Prompt)]
	public string Prompt { get; set; } = null!;

	/// <summary>
	/// Suffix for Fill-In-the-Middle generate
	/// </summary>
	[JsonPropertyName(Application.Suffix)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string Suffix { get; set; } = null!;

	/// <summary>
	/// Additional model parameters listed in the documentation for the
	/// Modelfile such as temperature
	/// </summary>
	[JsonPropertyName(Application.Options)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Base64-encoded images (for multimodal models such as llava)
	/// </summary>
	[JsonPropertyName(Application.Images)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Images { get; set; }

	/// <summary>
	/// System prompt to (overrides what is defined in the Modelfile)
	/// </summary>
	[JsonPropertyName(Application.System)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? System { get; set; }

	/// <summary>
	/// The full prompt or prompt template (overrides what is defined in the Modelfile)
	/// </summary>
	[JsonPropertyName(Application.Template)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Template { get; set; }

	/// <summary>
	/// The context parameter returned from a previous request to /generate,
	/// this can be used to keep a short conversational memory
	/// </summary>
	[JsonPropertyName(Application.Context)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public long[]? Context { get; set; }

	/// <summary>
	/// Gets or sets the KeepAlive property, which decides how long a given model should stay loaded.
	/// </summary>
	[JsonPropertyName(Application.KeepAlive)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }

	/// <summary>
	/// Gets or sets the format to return a response in. Currently accepts "json" or JsonSchema or null.
	/// </summary>
	[JsonPropertyName(Application.Format)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? Format { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object,
	/// rather than a stream of objects
	/// </summary>
	[JsonPropertyName(Application.Stream)]
	public bool Stream { get; set; } = true;

	/// <summary>
	/// In some cases you may wish to bypass the templating system and provide
	/// a full prompt. In this case, you can use the raw parameter to disable formatting.
	/// </summary>
	[JsonPropertyName(Application.Raw)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Raw { get; set; }

	/// <summary>
	/// When log probabilities are requested, response chunks will now include a Logprobs field with the token,
	/// log probability and raw bytes (for partial unicode).
	/// </summary>
	[JsonPropertyName(Application.Logprobs)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Logprobs { get; set; }

	/// <summary>
	/// When setting TopLogprobs, a number of most-likely tokens are also provided,
	/// making it possible to introspect alternative tokens.
	/// </summary>
	[JsonPropertyName(Application.TopLogprobs)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? TopLogprobs { get; set; }
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
	[JsonPropertyName(Application.Model)]
	public string Model { get; set; } = null!;

	/// <summary>
	/// Gets or sets the time the response was generated. 
	/// </summary>
	[JsonPropertyName(Application.CreatedAt)]
	public string? CreatedAtString
	{
		get => _createdAtString;
		set
		{
			_createdAtString = value;
			_createdAt =
				DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdAt)
					? createdAt
					: null;
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
	[JsonPropertyName(Application.Response)]
	public string Response { get; set; } = null!;

	/// <summary>
	/// Whether the response is complete
	/// </summary>
	[JsonPropertyName(Application.Done)]
	public bool Done { get; set; }

	/// <summary>
	/// Gets or sets the log probabilities of output tokens.
	/// </summary>
	[JsonPropertyName(Application.Logprobs)]
	public IEnumerable<Logprob>? Logprobs { get; set; }
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
	[JsonPropertyName(Application.Context)]
	public long[] Context { get; set; } = null!;

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
}

/// <summary>
/// Log probability information for a generated token
/// </summary>
public class Logprob
{
	/// <summary>
	/// Gets or sets the token text.
	/// </summary>
	[JsonPropertyName("token")]
	public string? Token { get; set; }

	/// <summary>
	/// Gets or sets the log probability of the token.
	/// </summary>
	[JsonPropertyName("logprob")]
	public double? LogProbability { get; set; }

	/// <summary>
	/// Gets or sets the raw byte representation of the token.
	/// </summary>
	[JsonPropertyName("bytes")]
	public int[]? Bytes { get; set; }

	/// <summary>
	/// Gets or sets the top alternative log probabilities for this token.
	/// </summary>
	[JsonPropertyName("top_logprobs")]
	public IEnumerable<Logprob>? TopLogprobs { get; set; }
}