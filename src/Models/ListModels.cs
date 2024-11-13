using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// List models that are available locally.
/// 
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md#list-local-models">Ollama API docs</see>
/// </summary>
public class ListModelsResponse
{
	/// <summary>
	/// Gets or sets the array of models returned by the API.
	/// </summary>
	[JsonPropertyName("models")]
	public Model[] Models { get; set; } = null!;
}

/// <summary>
/// Represents a model with its associated metadata.
/// </summary>
[DebuggerDisplay("{Name}")]
public class Model
{
	/// <summary>
	/// Gets or sets the name of the model.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = null!;

	/// <summary>
	/// Gets or sets the time the model was created or last modified.
	/// </summary>
	[JsonPropertyName("modified_at")]
	public DateTime ModifiedAt { get; set; }

	/// <summary>
	/// Gets or sets the size of the model file in bytes.
	/// </summary>
	[JsonPropertyName("size")]
	public long Size { get; set; }

	/// <summary>
	/// Gets or sets a cryptographic hash of the model file.
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// Gets or sets additional details about the model.
	/// </summary>
	[JsonPropertyName("details")]
	public Details Details { get; set; } = null!;
}

/// <summary>
/// Represents additional details about a model.
/// </summary>
public class Details
{
	/// <summary>
	/// Gets or sets the name of the parent model on which the model is based.
	/// </summary>
	[JsonPropertyName("parent_model")]
	public string? ParentModel { get; set; }

	/// <summary>
	/// Gets or sets the format of the model file.
	/// </summary>
	[JsonPropertyName("format")]
	public string Format { get; set; } = null!;

	/// <summary>
	/// Gets or sets the family of the model.
	/// </summary>
	[JsonPropertyName("family")]
	public string Family { get; set; } = null!;

	/// <summary>
	/// Gets or sets the families of the model.
	/// </summary>
	[JsonPropertyName("families")]
	public string[]? Families { get; set; }

	/// <summary>
	/// Gets or sets the number of parameters in the model.
	/// </summary>
	[JsonPropertyName("parameter_size")]
	public string ParameterSize { get; set; } = null!;

	/// <summary>
	/// Gets or sets the quantization level of the model.
	/// </summary>
	[JsonPropertyName("quantization_level")]
	public string QuantizationLevel { get; set; } = null!;
}
