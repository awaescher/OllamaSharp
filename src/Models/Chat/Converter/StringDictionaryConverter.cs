using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat.Converter;

/// <summary>
/// Converts a <see cref="Dictionary{TKey,TValue}"/> with <see cref="string"/> keys and values to and from JSON.
/// </summary>
public class StringDictionaryConverter : JsonConverter<Dictionary<string, string>>
{
	/// <inheritdoc />
	public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert,
		JsonSerializerOptions options)
	{
		var dictionary = new Dictionary<string, string>();

		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException("Expected StartObject token");
		}

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return dictionary;
			}

			// Read the property name
			if (reader.TokenType != JsonTokenType.PropertyName)
			{
				throw new JsonException("Expected PropertyName token");
			}

			string propertyName = reader.GetString()!;

			// Read the value
			if (!reader.Read())
			{
				throw new JsonException("Unexpected end of JSON");
			}

			string value = reader.TokenType switch
			{
				JsonTokenType.Number => reader.GetDouble().ToString(),
				JsonTokenType.True => "true",
				JsonTokenType.False => "false",
				JsonTokenType.Null => string.Empty,
				_ => reader.GetString() ?? string.Empty // Fallback for other types
			};

			dictionary[propertyName] = value;
		}

		throw new JsonException("Expected EndObject token");
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value,
		JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var kvp in value)
		{
			writer.WritePropertyName(kvp.Key);
			writer.WriteStringValue(kvp.Value);
		}

		writer.WriteEndObject();
	}
}
