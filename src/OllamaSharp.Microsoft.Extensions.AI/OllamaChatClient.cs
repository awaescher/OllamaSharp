using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp.Abstraction;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OllamaSharp;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// The Ollama implementation for Microsoft.Extensions.AI
/// </summary>
/// <param name="apiClient">The api client used to communicate with the Ollama endpoint</param>
public class OllamaChatClient(IOllamaApiClient apiClient) : IChatClient
{
	/// <summary>
	/// Gets the api client used to communicate with the Ollama endpoint
	/// </summary>
	public IOllamaApiClient ApiClient { get; } = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

	/// <inheritdoc/>
	public ChatClientMetadata Metadata => new("ollama", ApiClient.Uri, ApiClient.SelectedModel);

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
