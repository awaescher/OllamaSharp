using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using OllamaSharp.Streamer;

namespace OllamaSharp;

public class Chat
{
	private List<Message> _messages = new();

	public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

	public IOllamaApiClient Client { get; }

	public string Model { get; set; }

	public IResponseStreamer<ChatResponseStream?> Streamer { get; }

	public Chat(IOllamaApiClient client, Action<ChatResponseStream?> streamer)
		: this(client, new ActionResponseStreamer<ChatResponseStream?>(streamer))
	{
	}

	public Chat(IOllamaApiClient client, IResponseStreamer<ChatResponseStream?> streamer)
	{
		Client = client ?? throw new ArgumentNullException(nameof(client));
		Streamer = streamer ?? throw new ArgumentNullException(nameof(streamer));
		Model = Client.SelectedModel;
	}

	/// <summary>
	/// Sends a message to the currently selected model
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public Task<IEnumerable<Message>> Send(string message, CancellationToken cancellationToken = default)
	 => Send(message, tools: null, imagesAsBase64: null, cancellationToken);

	/// <summary>
	/// Sends a message to the currently selected model
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="tools">Tools that the model can make use of, see https://ollama.com/blog/tool-support. By using tools, response streaming is automatically turned off</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public Task<IEnumerable<Message>> Send(string message, IEnumerable<Tool>? tools, IEnumerable<string>? imagesAsBase64 = default, CancellationToken cancellationToken = default)
	 => SendAs(ChatRole.User, message, tools, imagesAsBase64, cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public Task<IEnumerable<Message>> SendAs(ChatRole role, string message, CancellationToken cancellationToken = default)
	 => SendAs(role, message, tools: null, imagesAsBase64: null, cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="tools">Tools that the model can make use of, see https://ollama.com/blog/tool-support. By using tools, response streaming is automatically turned off</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public async Task<IEnumerable<Message>> SendAs(ChatRole role, string message, IEnumerable<Tool>? tools, IEnumerable<string>? imagesAsBase64 = default, CancellationToken cancellationToken = default)
	{
		_messages.Add(new Message(role, message, imagesAsBase64?.ToArray()));

		var hasTools = tools?.Any() ?? false;

		var request = new ChatRequest
		{
			Messages = Messages,
			Model = Model,
			Stream = !hasTools, // cannot stream if tools should be used
			Tools = tools
		};

		var answer = await Client.SendChat(request, Streamer, cancellationToken);

		_messages = answer.ToList();

		return Messages;
	}
}
