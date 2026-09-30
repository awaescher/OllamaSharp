using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Answers choice, yes/no and scoring questions about a shared state with a local System One model.
/// Requires Ollama v0.35.0 or later. Streaming, images, tools and generation controls are not supported.
///
/// <see href="https://docs.ollama.com/api/systemone">Ollama API docs</see>
/// </summary>
public class SystemOneRequest : OllamaRequest
{
	/// <summary>
	/// The name of a local model trained for System One, such as <c>nimble</c>.
	/// Cloud models and MLX/Safetensors models are not supported.
	/// </summary>
	[JsonPropertyName(Application.Model)]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The shared state that all questions are asked about. Can be a nonempty string,
	/// or an object or array that gets serialized as JSON text. Not interpreted as
	/// chat messages or multimodal input.
	/// </summary>
	[JsonPropertyName(Application.State)]
	public object State { get; set; } = null!;

	/// <summary>
	/// The named questions about the shared state. Each question is scored separately
	/// against the full state; answers are not passed to later questions.
	/// </summary>
	[JsonPropertyName(Application.Questions)]
	public Dictionary<string, SystemOneQuestion> Questions { get; set; } = null!;

	/// <summary>
	/// Gets or sets how long the model should stay loaded after the request, as a
	/// duration string (such as <c>5m</c>) or seconds. Zero unloads after the request;
	/// a negative value keeps it loaded.
	/// </summary>
	[JsonPropertyName(Application.KeepAlive)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }
}

/// <summary>
/// A single named question asked by a <see cref="SystemOneRequest"/>.
/// Use <see cref="SystemOneChoiceQuestion"/>, <see cref="SystemOneNoulQuestion"/> or
/// <see cref="SystemOneScoreQuestion"/> to create one.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = Application.Type)]
[JsonDerivedType(typeof(SystemOneChoiceQuestion), Application.Choice)]
[JsonDerivedType(typeof(SystemOneNoulQuestion), Application.Noul)]
[JsonDerivedType(typeof(SystemOneScoreQuestion), Application.Score)]
public abstract class SystemOneQuestion
{
	/// <summary>
	/// The type discriminator of this question.
	/// </summary>
	[JsonIgnore]
	public abstract string Type { get; }

	/// <summary>
	/// The instructions shown to the model for this question. Can be a nonempty string,
	/// or an object or array that gets serialized as JSON text.
	/// </summary>
	[JsonPropertyName(Application.Instructions)]
	public object Instructions { get; set; } = null!;
}

/// <summary>
/// A question that picks one option out of a set of named criteria.
/// </summary>
public class SystemOneChoiceQuestion : SystemOneQuestion
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Choice;

	/// <summary>
	/// The option keys mapped to their descriptions. A null description uses the key
	/// itself. Requires between 2 and 26 options.
	/// </summary>
	[JsonPropertyName(Application.Criteria)]
	public Dictionary<string, string?> Criteria { get; set; } = null!;
}

/// <summary>
/// A yes/no (noul) question.
/// </summary>
public class SystemOneNoulQuestion : SystemOneQuestion
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Noul;

	/// <summary>
	/// Optional descriptions for the two outcomes. Omitted entries use <c>No</c> and <c>Yes</c>.
	/// </summary>
	[JsonPropertyName(Application.Criteria)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public SystemOneNoulCriteria? Criteria { get; set; }
}

/// <summary>
/// Descriptions for the <c>false</c> and <c>true</c> outcomes of a <see cref="SystemOneNoulQuestion"/>.
/// </summary>
public class SystemOneNoulCriteria
{
	/// <summary>
	/// The description of the false outcome. Defaults to <c>No</c>.
	/// </summary>
	[JsonPropertyName("false")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? No { get; set; }

	/// <summary>
	/// The description of the true outcome. Defaults to <c>Yes</c>.
	/// </summary>
	[JsonPropertyName("true")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Yes { get; set; }
}

/// <summary>
/// A question that scores the state on a scale defined by an ordered list of criteria.
/// </summary>
public class SystemOneScoreQuestion : SystemOneQuestion
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Score;

	/// <summary>
	/// The criteria ordered from the lowest score (index 0) to the highest. Defines a
	/// scale from 0 to the number of criteria minus 1. Requires between 2 and 26 criteria.
	/// </summary>
	[JsonPropertyName(Application.Criteria)]
	public List<string> Criteria { get; set; } = null!;
}

/// <summary>
/// The response from the <c>/v1/systemone</c> endpoint.
/// </summary>
public class SystemOneResponse
{
	/// <summary>
	/// The model name from the request.
	/// </summary>
	[JsonPropertyName(Application.Model)]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The answers keyed by the question names in the request.
	/// </summary>
	[JsonPropertyName(Application.Answers)]
	public Dictionary<string, SystemOneAnswer> Answers { get; set; } = null!;

	/// <summary>
	/// The token usage of the request.
	/// </summary>
	[JsonPropertyName(Application.Usage)]
	public SystemOneUsage Usage { get; set; } = null!;
}

/// <summary>
/// The token usage of a <see cref="SystemOneRequest"/>.
/// </summary>
public class SystemOneUsage
{
	/// <summary>
	/// The sum of the full rendered prompt lengths across all questions, including
	/// repeated shared context even when cached.
	/// </summary>
	[JsonPropertyName(Application.InputTokens)]
	public int InputTokens { get; set; }

	/// <summary>
	/// The tokens generated internally for scoring, including prefix preparation and
	/// retries. May exceed the question count.
	/// </summary>
	[JsonPropertyName(Application.OutputTokens)]
	public int OutputTokens { get; set; }
}

/// <summary>
/// The answer to a single named question. Cast to <see cref="SystemOneChoiceAnswer"/>,
/// <see cref="SystemOneNoulAnswer"/> or <see cref="SystemOneScoreAnswer"/> based on the
/// requested <see cref="SystemOneQuestion"/>.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = Application.Type)]
[JsonDerivedType(typeof(SystemOneChoiceAnswer), Application.Choice)]
[JsonDerivedType(typeof(SystemOneNoulAnswer), Application.Noul)]
[JsonDerivedType(typeof(SystemOneScoreAnswer), Application.Score)]
public abstract class SystemOneAnswer
{
	/// <summary>
	/// The type discriminator of this answer.
	/// </summary>
	[JsonIgnore]
	public abstract string Type { get; }
}

/// <summary>
/// The answer to a <see cref="SystemOneChoiceQuestion"/>.
/// </summary>
public class SystemOneChoiceAnswer : SystemOneAnswer
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Choice;

	/// <summary>
	/// The option key with the highest probability. Ties select the first option in request order.
	/// </summary>
	[JsonPropertyName(Application.Choice)]
	public string Choice { get; set; } = null!;

	/// <summary>
	/// The probabilities of all options, normalized over the supplied candidates.
	/// </summary>
	[JsonPropertyName(Application.Probabilities)]
	public Dictionary<string, double> Probabilities { get; set; } = null!;

	/// <summary>
	/// The distribution concentration between 0 (uniform) and 1 (one candidate dominates).
	/// Not calibrated correctness.
	/// </summary>
	[JsonPropertyName(Application.Confidence)]
	public double Confidence { get; set; }
}

/// <summary>
/// The answer to a <see cref="SystemOneNoulQuestion"/>.
/// </summary>
public class SystemOneNoulAnswer : SystemOneAnswer
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Noul;

	/// <summary>
	/// The probability of true among the false and true candidates. This is a number
	/// between 0 and 1, not a Boolean.
	/// </summary>
	[JsonPropertyName(Application.Noul)]
	public double Noul { get; set; }
}

/// <summary>
/// The answer to a <see cref="SystemOneScoreQuestion"/>.
/// </summary>
public class SystemOneScoreAnswer : SystemOneAnswer
{
	/// <inheritdoc />
	[JsonIgnore]
	public override string Type => Application.Score;

	/// <summary>
	/// The probability-weighted average of the zero-based criterion indices. Not rounded
	/// to a level and not normalized to 0-1.
	/// </summary>
	[JsonPropertyName(Application.Score)]
	public double Score { get; set; }

	/// <summary>
	/// The zero-based indices as string keys mapped to the criterion descriptions.
	/// </summary>
	[JsonPropertyName(Application.Legend)]
	public Dictionary<string, string> Legend { get; set; } = null!;

	/// <summary>
	/// The probabilities keyed by zero-based criterion indices as strings.
	/// </summary>
	[JsonPropertyName(Application.Probabilities)]
	public Dictionary<string, double> Probabilities { get; set; } = null!;

	/// <summary>
	/// The distribution concentration between 0 (uniform) and 1 (one candidate dominates).
	/// Not calibrated correctness.
	/// </summary>
	[JsonPropertyName(Application.Confidence)]
	public double Confidence { get; set; }
}
