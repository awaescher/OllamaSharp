using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat.Converter;

/// <summary>
/// Converts a <see cref="ChatRole"/> to and from JSON.
/// </summary>
public class ChatRoleConverter : JsonConverter<ChatRole>
{
	/// <summary>
	/// Reads and converts the JSON representation of a <see cref="ChatRole"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	/// <param name="typeToConvert">The type of the object to convert.</param>
	/// <param name="options">Options to control the conversion.</param>
	/// <returns>The <see cref="ChatRole"/> value.</returns>
	public override ChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return new ChatRole(value);
	}

	/// <summary>
	/// Writes a <see cref="ChatRole"/> as a JSON string.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The <see cref="ChatRole"/> value to write.</param>
	/// <param name="options">Options to control the conversion.</param>
	public override void Write(Utf8JsonWriter writer, ChatRole value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
