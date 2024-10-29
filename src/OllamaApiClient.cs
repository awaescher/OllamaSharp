using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;

namespace OllamaSharp;

/// <summary>
/// The default client to use the Ollama API conveniently.
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md"/>
/// </summary>
public class OllamaApiClient : IOllamaApiClient, IChatClient
{
	private readonly bool _disposeHttpClient;

	/// <summary>
	/// Gets the default request headers that are sent to the Ollama API.
	/// </summary>
	public Dictionary<string, string> DefaultRequestHeaders { get; } = [];

	/// <summary>
	/// Gets the serializer options for outgoing web requests like Post or Delete.
	/// </summary>
	public JsonSerializerOptions OutgoingJsonSerializerOptions { get; } = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	/// <summary>
	/// Gets the serializer options used for deserializing HTTP responses.
	/// </summary>
	public JsonSerializerOptions IncomingJsonSerializerOptions { get; } = new();

	/// <summary>
	/// Gets the current configuration of the API client.
	/// </summary>
	public Configuration Config { get; }

	/// <inheritdoc />
	public Uri Uri => _client.BaseAddress;

	/// <inheritdoc />
	public string SelectedModel { get; set; }

	/// <summary>
	/// Gets the HTTP client that is used to communicate with the Ollama API.
	/// </summary>
	private readonly HttpClient _client;

	/// <summary>
	/// Creates a new instance of the Ollama API client.
	/// </summary>
	/// <param name="uriString">The URI of the Ollama API endpoint.</param>
	/// <param name="defaultModel">The default model that should be used with Ollama.</param>
	public OllamaApiClient(string uriString, string defaultModel = "")
		: this(new Uri(uriString), defaultModel)
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama API client.
	/// </summary>
	/// <param name="uri">The URI of the Ollama API endpoint.</param>
	/// <param name="defaultModel">The default model that should be used with Ollama.</param>
	public OllamaApiClient(Uri uri, string defaultModel = "")
		: this(new Configuration { Uri = uri, Model = defaultModel })
	{
	}

	/// <summary>
	/// Creates a new instance of the Ollama API client.
	/// </summary>
	/// <param name="config">The configuration for the Ollama API client.</param>
	public OllamaApiClient(Configuration config)
		: this(new HttpClient() { BaseAddress = config.Uri }, config.Model)
	{
		_disposeHttpClient = true;
	}

	/// <summary>
	/// Creates a new instance of the Ollama API client.
	/// </summary>
	/// <param name="client">The HTTP client to access the Ollama API with.</param>
	/// <param name="defaultModel">The default model that should be used with Ollama.</param>
	/// <exception cref="ArgumentNullException"></exception>
	public OllamaApiClient(HttpClient client, string defaultModel = "")
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		Config = new Configuration
		{
			Uri = client.BaseAddress ?? throw new InvalidOperationException("HttpClient base address is not set!"),
			Model = defaultModel
		};
		SelectedModel = defaultModel;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<CreateModelRequest, CreateModelResponse?>("api/create", request, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/delete")
		{
			Content = new StringContent(JsonSerializer.Serialize(request, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		await SendToOllamaAsync(requestMessage, request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<IEnumerable<Model>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListModelsResponse>("api/tags", cancellationToken).ConfigureAwait(false);
		return data.Models;
	}

	/// <inheritdoc />
	public async Task<IEnumerable<RunningModel>> ListRunningModelsAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListRunningModelsResponse>("api/ps", cancellationToken).ConfigureAwait(false);
		return data.RunningModels;
	}

	/// <inheritdoc />
	public Task<ShowModelResponse> ShowModelAsync(ShowModelRequest request, CancellationToken cancellationToken = default)
		=> PostAsync<ShowModelRequest, ShowModelResponse>("api/show", request, cancellationToken);

	/// <inheritdoc />
	public Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
		=> PostAsync("api/copy", request, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<PullModelResponse?> PullModelAsync(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<PullModelRequest, PullModelResponse?>("api/pull", request, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<PushModelResponse?> PushModelAsync(PushModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<PushModelRequest, PushModelResponse?>("api/push", request, cancellationToken).ConfigureAwait(false);

		await foreach (var result in stream.ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public Task<EmbedResponse> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(request.Model))
			request.Model = SelectedModel;

		return PostAsync<EmbedRequest, EmbedResponse>("api/embed", request, cancellationToken);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<GenerateResponseStream?> GenerateAsync(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(request.Model))
			request.Model = SelectedModel;

		await foreach (var result in GenerateCompletionAsync(request, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseStream?> ChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(request.Model))
			request.Model = SelectedModel;

		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/chat")
		{
			Content = new StringContent(JsonSerializer.Serialize(request, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		var completion = request.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await SendToOllamaAsync(requestMessage, request, completion, cancellationToken).ConfigureAwait(false);

		await foreach (var result in ProcessStreamedChatResponseAsync(response, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Get, ""); // without route returns "Ollama is running"

		using var response = await SendToOllamaAsync(requestMessage, null, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		return !string.IsNullOrWhiteSpace(stringContent);
	}

	/// <inheritdoc />
	public async Task<Version> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<JsonNode>("api/version", cancellationToken).ConfigureAwait(false);
		return Version.Parse(data["version"]?.ToString());
	}

	private async IAsyncEnumerable<GenerateResponseStream?> GenerateCompletionAsync(GenerateRequest generateRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		var completion = generateRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await SendToOllamaAsync(requestMessage, generateRequest, completion, cancellationToken).ConfigureAwait(false);

		await foreach (var result in ProcessStreamedCompletionResponseAsync(response, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	private async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);

		using var response = await SendToOllamaAsync(requestMessage, null, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		return JsonSerializer.Deserialize<TResponse>(responseBody, IncomingJsonSerializerOptions)!;
	}

	private async Task PostAsync<TRequest>(string endpoint, TRequest ollamaRequest, CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(ollamaRequest, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
	}

	private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest ollamaRequest, CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(ollamaRequest, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		using var response = await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		return JsonSerializer.Deserialize<TResponse>(responseBody, IncomingJsonSerializerOptions)!;
	}

	private async IAsyncEnumerable<TResponse?> StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest ollamaRequest, [EnumeratorCancellation] CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(ollamaRequest, OutgoingJsonSerializerOptions), Encoding.UTF8, "application/json")
		};

		using var response = await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

		await foreach (var result in ProcessStreamedResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	private async IAsyncEnumerable<TLine?> ProcessStreamedResponseAsync<TLine>(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			yield return JsonSerializer.Deserialize<TLine?>(line, IncomingJsonSerializerOptions);
		}
	}

	private async IAsyncEnumerable<GenerateResponseStream?> ProcessStreamedCompletionResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			var streamedResponse = JsonSerializer.Deserialize<GenerateResponseStream>(line, IncomingJsonSerializerOptions);

			yield return streamedResponse?.Done ?? false
				? JsonSerializer.Deserialize<GenerateDoneResponseStream>(line, IncomingJsonSerializerOptions)!
				: streamedResponse;
		}
	}

	private async IAsyncEnumerable<ChatResponseStream?> ProcessStreamedChatResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			var streamedResponse = JsonSerializer.Deserialize<ChatResponseStream>(line, IncomingJsonSerializerOptions);

			yield return streamedResponse?.Done ?? false
				? JsonSerializer.Deserialize<ChatDoneResponseStream>(line, IncomingJsonSerializerOptions)!
				: streamedResponse;
		}
	}

	/// <summary>
	/// Sends an HTTP request message to the Ollama API.
	/// </summary>
	/// <param name="requestMessage">The HTTP request message to send.</param>
	/// <param name="ollamaRequest">The request containing custom HTTP request headers.</param>
	/// <param name="completionOption">When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	protected virtual async Task<HttpResponseMessage> SendToOllamaAsync(HttpRequestMessage requestMessage, OllamaRequest? ollamaRequest, HttpCompletionOption completionOption, CancellationToken cancellationToken)
	{
		requestMessage.ApplyCustomHeaders(DefaultRequestHeaders, ollamaRequest);

		var response = await _client.SendAsync(requestMessage, completionOption, cancellationToken).ConfigureAwait(false);

		await EnsureSuccessStatusCodeAsync(response).ConfigureAwait(false);

		return response;
	}

	private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
	{
		if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
		{
			var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false) ?? string.Empty;

			var errorElement = new JsonElement();

			var couldParse = false;

			try
			{
				couldParse = JsonDocument.Parse(body)?.RootElement.TryGetProperty("error", out errorElement) ?? false;
			}
			catch (JsonException)
			{
				// parsing failed, this is optional
			}

			var errorString = (couldParse ? errorElement.GetString() : body) ?? string.Empty;

			if (errorString.Contains("does not support tools"))
				throw new ModelDoesNotSupportToolsException(errorString);

			throw new OllamaException(errorString);
		}

		response.EnsureSuccessStatusCode();
	}
	/// <summary>
	/// Releases the resources used by the <see cref="OllamaApiClient"/> instance.
	/// Disposes the internal HTTP client if it was created internally.
	/// </summary>
	public void Dispose()
	{
		if (_disposeHttpClient)
			_client?.Dispose();
	}

	#region IChatClient implementation

	/// <inheritdoc/>
	ChatClientMetadata IChatClient.Metadata => new("ollama", Uri, SelectedModel);

	/// <inheritdoc/>
	async Task<ChatCompletion> IChatClient.CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options, CancellationToken cancellationToken)
	{
		var request = MicrosoftAi.AbstractionMapper.ToOllamaSharpChatRequest(this, chatMessages, options, stream: false);
		var response = await ChatAsync(request, cancellationToken).StreamToEndAsync().ConfigureAwait(false);
		return MicrosoftAi.AbstractionMapper.ToChatCompletion(response, request.Model) ?? new ChatCompletion([]);
	}

	/// <inheritdoc/>
	async IAsyncEnumerable<StreamingChatCompletionUpdate> IChatClient.CompleteStreamingAsync(IList<ChatMessage> chatMessages, ChatOptions? options, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = MicrosoftAi.AbstractionMapper.ToOllamaSharpChatRequest(this, chatMessages, options, stream: true);
		await foreach (var response in ChatAsync(request, cancellationToken).ConfigureAwait(false))
			yield return MicrosoftAi.AbstractionMapper.ToStreamingChatCompletionUpdate(response);
	}

	/// <inheritdoc/>
	TService? IChatClient.GetService<TService>(object? key) where TService : class
		=> key is null ? this as TService : null;

	/// <inheritdoc/>
	void IDisposable.Dispose()
	{
		Dispose();
	}

	#endregion

	/// <summary>
	/// The configuration for the Ollama API client.
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// Gets or sets the URI of the Ollama API endpoint.
		/// </summary>
		public Uri Uri { get; set; } = null!;

		/// <summary>
		/// Gets or sets the model that should be used.
		/// </summary>
		public string Model { get; set; } = null!;
	}
}

/// <summary>
/// Represents a conversation context containing context data.
/// </summary>
public record ConversationContext(long[] Context);

/// <summary>
/// Represents a conversation context with an additional response.
/// Inherits from <see cref="ConversationContext"/>.
/// </summary>
public record ConversationContextWithResponse(string Response, long[] Context) : ConversationContext(Context);
