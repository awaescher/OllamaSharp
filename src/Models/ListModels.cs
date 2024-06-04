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
	public Model[] Models { get; set; }
}

[DebuggerDisplay("{Name}")]
public class Model
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("modified_at")]
	public DateTime ModifiedAt { get; set; }

	[JsonPropertyName("size")]
	public long Size { get; set; }

	[JsonPropertyName("digest")]
	public string Digest { get; set; }

	[JsonPropertyName("details")]
	public Details Details { get; set; }
}

public class Details
{
	[JsonPropertyName("parent_model")]
	public string ParentModel { get; set; }

	[JsonPropertyName("format")]
	public string Format { get; set; }

	[JsonPropertyName("family")]
	public string Family { get; set; }

	[JsonPropertyName("families")]
	public string[]? Families { get; set; }

	[JsonPropertyName("parameter_size")]
	public string ParameterSize { get; set; }

	[JsonPropertyName("quantization_level")]
	public string QuantizationLevel { get; set; }
}