using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace OllamaSharp;

/// <summary>
/// A chat helper that handles the chat logic internally and
/// automatically extends the message history.
///
/// <example>
/// A simple interactive chat can be implemented in just a handful of lines:
/// <code>
/// var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
/// var chat = new Chat(ollama);
/// // ...
/// while (true)
/// {
/// 	Console.Write("You: ");
/// 	var message = Console.ReadLine()!;
/// 	Console.Write("Ollama: ");
/// 	await foreach (var answerToken in chat.SendAsync(message))
/// 		Console.Write(answerToken);
///		// ...
/// 	Console.WriteLine();
/// }
/// // ...
/// // Output:
/// // You: Write a haiku about AI models
/// // Ollama: Code whispers secrets
/// //   Intelligent designs unfold
/// //   Minds beyond our own
/// </code>
/// </example>
/// </summary>
public class Chat
{
	/// <summary>
	/// Gets or sets the messages of the chat history
	/// </summary>
	public List<Message> Messages { get; set; } = [];

	/// <summary>
	/// Gets the Ollama API client
	/// </summary>
	public IOllamaApiClient Client { get; }

	/// <summary>
	/// Gets or sets the AI model to chat with
	/// </summary>
	public string Model { get; set; }

	/// <summary>
	/// Gets or sets the RequestOptions to chat with
	/// </summary>
	public RequestOptions? Options { get; set; }

	/// <summary>
	/// Creates a new chat instance
	/// </summary>
	/// <param name="client">The Ollama client to use for the chat</param>
	/// <param name="systemPrompt">An optional system prompt to define the behavior of the chat assistant</param>
	/// <exception cref="ArgumentNullException">
	/// If the client is null, an <see cref="ArgumentNullException"/> is thrown.
	/// </exception>
	/// <example>
	/// Setting up a chat with a system prompt:
	/// <code>
	///		var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	///		var prompt = "You are a helpful assistant that will answer any question you are asked.";
	///		var chat = new Chat(client, prompt);		
	/// </code>
	/// </example>
	public Chat(IOllamaApiClient client, string systemPrompt = "")
	{
		Client = client ?? throw new ArgumentNullException(nameof(client));
		Model = Client.SelectedModel;

		if (!string.IsNullOrEmpty(systemPrompt))
			Messages.Add(new Message(ChatRole.System, systemPrompt));
	}

	/// <summary>
	/// Sends a message to the currently selected model and streams its response
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>An <see cref="IAsyncEnumerable{String}"/> that streams the response.</returns>
	/// <example>
	/// Getting a response from the model:
	/// <code>
	/// var response = await chat.SendAsync("Write a haiku about AI models");
	/// await foreach (var answerToken in response)
	///		 Console.WriteLine(answerToken);
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsync(string message, CancellationToken cancellationToken = default)
		=> SendAsync(message, tools: null, imagesAsBase64: null, cancellationToken);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="imagesAsBytes">Images in byte representation to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>An <see cref="IAsyncEnumerable{String}"/> that streams the response.</returns>
	/// <example>
	/// Getting a response from the model with an image:
	/// <code>
	///  var client = new HttpClient();
	///  var cat = await client.GetByteArrayAsync("https://cataas.com/cat");
	///  var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	///  var chat = new Chat(ollama);
	///  var response = chat.SendAsync("What do you see?", [cat]);
	///  await foreach (var answerToken in response) Console.Write(answerToken);
	///
	///  // Output: The image shows a white kitten with black markings on its
	///  //         head and tail, sitting next to an orange tabby cat. The kitten
	///  //         is looking at the camera while the tabby cat appears to be
	///  //         sleeping or resting with its eyes closed. The two cats are
	///  //         lying in a blanket that has been rumpled up.
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsync(string message, IEnumerable<IEnumerable<byte>>? imagesAsBytes, CancellationToken cancellationToken = default)
		=> SendAsync(message, imagesAsBytes?.ToBase64() ?? [], cancellationToken);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>An <see cref="IAsyncEnumerable{String}"/> that streams the response.</returns>
	/// <example>
	/// Getting a response from the model with an image:
	/// <code>
	/// var client = new HttpClient();
	/// var cat = await client.GetByteArrayAsync("https://cataas.com/cat");
	/// var base64Cat = Convert.ToBase64String(cat);
	/// var ollama = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(ollama);
	/// var response = chat.SendAsync("What do you see?", [base64Cat]);
	/// await foreach (var answerToken in response) Console.Write(answerToken);
	/// 
	/// // Output:
	/// // The image shows a cat lying on the floor next to an iPad. The cat is looking
	/// // at the screen, which displays a game with fish and other sea creatures. The
	/// // cat's paw is touching the screen, as if it is playing the game. The background
	/// // of the image is a wooden floor.
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsync(string message, IEnumerable<string>? imagesAsBase64, CancellationToken cancellationToken = default)
		=> SendAsync(message, [], imagesAsBase64, cancellationToken);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response
	/// </summary>
	/// <param name="message">The message to send</param>
	/// <param name="tools">Tools that the model can make use of, see https://ollama.com/blog/tool-support. By using tools, response streaming is automatically turned off</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public IAsyncEnumerable<string> SendAsync(string message, IReadOnlyCollection<Tool>? tools, IEnumerable<string>? imagesAsBase64 = default, CancellationToken cancellationToken = default)
		=> SendAsAsync(ChatRole.User, message, tools, imagesAsBase64, cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, tools: null, imagesAsBase64: null, cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="imagesAsBytes">Images in byte representation to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<IEnumerable<byte>>? imagesAsBytes, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, imagesAsBytes?.ToBase64() ?? [], cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<string>? imagesAsBase64, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, [], imagesAsBase64, cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response
	/// </summary>
	/// <param name="role">The role in which the message should be sent</param>
	/// <param name="message">The message to send</param>
	/// <param name="tools">Tools that the model can make use of, see https://ollama.com/blog/tool-support. By using tools, response streaming is automatically turned off</param>
	/// <param name="imagesAsBase64">Base64 encoded images to send to the model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public async IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IReadOnlyCollection<Tool>? tools, IEnumerable<string>? imagesAsBase64 = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Messages.Add(new Message(role, message, imagesAsBase64?.ToArray()));

		var hasTools = tools?.Any() ?? false;

		var request = new ChatRequest
		{
			Messages = Messages,
			Model = Model,
			Stream = !hasTools, // cannot stream if tools should be used
			Tools = tools,
			Options = Options
		};

		var messageBuilder = new MessageBuilder();

		await foreach (var answer in Client.ChatAsync(request, cancellationToken).ConfigureAwait(false))
		{
			if (answer is not null)
			{
				messageBuilder.Append(answer);
				yield return answer.Message.Content ?? string.Empty;
			}
		}

		if (messageBuilder.HasValue)
			Messages.Add(messageBuilder.ToMessage());
	}
}
