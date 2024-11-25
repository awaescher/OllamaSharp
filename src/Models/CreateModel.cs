using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Create a model from a Modelfile. It is recommended to set <see cref="ModelFileContent"/> to the
/// content of the Modelfile rather than just set path. This is a requirement
/// for remote create. Remote model creation must also create any file blobs,
/// fields such as FROM and ADAPTER, explicitly with the server using Create a
/// Blob and the value to the path indicated in the response.
///
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#create-a-model">Ollama API docs</see>
/// 
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class CreateModelRequest : OllamaRequest
{
	/// <summary>
	/// Name of the model to create
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// Contents of the Modelfile
	/// See https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md
	/// </summary>
	[JsonPropertyName("modelfile")]
	public string ModelFileContent { get; set; } = null!;

	/// <summary>
	/// Path to the Modelfile (optional)
	/// </summary>
	[JsonPropertyName("path")]
	public string? Path { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object, rather than a stream of objects (optional)
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }

	/// <summary>
	/// Set the quantization level for quantize model when importing (e.g. q4_0, optional)
	/// </summary>
	[JsonPropertyName("quantize")]
	public string? Quantize { get; set; }
}

/// <summary>
/// Represents the response from the /api/create endpoint
/// </summary>
public class CreateModelResponse
{
	/// <summary>
	/// Represents the status of a model creation.
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;
}