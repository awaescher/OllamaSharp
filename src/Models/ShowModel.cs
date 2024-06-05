using System;
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

	/// <summary>
	///  The name of the model to show(Obsolete)
	/// </summary>
	[Obsolete("Name is deprecated, see Model")]
	[JsonPropertyName("name")]
	public string? Name { get; set; }
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
}