using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#copy-a-model
/// </summary>
public class CopyModelRequest
{
	[JsonPropertyName("source")]
	public string Source { get; set; }

	[JsonPropertyName("destination")]
	public string Destination { get; set; }
}