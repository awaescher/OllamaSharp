using System.Runtime.CompilerServices;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;

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
	/// Event that gets fired for each token that the AI model is thinking. This will just work for models that support thinking according to their Ollama
	/// manifest and if Think is set to true.
	/// If Think is set to null, think tokens will be written to the default model output.
	/// If Think is false, think tokens will not be emitted.
	/// </summary>
	public event EventHandler<string>? OnThink;

	/// <summary>
	/// Gets fired when the AI model wants to invoke a tool.
	/// </summary>
	public event EventHandler<Message.ToolCall>? OnToolCall;

	/// <summary>
	/// Gets fired after a tool was invoked and the result is available.
	/// </summary>
	public event EventHandler<ToolResult>? OnToolResult;

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
	/// Gets or sets the class instance that invokes provided tools requested by the AI model
	/// </summary>
	public IToolInvoker ToolInvoker { get; set; } = new DefaultToolInvoker();

	/// <summary>
	/// Gets or sets a value to enable or disable thinking. Use reasoning models like openthinker, qwen3,
	/// deepseek-r1, phi4-reasoning that support thinking when activating this option.
	/// This might cause errors with non-reasoning models, see https://github.com/awaescher/OllamaSharp/releases/tag/5.2.0
	/// More information: https://github.com/ollama/ollama/releases/tag/v0.9.0
	/// </summary>
	public bool? Think { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Chat"/> class.
	/// This basic constructor sets up the chat without a predefined system prompt.
	/// </summary>
	/// <param name="client">
	/// An implementation of the <see cref="IOllamaApiClient"/> interface, used for managing communication with the chat backend.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when the <paramref name="client"/> parameter is <c>null</c>.
	/// </exception>
	/// <example>
	/// Setting up a chat instance without a system prompt:
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(client);
	///
	/// // Sending a message to the chat
	/// chat.SendMessage("Hello, how are you?");
	/// </code>
	/// </example>
	public Chat(IOllamaApiClient client)
	{
		Client = client ?? throw new ArgumentNullException(nameof(client));
		Model = Client.SelectedModel;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Chat"/> class with a custom system prompt.
	/// This constructor allows you to define the assistant's initial behavior or personality using a system prompt.
	/// </summary>
	/// <param name="client">
	/// An implementation of the <see cref="IOllamaApiClient"/> interface, used for managing communication with the chat backend.
	/// </param>
	/// <param name="systemPrompt">
	/// A string representing the system prompt that defines the behavior and context for the chat assistant. For example, you can set the assistant to be helpful, humorous, or focused on a specific domain.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when the <paramref name="client"/> parameter is <c>null</c>.
	/// </exception>
	/// <example>
	/// Creating a chat instance with a custom system prompt:
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var systemPrompt = "You are an expert assistant specializing in data science.";
	/// var chat = new Chat(client, systemPrompt);
	///
	/// // Sending a message to the chat
	/// chat.SendMessage("Can you explain neural networks?");
	/// </code>
	/// </example>
	public Chat(IOllamaApiClient client, string systemPrompt) : this(client)
	{
		if (string.IsNullOrWhiteSpace(systemPrompt))
			return;

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
		=> SendAsync(message, tools: null, imagesAsBase64: null, format: null, cancellationToken: cancellationToken);

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
		=> SendAsync(message, imagesAsBase64: imagesAsBytes?.ToBase64(), cancellationToken: cancellationToken);

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
		=> SendAsync(message, tools: null, imagesAsBase64: imagesAsBase64, cancellationToken: cancellationToken);

	/// <summary>
	/// Sends a message to the currently selected model and streams its response.
	/// Allows for optional tools, images, or response formatting to customize the interaction.
	/// </summary>
	/// <param name="message">
	/// The message to send to the chat model as a string.
	/// </param>
	/// <param name="tools">
	/// A collection of <see cref="Tool"/> instances that the model can utilize.
	/// Enabling tools automatically disables response streaming. For more information, see the tools documentation: <a href="https://ollama.com/blog/tool-support">Tool Support</a>.
	/// </param>
	/// <param name="imagesAsBase64">
	/// An optional collection of images encoded as Base64 strings to pass into the model.
	/// </param>
	/// <param name="format">
	/// Specifies the response format. Can be set to <c>"json"</c> or an object created with <c>JsonSerializerOptions.Default.GetJsonSchemaAsNode</c>.
	/// </param>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
	/// </param>
	/// <returns>
	/// An asynchronous enumerable stream of string responses from the model.
	/// </returns>
	/// <example>
	/// Example usage of <see cref="SendAsync(string, IEnumerable{object}?, IEnumerable{string}?, object?, CancellationToken)"/>:
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(client);
	/// var tools = new List&lt;Tool&gt; { new Tool() }; // Example tools
	/// var images = new List&lt;string&gt; { ConvertImageToBase64("path-to-image.jpg") };
	/// await foreach (var response in chat.SendAsync(
	///   "Tell me about recent advancements in AI.",
	///   tools: tools,
	///   imagesAsBase64: images,
	///   format: "json",
	///   cancellationToken: CancellationToken.None))
	/// {
	///   Console.WriteLine(response);
	/// }
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsync(string message, IEnumerable<object>? tools,
		IEnumerable<string>? imagesAsBase64 = null, object? format = null,
		CancellationToken cancellationToken = default)
		=> SendAsAsync(ChatRole.User, message, tools: tools, imagesAsBase64: imagesAsBase64, format: format,
			cancellationToken: cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response.
	/// </summary>
	/// <param name="role">
	/// The role in which the message should be sent, represented by a <see cref="ChatRole"/>.
	/// </param>
	/// <param name="message">
	/// The message to be sent as a string.
	/// </param>
	/// <param name="cancellationToken">
	/// An optional <see cref="CancellationToken"/> to observe while waiting for the response.
	/// </param>
	/// <returns>
	/// An <see cref="IAsyncEnumerable{T}"/> of strings representing the streamed response from the server.
	/// </returns>
	/// <example>
	/// Example usage of the <see cref="SendAsAsync(ChatRole, string, CancellationToken)"/> method:
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(client);
	/// var role = new ChatRole("assistant");
	/// var responseStream = chat.SendAsAsync(role, "How can I assist you today?");
	/// await foreach (var response in responseStream)
	/// {
	///		Console.WriteLine(response); // Streams and prints the response from the server
	/// }
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message,
		CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, tools: null, imagesAsBase64: null, cancellationToken: cancellationToken);

	/// <summary>
	/// Sends a message in a given role to the currently selected model and streams its response asynchronously.
	/// </summary>
	/// <param name="role">
	/// The role in which the message should be sent. Refer to <see cref="ChatRole"/> for supported roles.
	/// </param>
	/// <param name="message">
	/// The message to send to the model.
	/// </param>
	/// <param name="imagesAsBytes">
	/// Optional images represented as byte arrays to include in the request. This parameter can be <c>null</c>.
	/// </param>
	/// <param name="cancellationToken">
	/// A cancellation token to observe while waiting for the response.
	/// By default, this parameter is set to <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// An <see cref="IAsyncEnumerable{T}"/> of strings representing the streamed response generated by the model.
	/// </returns>
	/// <example>
	/// Sending a user message with optional images:
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(client);
	/// var role = new ChatRole("user");
	/// var message = "What's the weather like today?";
	/// var images = new List&lt;IEnumerable&lt;byte>> { File.ReadAllBytes("exampleImage.jpg") };
	/// await foreach (var response in chat.SendAsAsync(role, message, images, CancellationToken.None))
	/// {
	///   Console.WriteLine(response);
	/// }
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message,
		IEnumerable<IEnumerable<byte>>? imagesAsBytes, CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, imagesAsBase64: imagesAsBytes?.ToBase64(), cancellationToken: cancellationToken);

	/// <summary>
	/// Sends a message with a specified role to the current model and streams the response as an asynchronous sequence of strings.
	/// </summary>
	/// <param name="role">
	/// The role from which the message originates, such as "User" or "Assistant".
	/// </param>
	/// <param name="message">
	/// The message to send to the model.
	/// </param>
	/// <param name="imagesAsBase64">
	/// Optional collection of images, encoded in Base64 format, to include with the message.
	/// </param>
	/// <param name="cancellationToken">
	/// A token that can be used to cancel the operation.
	/// </param>
	/// <returns>
	/// An asynchronous sequence of strings representing the streamed response from the model.
	/// </returns>
	/// <example>
	/// <code>
	/// var client = new OllamaApiClient("http://localhost:11434", "llama3.2-vision:latest");
	/// var chat = new Chat(client)
	/// {
	///   Model = "llama3.2-vision:latest"
	/// };
	/// // Sending a message as a user role and processing the response
	/// await foreach (var response in chat.SendAsAsync(ChatRole.User, "Describe the image", null))
	/// {
	///   Console.WriteLine(response);
	/// }
	/// </code>
	/// </example>
	public IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<string>? imagesAsBase64,
		CancellationToken cancellationToken = default)
		=> SendAsAsync(role, message, tools: null, imagesAsBase64: imagesAsBase64,
			cancellationToken: cancellationToken);

	/// <summary>
	/// Sends a message as a specified role to the current model and streams back its response as an asynchronous enumerable.
	/// </summary>
	/// <param name="role">
	/// The role in which the message should be sent. This determines the context or perspective of the message.
	/// </param>
	/// <param name="message">
	/// The message that needs to be sent to the chat model.
	/// </param>
	/// <param name="tools">
	/// A collection of tools available for the model to utilize. Tools can alter the behavior of the model, such as turning off response streaming automatically when used.
	/// </param>
	/// <param name="imagesAsBase64">
	/// An optional collection of images encoded in Base64 format, which are sent along with the message to the model.
	/// </param>
	/// <param name="format">
	/// Defines the response format. Acceptable values include <c>"json"</c> or a schema object created with <c>JsonSerializerOptions.Default.GetJsonSchemaAsNode</c>.
	/// </param>
	/// <param name="cancellationToken">
	/// A token to cancel the ongoing operation if required.
	/// </param>
	/// <returns>
	/// An asynchronous enumerable of response strings streamed from the model.
	/// </returns>
	/// <exception cref="NotSupportedException">
	/// Thrown if the <paramref name="format"/> argument is of type <see cref="CancellationToken"/> by mistake, or if any unsupported types are passed.
	/// </exception>
	/// <example>
	/// Using the <see cref="SendAsAsync(ChatRole, string, IEnumerable{object}, IEnumerable{string}, object, CancellationToken)"/> method to send a message and stream the model's response:
	/// <code>
	/// var chat = new Chat(client);
	/// var role = new ChatRole("assistant");
	/// var tools = new List&lt;Tool>();
	/// var images = new List&lt;string> { "base64EncodedImageData" };
	/// await foreach (var response in chat.SendAsAsync(role, "Generate a summary for the attached image", tools, images))
	/// {
	///   Console.WriteLine($"Received response: {response}");
	/// }
	/// </code>
	/// </example>
	public async IAsyncEnumerable<string> SendAsAsync(ChatRole role, string message, IEnumerable<object>? tools,
		IEnumerable<string>? imagesAsBase64 = null, object? format = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (format is CancellationToken)
			throw new NotSupportedException(
				$"Argument \"{nameof(format)}\" cannot be of type {nameof(CancellationToken)}. Make sure you use the correct method overload of {nameof(Chat)}{nameof(SendAsync)}() or {nameof(Chat)}{nameof(SendAsAsync)}().");

		Messages.Add(new Message(role, message, imagesAsBase64?.ToArray()));

		var request = new ChatRequest
		{
			Messages = Messages,
			Model = Model,
			Stream = true,
			Tools = tools,
			Format = format,
			Options = Options,
			Think = Think,
		};

		var messageBuilder = new MessageBuilder();
		await foreach (var answer in Client.ChatAsync(request, cancellationToken).ConfigureAwait(false))
		{
			if (answer is null)
				continue;

			messageBuilder.Append(answer);

			// yield the message content or call the delegate to handle thinking
			var isThinking = Think == true && !string.IsNullOrEmpty(answer.Message.Thinking);
			if (isThinking)
				OnThink?.Invoke(this, answer.Message.Thinking!);
			else
				yield return answer.Message.Content ?? string.Empty;
		}

		if (messageBuilder.HasValue)
		{
			var answerMessage = messageBuilder.ToMessage();
			Messages.Add(answerMessage);

			if (ToolInvoker is not null && role != ChatRole.Tool)
			{
				var toolResultMessages = new List<Message>();
				foreach (var toolCall in answerMessage.ToolCalls ?? [])
				{
					// call tools if available and requested by the AI model and yield the results
					OnToolCall?.Invoke(this, toolCall);
					var toolResult = await ToolInvoker.InvokeAsync(toolCall, tools ?? [], cancellationToken).ConfigureAwait(false);
					toolResultMessages.Add(new Message(ChatRole.Tool, $"Tool: {StringifyToolCall(toolCall)}:\nResult: {toolResult.Result}")); // TODO Arguments
					OnToolResult?.Invoke(this, toolResult);
				}

				if (toolResultMessages.Any())
				{
					// in case of multiple tool calls, add these to the message history except the last one.
					// the last one will be used as message to send back to the AI model which causes the chat to go on.
					Messages.AddRange(toolResultMessages.Take(toolResultMessages.Count - 1));
					await foreach (var answer in SendAsAsync(ChatRole.Tool, toolResultMessages.Last()!.Content ?? "", cancellationToken).ConfigureAwait(false))
						yield return answer;
				}
			}
		}
	}

	private static string StringifyToolCall(Message.ToolCall toolCall)
	{
		return $"{toolCall.Function?.Name ?? "(unnamed tool)"}({string.Join(", ", toolCall.Function?.Arguments?.Select(kvp => $"{kvp.Key}: {kvp.Value}") ?? [])})";
	}

}
