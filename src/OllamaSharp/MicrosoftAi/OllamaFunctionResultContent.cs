namespace OllamaSharp.MicrosoftAi;

using System.Text.Json;

/// <summary>
/// A holder for the result of an Ollama function call.
/// </summary>
internal sealed class OllamaFunctionResultContent
{
	/// <summary>
	/// The function call ID for which this is the result.
	/// </summary>
	public string? CallId { get; set; }

	/// <summary>
	/// This element value may be <see langword="null" /> if the function returned <see langword="null" />,
	/// if the function was void-returning and thus had no result, or if the function call failed.
	/// Typically, however, in order to provide meaningfully representative information to an AI service,
	/// a human-readable representation of those conditions should be supplied.
	/// </summary>
	public JsonElement Result { get; set; }
}