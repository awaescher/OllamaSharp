using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat
{
	/// <summary>
	/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-a-chat-completion
	/// </summary>
	public class ChatRequest
	{
		/// <summary>
		/// The model name (required)
		/// </summary>
		[JsonPropertyName("model")]
		public string Model { get; set; }

		/// <summary>
		/// The messages of the chat, this can be used to keep a chat memory
		/// </summary>
		[JsonPropertyName("messages")]
		public IEnumerable<Message> Messages { get; set; }

		/// <summary>
		/// Additional model parameters listed in the documentation for the Modelfile such as temperature
		/// </summary>
		[JsonPropertyName("options")]
		public string Options { get; set; }

		/// <summary>
		/// The full prompt or prompt template (overrides what is defined in the Modelfile)
		/// </summary>
		[JsonPropertyName("template")]
		public string Template { get; set; }

		/// <summary>
		/// If false the response will be returned as a single response object, rather than a stream of objects
		/// </summary>
		[JsonPropertyName("stream")]
		public bool Stream { get; set; } = true;
	}
}