using System;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models
{
	/// <summary>
	/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#push-a-model
	/// </summary>
	public class PushRequest
	{
        /// <summary>
        /// Name of the model to push in the form of <namespace>/<model>:<tag> (Obsolete)
        /// </summary>
        [Obsolete("Name is deprecated, see Model")]
        [JsonPropertyName("name")]
		public string? Name { get; set; }

        /// <summary>
        /// Name of the model to push in the form of <namespace>/<model>:<tag> 
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("insecure")]
		public bool Insecure { get; set; }

		[JsonPropertyName("stream")]
		public bool Stream { get; set; }
	}

	public class PushStatus
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("digest")]
		public string Digest { get; set; }

		[JsonPropertyName("total")]
		public int Total { get; set; }
	}
}