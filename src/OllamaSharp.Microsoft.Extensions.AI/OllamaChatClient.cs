using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace OllamaSharp.Abstraction;

public class OllamaChatClient : IChatClient
{
	public IOllamaApiClient ApiClient { get; }

	public ChatClientMetadata Metadata => new("ollama", ApiClient.Uri, ApiClient.SelectedModel);

	public OllamaChatClient(IOllamaApiClient apiClient)
	{
		ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
	}

	public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(ApiClient, chatMessages, options, stream: false);
		var response = await ApiClient.Chat(request, cancellationToken).StreamToEnd();
		return AbstractionMapper.ToChatCompletion(request, response);
	}

	public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(ApiClient, chatMessages, options, stream: true);
		await foreach (var response in ApiClient.Chat(request, cancellationToken))
			yield return AbstractionMapper.ToStreamingChatCompletionUpdate(response);
	}

	public void Dispose()
	{
		// not required
	}

	/// <inheritdoc />
	public TService? GetService<TService>(object? key = null) where TService : class
		=> key is null ? this as TService : null;
}
