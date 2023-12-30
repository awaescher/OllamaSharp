using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat.Converter
{
    public class ChatRoleConverter : JsonConverter<ChatRole>
    {
        public override ChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            return new ChatRole(value);
        }

        public override void Write(Utf8JsonWriter writer, ChatRole value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}