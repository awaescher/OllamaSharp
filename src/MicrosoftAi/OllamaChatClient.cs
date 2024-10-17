using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// The Ollama implementation for Microsoft.Extensions.AI
/// </summary>
public class OllamaChatClient : IChatClient
{
	/// <summary>
	/// Gets the api client used to communicate with the Ollama endpoint
	/// </summary>
	public IOllamaApiClient ApiClient { get; }

	/// <inheritdoc/>
	public ChatClientMetadata Metadata => new("ollama", ApiClient.Uri, ApiClient.SelectedModel);

	/// <summary>
	/// Creates a new instance of the Ollama implementation of IChatClient
	/// </summary>
	/// <param name="uriString">The uri of the Ollama API endpoint</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	public OllamaChatClient(string uriString, string defaultModel = "")
		: this(new Uri(uriString), defaultModel)
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama implementation of IChatClient
	/// </summary>
	/// <param name="uri">The uri of the Ollama API endpoint</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	public OllamaChatClient(Uri uri, string defaultModel = "")
		: this(new OllamaApiClient.Configuration { Uri = uri, Model = defaultModel })
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama implementation of IChatClient
	/// </summary>
	/// <param name="config">The configuration for the Ollama API client</param>
	public OllamaChatClient(OllamaApiClient.Configuration config)
		: this(new HttpClient() { BaseAddress = config.Uri }, config.Model)
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama implementation of IChatClient
	/// </summary>
	/// <param name="client">The Http client to access the Ollama API with</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	/// <exception cref="ArgumentNullException"></exception>
	public OllamaChatClient(HttpClient client, string defaultModel = "")
		: this(new OllamaApiClient(client, defaultModel))
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama implementation of IChatClient
	/// </summary>
	/// <param name="apiClient">The api client used to communicate with the Ollama endpoint</param>
	public OllamaChatClient(IOllamaApiClient apiClient)
	{
		ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
	}

	/// <inheritdoc/>
	public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(ApiClient, chatMessages, options, stream: false);
		var response = await ApiClient.Chat(request, cancellationToken).StreamToEnd();
		return AbstractionMapper.ToChatCompletion(request, response) ?? new ChatCompletion([]);
	}

	/// <inheritdoc/>
	public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(ApiClient, chatMessages, options, stream: true);
		await foreach (var response in ApiClient.Chat(request, cancellationToken))
			yield return AbstractionMapper.ToStreamingChatCompletionUpdate(response);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// not required
	}

	/// <inheritdoc />
	public TService? GetService<TService>(object? key = null) where TService : class
		=> key is null ? this as TService : null;
}
