using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-chat-completion
/// </summary>
public class ChatRequest
{
	/// <summary>
	/// The model name (required)
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = null!;

	/// <summary>
	/// The messages of the chat, this can be used to keep a chat memory
	/// </summary>
	[JsonPropertyName("messages")]
	public IEnumerable<Message>? Messages { get; set; }

	/// <summary>
	/// Additional model parameters listed in the documentation for the Modelfile such as temperature
	/// </summary>
	[JsonPropertyName("options")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// The full prompt or prompt template (overrides what is defined in the Modelfile)
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
	/// The format to return a response in. Currently only accepts "json" or null.
	/// </summary>
	[JsonPropertyName("format")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Format { get; set; }

	/// <summary>
	/// If false the response will be returned as a single response object rather than a stream of objects
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; } = true;

	/// <summary>
	/// Tools for the model to use if supported. Requires stream to be set to false.
	/// </summary>
	[JsonPropertyName("tools")]
	public IEnumerable<Tool>? Tools { get; set; }
}

public class Tool
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[JsonPropertyName("type")]
	public string? Type { get; set; } = "function";

	[JsonPropertyName("function")]
	public Function? Function { get; set; }
}

public class Function
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("parameters")]
	public Parameters? Parameters { get; set; }
}

public class Parameters
{
	[JsonPropertyName("type")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? Type { get; set; } = "object";

	[JsonPropertyName("properties")]
	public Dictionary<string, Properties>? Properties { get; set; }

	[JsonPropertyName("required")]
	public IEnumerable<string>? Required { get; set; }
}

public class Properties
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("enum")]
	public IEnumerable<string>? Enum { get; set; }
}
