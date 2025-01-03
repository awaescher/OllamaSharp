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
	public IEnumerable<Tool>? Tools { get; set; }
}

/// <summary>
/// Represents a tool that the model can use, if supported.
/// </summary>
public class Tool
{
	/// <summary>
	/// Gets or sets the type of the tool, default is "function".
	/// </summary>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonPropertyName("type")]
	public string? Type { get; set; } = "function";

	/// <summary>
	/// Gets or sets the function definition associated with this tool.
	/// </summary>
	[JsonPropertyName("function")]
	public Function? Function { get; set; }
}

/// <summary>
/// Represents a function that can be executed by a tool.
/// </summary>
public class Function
{
	/// <summary>
	/// Gets or sets the name of the function.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the description of the function.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the parameters required by the function.
	/// </summary>
	[JsonPropertyName("parameters")]
	public Parameters? Parameters { get; set; }
}

/// <summary>
/// Represents the parameters required by a function, including their properties and required fields.
/// </summary>
public class Parameters
{
	/// <summary>
	/// Gets or sets the type of the parameters, default is "object".
	/// </summary>
	[JsonPropertyName("type")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? Type { get; set; } = "object";

	/// <summary>
	/// Gets or sets the properties of the parameters with their respective types and descriptions.
	/// </summary>
	[JsonPropertyName("properties")]
	public Dictionary<string, Property>? Properties { get; set; }

	/// <summary>
	/// Gets or sets a list of required fields within the parameters.
	/// </summary>
	[JsonPropertyName("required")]
	public IEnumerable<string>? Required { get; set; }
}

/// <summary>
/// Represents a property within a function's parameters, including its type, description, and possible values.
/// </summary>
public class Property
{
	/// <summary>
	/// Gets or sets the type of the property.
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// Gets or sets the description of the property.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets an enumeration of possible values for the property.
	/// </summary>
	[JsonPropertyName("enum")]
	public IEnumerable<string>? Enum { get; set; }
}
