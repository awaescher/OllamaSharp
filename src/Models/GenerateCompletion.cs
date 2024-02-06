using System.Text.Json.Serialization;

namespace OllamaSharp.Models
{
	/// <summary>
	/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-completion
	/// </summary>
	public class GenerateCompletionRequest
	{
		/// <summary>
		/// The model name (required)
		/// </summary>
		[JsonPropertyName("model")]
		public string Model { get; set; }

		/// <summary>
		/// The prompt to generate a response for
		/// </summary>
		[JsonPropertyName("prompt")]
		public string Prompt { get; set; }

		/// <summary>
		/// Additional model parameters listed in the documentation for the Modelfile such as temperature
		/// </summary>
		[JsonPropertyName("options")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public RequestOptions Options { get; set; }

		/// <summary>
		/// Base64-encoded images (for multimodal models such as llava)
		/// </summary>
		[JsonPropertyName("images")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string[] Images { get; set; }

		/// <summary>
		/// System prompt to (overrides what is defined in the Modelfile)
		/// </summary>
		[JsonPropertyName("system")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string System { get; set; }

		/// <summary>
		/// The full prompt or prompt template (overrides what is defined in the Modelfile)
		/// </summary>
		[JsonPropertyName("template")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string Template { get; set; }

		/// <summary>
		/// The context parameter returned from a previous request to /generate, this can be used to keep a short conversational memory
		/// </summary>
		[JsonPropertyName("context")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public long[] Context { get; set; }

		/// <summary>
		/// Gets or sets the KeepAlive property, which decides how long a given model should stay loaded.
		/// </summary>
		[JsonPropertyName("keep_alive")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string KeepAlive { get; set; }

		/// <summary>
		/// The format to return a response in. Currently only accepts "json" or null.
		/// </summary>
		[JsonPropertyName("format")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string Format { get; set; }

		/// <summary>
		/// If false the response will be returned as a single response object, rather than a stream of objects
		/// </summary>
		[JsonPropertyName("stream")]
		public bool Stream { get; set; } = true;

		/// <summary>
		/// In some cases you may wish to bypass the templating system and provide a full prompt. In this case, you can use the raw parameter to disable formatting.
		/// </summary>
		[JsonPropertyName("raw")]
		public bool Raw { get; set; }
	}

	public class GenerateCompletionResponseStream
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

	public class GenerateCompletionDoneResponseStream : GenerateCompletionResponseStream
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
}
