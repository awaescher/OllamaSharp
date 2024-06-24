using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#show-model-information
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class ShowModelRequest
{
	/// <summary>
	///  The name of the model to show
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}

public class ShowModelResponse
{
	/// <summary>
	/// The license for the model
	/// </summary>
	[JsonPropertyName("license")]
	public string? License { get; set; }

	/// <summary>
	/// The Modelfile for the model
	/// </summary>
	[JsonPropertyName("modelfile")]
	public string? Modelfile { get; set; }

	/// <summary>
	/// The parameters for the model
	/// </summary>
	[JsonPropertyName("parameters")]
	public string? Parameters { get; set; }

	/// <summary>
	/// The template for the model
	/// </summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>
	/// The system prompt for the model
	/// </summary>
	[JsonPropertyName("system")]
	public string? System { get; set; }

	/// <summary>
	/// Additional details about the model
	/// </summary>
	[JsonPropertyName("details")]
	public Details Details { get; set; } = null!;

	/// <summary>
	/// Extra information about the model
	/// </summary>
	[JsonPropertyName("model_info")]
	public ModelInfo Info { get; set; } = null!;
}

public class ModelInfo
{
	[JsonPropertyName("general.architecture")]
	public string? Architecture { get; set; }

	[JsonPropertyName("general.file_type")]
	public int? FileType { get; set; }

	[JsonPropertyName("general.parameter_count")]
	public long? ParameterCount { get; set; }

	[JsonPropertyName("general.quantization_version")]
	public int? QuantizationVersion { get; set; }

	[JsonExtensionData]
	public IDictionary<string, object>? ExtraInfo { get; set; }
}