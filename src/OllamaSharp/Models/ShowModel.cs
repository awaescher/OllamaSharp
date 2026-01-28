using System.Text.Json.Serialization;
using OllamaSharp.Constants;

namespace OllamaSharp.Models;

/// <summary>
/// Show information about a model including details, modelfile, template, parameters, license, system prompt.<br/>
/// <see href="https://github.com/ollama/ollama/blob/main/docs/api.md#show-model-information">Ollama API docs</see>
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class ShowModelRequest : OllamaRequest
{
	/// <summary>
	/// Gets or sets the name of the model to show.
	/// </summary>
	[JsonPropertyName(Application.Model)]
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
	[JsonPropertyName(Application.License)]
	public string? License { get; set; }

	/// <summary>
	/// Gets or sets the Modelfile for the model.
	/// </summary>
	[JsonPropertyName(Application.ModelFile)]
	public string? Modelfile { get; set; }

	/// <summary>
	/// Gets or sets the parameters for the model.
	/// </summary>
	[JsonPropertyName(Application.Parameters)]
	public string? Parameters { get; set; }

	/// <summary>
	/// Gets or sets the template for the model.
	/// </summary>
	[JsonPropertyName(Application.Template)]
	public string? Template { get; set; }

	/// <summary>
	/// Gets or sets the system prompt for the model.
	/// </summary>
	[JsonPropertyName(Application.System)]
	public string? System { get; set; }

	/// <summary>
	/// Gets or sets additional details about the model.
	/// </summary>
	[JsonPropertyName(Application.Details)]
	public Details Details { get; set; } = null!;

	/// <summary>
	/// Gets or sets extra information about the model.
	/// </summary>
	[JsonPropertyName(Application.ModelInfo)]
	public ModelInfo Info { get; set; } = null!;

	/// <summary>
	/// Gets or sets extra information about the projector.
	/// </summary>
	[JsonPropertyName(Application.Projector)]
	public ProjectorInfo? Projector { get; set; } = null!;

	/// <summary>
	/// Gets or sets model capabilities such as completion and vision.
	/// </summary>
	[JsonPropertyName(Application.Capabilities)]
	public string[]? Capabilities { get; set; } = null!;
}

/// <summary>
/// Represents additional model information.
/// </summary>
public class ModelInfo
{
	/// <summary>
	/// Gets or sets the architecture of the model.
	/// </summary>
	[JsonPropertyName(Application.GeneralArchitecture)]
	public string? Architecture { get; set; }

	/// <summary>
	/// Gets or sets the file type of the model.
	/// </summary>
	[JsonPropertyName(Application.GeneralFileType)]
	public int? FileType { get; set; }

	/// <summary>
	/// Gets or sets the parameter count of the model.
	/// </summary>
	[JsonPropertyName(Application.GeneralParameterCount)]
	public long? ParameterCount { get; set; }

	/// <summary>
	/// Gets or sets the quantization version of the model.
	/// </summary>
	[JsonPropertyName(Application.GeneralQuantizationVersion)]
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
