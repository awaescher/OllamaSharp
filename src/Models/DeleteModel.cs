using System;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models
{
    /// <summary>
    /// https://github.com/jmorganca/ollama/blob/main/docs/api.md#delete-a-model
    /// </summary>

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
    public class DeleteModelRequest
	{
        [Obsolete("Name is deprecated, see Model")]
        [JsonPropertyName("name")]
		public string? Name { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }
}