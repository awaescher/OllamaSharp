using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents a request to generate a chat completion using the specified model and parameters.
/// </summary>
public class ChatRequest : OllamaRequest
{
	/// <summary>
	/// Gets or sets the model name (required).
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// Gets or sets the messages of the chat, this can be used to keep a chat memory.
	/// </summary>
	[JsonPropertyName("messages")]
	public IEnumerable<Message>? Messages { get; set; }

	/// <summary>
	/// Gets or sets additional model parameters listed in the documentation for the Modelfile such as temperature.
	/// </summary>
	[JsonPropertyName("options")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Gets or sets the full prompt or prompt template (overrides what is defined in the Modelfile).
	/// </summary>
	[JsonPropertyName("template")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Template { get; set; }

	/// <summary>
	/// Gets or sets the KeepAlive property, which decides how long a given model should stay loaded.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? KeepAlive { get; set; }

	/// <summary>
	/// Gets or sets the format to return a response in. Currently accepts "json" or JsonSchema or null.
	/// </summary>
	[JsonPropertyName("format")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? Format { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the response will be returned as a single response object rather than a stream of objects.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; } = true;

	/// <summary>
	/// Gets or sets the tools for the model to use if supported. Requires stream to be set to false.
	/// </summary>
	[JsonPropertyName("tools")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<object>? Tools { get; set; }
}