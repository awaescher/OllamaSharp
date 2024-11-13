using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// Show information about a model including details, modelfile, template,
/// parameters, license, system prompt.
///
/// <see href="https://github.com/ollama/ollama/blob/main/docs/api.md#show-model-information">Ollama API docs</see>
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class ShowModelRequest : OllamaRequest
{
	/// <summary>
	/// Gets or sets the name of the model to show.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}

/// <summary>
/// Represents the response containing detailed model information.
/// </summary>
public class ShowModelResponse
{
	/// <summary>
	/// Gets or sets the license for the model.
	/// </summary>
	[JsonPropertyName("license")]
	public string? License { get; set; }

	/// <summary>
	/// Gets or sets the Modelfile for the model.
	/// </summary>
	[JsonPropertyName("modelfile")]
	public string? Modelfile { get; set; }

	/// <summary>
	/// Gets or sets the parameters for the model.
	/// </summary>
	[JsonPropertyName("parameters")]
	public string? Parameters { get; set; }

	/// <summary>
	/// Gets or sets the template for the model.
	/// </summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>
	/// Gets or sets the system prompt for the model.
	/// </summary>
	[JsonPropertyName("system")]
	public string? System { get; set; }

	/// <summary>
	/// Gets or sets additional details about the model.
	/// </summary>
	[JsonPropertyName("details")]
	public Details Details { get; set; } = null!;

	/// <summary>
	/// Gets or sets extra information about the model.
	/// </summary>
	[JsonPropertyName("model_info")]
	public ModelInfo Info { get; set; } = null!;

	/// <summary>
	/// Gets or sets extra information about the projector.
	/// </summary>
	[JsonPropertyName("projector_info")]
	public ProjectorInfo? Projector { get; set; } = null!;
}

/// <summary>
/// Represents additional model information.
/// </summary>
public class ModelInfo
{
	/// <summary>
	/// Gets or sets the architecture of the model.
	/// </summary>
	[JsonPropertyName("general.architecture")]
	public string? Architecture { get; set; }

	/// <summary>
	/// Gets or sets the file type of the model.
	/// </summary>
	[JsonPropertyName("general.file_type")]
	public int? FileType { get; set; }

	/// <summary>
	/// Gets or sets the parameter count of the model.
	/// </summary>
	[JsonPropertyName("general.parameter_count")]
	public long? ParameterCount { get; set; }

	/// <summary>
	/// Gets or sets the quantization version of the model.
	/// </summary>
	[JsonPropertyName("general.quantization_version")]
	public int? QuantizationVersion { get; set; }

	/// <summary>
	/// Gets or sets additional information as a dictionary.
	/// </summary>
	[JsonExtensionData]
	public IDictionary<string, object>? ExtraInfo { get; set; }
}

/// <summary>
/// Represents projector-specific information.
/// </summary>
public class ProjectorInfo
{
	/// <summary>
	/// Gets or sets additional projector information as a dictionary.
	/// </summary>
	[JsonExtensionData]
	public IDictionary<string, object>? ExtraInfo { get; set; }
}
