using System;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models
{
    /// <summary>
    /// https://github.com/jmorganca/ollama/blob/main/docs/api.md#pull-a-model
    /// </summary>

    [Obsolete("Name is deprecated, see Model")]
    public class PullModelRequest
	{
        [Obsolete("Name is deprecated, see Model")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }

	public class PullStatus
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("digest")]
		public string Digest { get; set; }

		[JsonPropertyName("total")]
		public long Total { get; set; }

		[JsonPropertyName("completed")]
		public long Completed { get; set; }

		[JsonIgnore]
		public double Percent => Total == 0 ? 100.0 : Completed * 100 / Total;
	}
}