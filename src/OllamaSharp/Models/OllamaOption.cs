using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Collection of options available to Ollama
/// </summary>
/// <param name="name">The name of the setting like defined in the Ollama api docs</param>
public class OllamaOption(string name)
{
	/// <summary>
	/// Gets the name of the Ollama setting
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	/// Enable f16 key/value.
	/// (Default: False)
	/// </summary>
	public static OllamaOption F16kv { get; } = new(Application.F16kv);

	/// <summary>
	/// The penalty to apply to tokens based on their frequency in the prompt.
	/// (Default: 0.0)
	/// </summary>
	public static OllamaOption FrequencyPenalty { get; } = new(Application.FrequencyPenalty);

	/// <summary>
	/// Return logits for all the tokens, not just the last one.
	/// (Default: False)
	/// </summary>
	public static OllamaOption LogitsAll { get; } = new(Application.LogitsAll);

	/// <summary>
	/// Enable low VRAM mode.
	/// (Default: False)
	/// </summary>
	public static OllamaOption LowVram { get; } = new(Application.LowVram);

	/// <summary>
	/// This option controls which GPU is used for small tensors. The overhead of
	/// splitting the computation across all GPUs is not worthwhile. The GPU will
	/// use slightly more VRAM to store a scratch buffer for temporary results.
	/// By default, GPU 0 is used.
	/// </summary>
	public static OllamaOption MainGpu { get; } = new(Application.MainGpu);

	/// <summary>
	/// Alternative to the top_p, and aims to ensure a balance of quality and variety.min_p represents the minimum
	/// probability for a token to be considered, relative to the probability of the most likely token.For
	/// example, with min_p=0.05 and the most likely token having a probability of 0.9, logits with a value less
	/// than 0.05*0.9=0.045 are filtered out. (Default: 0.0)
	/// </summary>
	public static OllamaOption MinP { get; } = new(Application.MinP);

	/// <summary>
	/// Enable Mirostat sampling for controlling perplexity.
	/// (default: 0, 0 = disabled, 1 = Mirostat, 2 = Mirostat 2.0)
	/// </summary>
	public static OllamaOption MiroStat { get; } = new(Application.MiroStat);

	/// <summary>
	/// Influences how quickly the algorithm responds to feedback from the
	/// generated text. A lower learning rate will result in slower adjustments,
	/// while a higher learning rate will make the algorithm more responsive.
	/// (Default: 0.1)
	/// </summary>
	public static OllamaOption MiroStatEta { get; } = new(Application.MiroStatEta);

	/// <summary>
	/// Controls the balance between coherence and diversity of the output.
	/// A lower value will result in more focused and coherent text.
	/// (Default: 5.0)
	/// </summary>
	public static OllamaOption MiroStatTau { get; } = new(Application.MiroStatTau);

	/// <summary>
	///  Enable NUMA support.
	/// (Default: False)
	/// </summary>
	public static OllamaOption Numa { get; } = new(Application.Numa);

	/// <summary>
	/// Prompt processing maximum batch size.
	/// (Default: 512)
	/// </summary>
	public static OllamaOption NumBatch { get; } = new(Application.NumBatch);

	/// <summary>
	/// Sets the size of the context window used to generate the next token.
	/// (Default: 2048)
	/// </summary>
	public static OllamaOption NumCtx { get; } = new(Application.NumCtx);

	/// <summary>
	/// The number of layers to send to the GPU(s). On macOS it defaults to
	/// 1 to enable metal support, 0 to disable.
	/// </summary>
	public static OllamaOption NumGpu { get; } = new(Application.NumGpu);

	/// <summary>
	/// The number of GQA groups in the transformer layer. Required for some
	/// models, for example it is 8 for llama2:70b
	/// </summary>
	public static OllamaOption NumGqa { get; } = new(Application.NumGqa);

	/// <summary>
	/// Number of tokens to keep from the initial prompt.
	/// (Default: 4, -1 = all)
	/// </summary>
	public static OllamaOption NumKeep { get; } = new(Application.NumKeep);

	/// <summary>
	/// Maximum number of tokens to predict when generating text.
	/// (Default: 128, -1 = infinite generation, -2 = fill context)
	/// </summary>
	public static OllamaOption NumPredict { get; } = new(Application.NumPredict);

	/// <summary>
	/// The number of tokens to generate in the output.
	///	(Default: -1, infinite generation)
	/// </summary>
	public static OllamaOption MaxOutputTokens { get; } = new(Application.MaxOutputTokens);

	/// <summary>
	/// Sets the number of threads to use during computation. By default,
	/// Ollama will detect this for optimal performance.
	/// It is recommended to set this value to the number of physical CPU cores
	/// your system has (as opposed to the logical number of cores).
	/// </summary>
	public static OllamaOption NumThread { get; } = new(Application.NumThread);

	/// <summary>
	/// Penalize newline tokens (Default: True)
	/// </summary>
	public static OllamaOption PenalizeNewline { get; } = new(Application.PenalizeNewline);

	/// <summary>
	/// The penalty to apply to tokens based on their presence in the prompt.
	/// (Default: 0.0)
	/// </summary>
	public static OllamaOption PresencePenalty { get; } = new(Application.PresencePenalty);

	/// <summary>
	/// Sets how far back for the model to look back to prevent repetition.
	/// (Default: 64, 0 = disabled, -1 = num_ctx)
	/// </summary>
	public static OllamaOption RepeatLastN { get; } = new(Application.RepeatLastN);

	/// <summary>
	/// Sets how strongly to penalize repetitions.
	/// A higher value (e.g., 1.5) will penalize repetitions more strongly,
	/// while a lower value (e.g., 0.9) will be more lenient. (Default: 1.1)
	/// </summary>
	public static OllamaOption RepeatPenalty { get; } = new(Application.RepeatPenalty);

	/// <summary>
	/// Sets the random number seed to use for generation.
	/// Setting this to a specific number will make the model generate the same
	/// text for the same prompt. (Default: 0)
	/// </summary>
	public static OllamaOption Seed { get; } = new(Application.Seed);

	/// <summary>
	/// Sets the stop sequences to use. When this pattern is encountered the
	/// LLM will stop generating text and return. Multiple stop patterns may
	/// be set by specifying multiple separate stop parameters in a modelfile.
	/// </summary>
	public static OllamaOption Stop { get; } = new(Application.Stop);

	/// <summary>
	/// The temperature of the model. Increasing the temperature will make the
	/// model answer more creatively. (Default: 0.8)
	/// </summary>
	public static OllamaOption Temperature { get; } = new(Application.Temperature);

	/// <summary>
	/// Tail free sampling is used to reduce the impact of less probable
	/// tokens from the output. A higher value (e.g., 2.0) will reduce the
	/// impact more, while a value of 1.0 disables this setting. (default: 1)
	/// </summary>
	public static OllamaOption TfsZ { get; } = new(Application.TfsZ);

	/// <summary>
	/// Reduces the probability of generating nonsense. A higher value
	/// (e.g. 100) will give more diverse answers, while a lower value (e.g. 10)
	/// will be more conservative. (Default: 40)
	/// </summary>
	public static OllamaOption TopK { get; } = new(Application.TopK);

	/// <summary>
	/// Works together with top-k. A higher value (e.g., 0.95) will lead to
	/// more diverse text, while a lower value (e.g., 0.5) will generate more
	/// focused and conservative text. (Default: 0.9)
	/// </summary>
	public static OllamaOption TopP { get; } = new(Application.TopP);

	/// <summary>
	/// The typical-p value to use for sampling. Locally Typical Sampling implementation described in the paper
	/// https://arxiv.org/abs/2202.00666. (Default: 1.0)
	/// </summary>
	public static OllamaOption TypicalP { get; } = new(Application.TypicalP);

	/// <summary>
	/// Lock the model in memory to prevent swapping. This can improve
	/// performance, but it uses more RAM and may slow down loading.
	/// (Default: False)
	/// </summary>
	public static OllamaOption UseMlock { get; } = new(Application.UseMlock);

	/// <summary>
	/// Models are mapped into memory by default, which allows the system to
	/// load only the necessary parts as needed. Disabling mmap makes loading
	/// slower but reduces pageouts if you're not using mlock. If the model is
	/// bigger than your RAM, turning off mmap stops it from loading.
	/// (Default: True)
	/// </summary>
	public static OllamaOption UseMmap { get; } = new(Application.UseMmap);

	/// <summary>
	/// Load only the vocabulary, not the weights.
	/// (Default: False)
	/// </summary>
	public static OllamaOption VocabOnly { get; } = new(Application.VocabOnly);

	/// <summary>
	/// Gets or sets a value to enable or disable thinking. Use reasoning models like OpenThinker, Qwen3,
	/// DeepSeek-R1, Phi4-Reasoning that support thinking when activating this option.
	/// (Default: Null)
	/// </summary>
	public static OllamaOption Think { get; } = new(Application.Think);
}
