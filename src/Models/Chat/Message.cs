using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models.Chat
{
    [DebuggerDisplay("{Role}: {Content}")]
    public class Message
    {
        /// <summary>
        /// The role of the message, either system, user or assistant
        /// </summary>
        [JsonPropertyName("role")]
        public ChatRole? Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// Base64-encoded images (for multimodal models such as llava)
        /// </summary>
        [JsonPropertyName("images")]
        public string[] Images { get; set; }
    }
}