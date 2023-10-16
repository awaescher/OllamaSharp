using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#create-a-model
/// </summary>
public class CreateModelRequest
{
	/// <summary>
	/// Name of the model to create
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }

	/// <summary>
	/// Path to the Modelfile
	/// </summary>
	[JsonPropertyName("path")]
	public string Path { get; set; }

	/// <summary>
	/// If false the response will be be returned as a single response object, rather than a stream of objects (optional)
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }
}

public class CreateStatus
{
	[JsonPropertyName("status")]
	public string Status { get; set; }
}