using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat.Converter;

/// <summary>
/// Converts a <see cref="ChatRole"/> to and from JSON.
/// </summary>
public class ChatRoleConverter : JsonConverter<ChatRole>
{
	public override ChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return new ChatRole(value);
	}

	public override void Write(Utf8JsonWriter writer, ChatRole value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
