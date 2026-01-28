using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// The configuration information used for a chat completions request.
/// </summary>
public class RequestOptions
{
	/// <summary>
	/// Enable Mirostat sampling for controlling perplexity.
	/// (default: 0, 0 = disabled, 1 = Mirostat, 2 = Mirostat 2.0)
	/// </summary>
	[JsonPropertyName(Application.MiroStat)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MiroStat { get; set; }

	/// <summary>
	/// Influences how quickly the algorithm responds to feedback from the
	/// generated text. A lower learning rate will result in slower adjustments,
	/// while a higher learning rate will make the algorithm more responsive.
	/// (Default: 0.1)
	/// </summary>
	[JsonPropertyName(Application.MiroStatEta)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? MiroStatEta { get; set; }

	/// <summary>
	/// Controls the balance between coherence and diversity of the output.
	/// A lower value will result in more focused and coherent text.
	/// (Default: 5.0)
	/// </summary>
	[JsonPropertyName(Application.MiroStatTau)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? MiroStatTau { get; set; }

	/// <summary>
	/// Sets the size of the context window used to generate the next token.
	/// (Default: 2048)
	/// </summary>
	[JsonPropertyName(Application.NumCtx)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumCtx { get; set; }

	/// <summary>
	/// The number of GQA groups in the transformer layer. Required for some
	/// models, for example it is 8 for llama2:70b
	/// </summary>
	[JsonPropertyName(Application.NumGqa)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumGqa { get; set; }

	/// <summary>
	/// The number of layers to send to the GPU(s). On macOS it defaults to
	/// 1 to enable metal support, 0 to disable.
	/// </summary>
	[JsonPropertyName(Application.NumGpu)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumGpu { get; set; }

	/// <summary>
	/// This option controls which GPU is used for small tensors. The overhead of
	/// splitting the computation across all GPUs is not worthwhile. The GPU will
	/// use slightly more VRAM to store a scratch buffer for temporary results.
	/// By default, GPU 0 is used.
	/// </summary>
	[JsonPropertyName(Application.MainGpu)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? MainGpu { get; set; }

	/// <summary>
	/// Prompt processing maximum batch size.
	/// (Default: 512)
	/// </summary>
	[JsonPropertyName(Application.NumBatch)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumBatch { get; set; }

	/// <summary>
	/// Sets the number of threads to use during computation. By default,
	/// Ollama will detect this for optimal performance.
	/// It is recommended to set this value to the number of physical CPU cores
	/// your system has (as opposed to the logical number of cores).
	/// </summary>
	[JsonPropertyName(Application.NumThread)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumThread { get; set; }

	/// <summary>
	/// Number of tokens to keep from the initial prompt.
	/// (Default: 4, -1 = all)
	/// </summary>
	[JsonPropertyName(Application.NumKeep)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumKeep { get; set; }

	/// <summary>
	/// Sets how far back for the model to look back to prevent repetition.
	/// (Default: 64, 0 = disabled, -1 = num_ctx)
	/// </summary>
	[JsonPropertyName(Application.RepeatLastN)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? RepeatLastN { get; set; }

	/// <summary>
	/// Sets how strongly to penalize repetitions.
	/// A higher value (e.g., 1.5) will penalize repetitions more strongly,
	/// while a lower value (e.g., 0.9) will be more lenient. (Default: 1.1)
	/// </summary>
	[JsonPropertyName(Application.RepeatPenalty)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? RepeatPenalty { get; set; }

	/// <summary>
	/// The penalty to apply to tokens based on their presence in the prompt.
	/// (Default: 0.0)
	/// </summary>
	[JsonPropertyName(Application.PresencePenalty)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? PresencePenalty { get; set; }

	/// <summary>
	/// The penalty to apply to tokens based on their frequency in the prompt.
	/// (Default: 0.0)
	/// </summary>
	[JsonPropertyName(Application.FrequencyPenalty)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? FrequencyPenalty { get; set; }

	/// <summary>
	/// The temperature of the model. Increasing the temperature will make the
	/// model answer more creatively. (Default: 0.8)
	/// </summary>
	[JsonPropertyName(Application.Temperature)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? Temperature { get; set; }

	/// <summary>
	/// Sets the random number seed to use for generation.
	/// Setting this to a specific number will make the model generate the same
	/// text for the same prompt. (Default: 0)
	/// </summary>
	[JsonPropertyName(Application.Seed)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? Seed { get; set; }

	/// <summary>
	/// Sets the stop sequences to use. When this pattern is encountered the
	/// LLM will stop generating text and return. Multiple stop patterns may
	/// be set by specifying multiple separate stop parameters in a modelfile.
	/// </summary>
	[JsonPropertyName(Application.Stop)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Stop { get; set; }

	/// <summary>
	/// Tail free sampling is used to reduce the impact of less probable
	/// tokens from the output. A higher value (e.g., 2.0) will reduce the
	/// impact more, while a value of 1.0 disables this setting. (default: 1)
	/// </summary>
	[JsonPropertyName(Application.TfsZ)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? TfsZ { get; set; }

	/// <summary>
	/// Maximum number of tokens to predict when generating text.
	/// (Default: 128, -1 = infinite generation, -2 = fill context)
	/// </summary>
	[JsonPropertyName(Application.NumPredict)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? NumPredict { get; set; }

	/// <summary>
	/// Reduces the probability of generating nonsense. A higher value
	/// (e.g. 100) will give more diverse answers, while a lower value (e.g. 10)
	/// will be more conservative. (Default: 40)
	/// </summary>
	[JsonPropertyName(Application.TopK)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? TopK { get; set; }

	/// <summary>
	/// Works together with top-k. A higher value (e.g., 0.95) will lead to
	/// more diverse text, while a lower value (e.g., 0.5) will generate more
	/// focused and conservative text. (Default: 0.9)
	/// </summary>
	[JsonPropertyName(Application.TopP)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? TopP { get; set; }

	/// <summary>
	/// Alternative to the top_p, and aims to ensure a balance of quality and variety. min_p represents the minimum
	/// probability for a token to be considered, relative to the probability of the most likely token.For
	/// example, with min_p=0.05 and the most likely token having a probability of 0.9, logits with a value less
	/// than 0.05*0.9=0.045 are filtered out. (Default: 0.0)
	/// </summary>
	[JsonPropertyName(Application.MinP)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? MinP { get; set; }

	/// <summary>
	/// The typical-p value to use for sampling. Locally Typical Sampling implementation described in the paper
	/// https://arxiv.org/abs/2202.00666. (Default: 1.0)
	/// </summary>
	[JsonPropertyName(Application.TypicalP)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public float? TypicalP { get; set; }

	/// <summary>
	/// Penalize newline tokens (Default: True)
	/// </summary>
	[JsonPropertyName(Application.PenalizeNewline)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? PenalizeNewline { get; set; }

	/// <summary>
	/// Models are mapped into memory by default, which allows the system to
	/// load only the necessary parts as needed. Disabling mmap makes loading
	/// slower but reduces pageouts if you're not using mlock. If the model is
	/// bigger than your RAM, turning off mmap stops it from loading.
	/// (Default: True)
	/// </summary>
	[JsonPropertyName(Application.UseMmap)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? UseMmap { get; set; }

	/// <summary>
	/// Lock the model in memory to prevent swapping. This can improve
	/// performance, but it uses more RAM and may slow down loading.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.UseMlock)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? UseMlock { get; set; }

	/// <summary>
	/// Enable low VRAM mode.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.LowVram)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? LowVram { get; set; }

	/// <summary>
	/// Enable f16 key/value.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.F16kv)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? F16kv { get; set; }

	/// <summary>
	/// Return logits for all the tokens, not just the last one.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.LogitsAll)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? LogitsAll { get; set; }

	/// <summary>
	/// Load only the vocabulary, not the weights.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.VocabOnly)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? VocabOnly { get; set; }

	/// <summary>
	///  Enable NUMA support.
	/// (Default: False)
	/// </summary>
	[JsonPropertyName(Application.Numa)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? Numa { get; set; }
}