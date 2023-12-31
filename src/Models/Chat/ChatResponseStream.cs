using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat
{
	public class ChatResponseStream
	{
		[JsonPropertyName("model")]
		public string Model { get; set; }

		[JsonPropertyName("created_at")]
		public string CreatedAt { get; set; }

		[JsonPropertyName("message")]
		public Message Message { get; set; }

		[JsonPropertyName("done")]
		public bool Done { get; set; }
	}
}