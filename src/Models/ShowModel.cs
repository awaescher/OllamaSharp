using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#show-model-information
/// </summary>
public class ShowModelRequest
{
	/// <summary>
	/// The name of the model to show
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }
}

public class ShowModelResponse
{
	[JsonPropertyName("license")]
	public string License { get; set; }

	[JsonPropertyName("modelfile")]
	public string Modelfile { get; set; }

	[JsonPropertyName("parameters")]
	public string Parameters { get; set; }

	[JsonPropertyName("template")]
	public string Template { get; set; }
}