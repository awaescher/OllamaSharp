using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#list-local-models
/// </summary>
public class ListModelsResponse
{
	[JsonPropertyName("models")]
	public Model[] Models { get; set; } = null!;
}

[DebuggerDisplay("{Name}")]
public class Model
{
	/// <summary>
	/// The name of the model
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = null!;

	/// <summary>
	/// The time the model was created or modified
	/// </summary>
	[JsonPropertyName("modified_at")]
	public DateTime ModifiedAt { get; set; }

	/// <summary>
	/// The size of the model file in bytes
	/// </summary>
	[JsonPropertyName("size")]
	public long Size { get; set; }

	/// <summary>
	/// A cryptographic hash of the model file
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// Additional details about the model
	/// </summary>
	[JsonPropertyName("details")]
	public Details Details { get; set; } = null!;
}

public class Details
{
	/// <summary>
	/// The name of the parent model on which the model is based
	/// </summary>
	[JsonPropertyName("parent_model")]
	public string? ParentModel { get; set; }

	/// <summary>
	/// The format of the model file
	/// </summary>
	[JsonPropertyName("format")]
	public string Format { get; set; } = null!;

	/// <summary>
	/// The family of the model
	/// </summary>
	[JsonPropertyName("family")]
	public string Family { get; set; } = null!;

	/// <summary>
	/// Represents the model's families. 
	/// </summary>
	[JsonPropertyName("families")]
	public string[]? Families { get; set; }

	/// <summary>
	/// The number of parameters in the model
	/// </summary>
	[JsonPropertyName("parameter_size")]
	public string ParameterSize { get; set; } = null!;

	/// <summary>
	/// The quantization level of the model
	/// </summary>
	[JsonPropertyName("quantization_level")]
	public string QuantizationLevel { get; set; } = null!;
}