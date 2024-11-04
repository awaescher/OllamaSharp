namespace OllamaSharp.MicrosoftAi;

using System.Text.Json;

internal sealed class OllamaFunctionResultContent
{
	public string? CallId { get; set; }
	public JsonElement Result { get; set; }
}