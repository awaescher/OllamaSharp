using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharp
{
	public class Chat
	{
		private List<Message> _messages = new();

		public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

		public IOllamaApiClient Client { get; }

		public string Model { get; set; }

		public IResponseStreamer<ChatResponseStream> Streamer { get; }

		public Chat(IOllamaApiClient client, Action<ChatResponseStream> streamer)
			: this(client, new ActionResponseStreamer<ChatResponseStream>(streamer))
		{
		}

		public Chat(IOllamaApiClient client, IResponseStreamer<ChatResponseStream> streamer)
		{
			Client = client ?? throw new ArgumentNullException(nameof(client));
			Streamer = streamer ?? throw new ArgumentNullException(nameof(streamer));
		}

		public Task<IEnumerable<Message>> Send(string message, IEnumerable<string> imagesAsBase64 = null) => SendAs("user", message, imagesAsBase64);

		public async Task<IEnumerable<Message>> SendAs(ChatRole role, string message, IEnumerable<string> imagesAsBase64 = null)
		{
			_messages.Add(new Message { Role = role, Content = message, Images = imagesAsBase64?.ToArray() });

			var request = new ChatRequest
			{
				Messages = Messages,
				Model = Client.SelectedModel,
				Stream = true
			};

			var answer = await Client.SendChat(request, Streamer);
			_messages = answer.ToList();
			return Messages;
		}
	}
}