using System.Text.Json.Serialization;

namespace OllamaSharp.ModelContextProtocol.Server.Types;

/// <summary>
/// Represents a property in a JSON schema.
/// <see href="https://github.com/modelcontextprotocol/specification/blob/main/schema/2024-11-05/schema.json">See the schema for details</see>
/// </summary>
internal class JsonSchemaProperty
{
	/// <summary>
	/// The type of the property. Should be a JSON Schema type and is required.
	/// </summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// A human-readable description of the property.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; } = string.Empty;
}