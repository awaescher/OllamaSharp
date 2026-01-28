using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using OllamaSharp.Constants;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;

namespace OllamaSharp;

/// <summary>
/// The default client to use the Ollama API conveniently.
/// <see href="https://github.com/jmorganca/ollama/blob/main/docs/api.md"/>
/// </summary>
public class OllamaApiClient : IOllamaApiClient, IChatClient, IEmbeddingGenerator<string, Embedding<float>>
{
	/// <summary>
	/// Gets the default request headers that are sent to the Ollama API.
	/// </summary>
	public Dictionary<string, string> DefaultRequestHeaders { get; } = [];

	/// <summary>
	/// Gets the serializer options for outgoing web requests like Post or Delete.
	/// </summary>
	public JsonSerializerOptions OutgoingJsonSerializerOptions { get; }

	/// <summary>
	/// Gets the serializer options used for deserializing HTTP responses.
	/// </summary>
	public JsonSerializerOptions IncomingJsonSerializerOptions { get; }

	/// <summary>
	/// Gets the current configuration of the API client.
	/// </summary>
	public Configuration Config { get; }

	/// <inheritdoc />
	public Uri Uri => _client.BaseAddress!;

	/// <inheritdoc />
	public string SelectedModel { get; set; }

	/// <summary>
	/// Gets the <see cref="HttpClient"/> used to communicate with the Ollama API.
	/// </summary>
	private readonly HttpClient _client;
	/// <summary>
	/// If true, the <see cref="HttpClient"/> will be disposed when the <see cref="OllamaApiClient"/> is disposed.
	/// </summary>
	private readonly bool _disposeHttpClient;

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
		: this(new HttpClient() { BaseAddress = config.Uri }, config.Model, config.JsonSerializerContext)
	{
		_disposeHttpClient = true;
	}

	/// <summary>
	/// Creates a new instance of the Ollama API client.
	/// </summary>
	/// <param name="client">The HTTP client to access the Ollama API with.</param>
	/// <param name="defaultModel">The default model that should be used with Ollama.</param>
	/// <param name="jsonSerializerContext">The JSON serializer context for source generation (optional, for NativeAOT scenarios).</param>
	/// <exception cref="ArgumentNullException"></exception>
	public OllamaApiClient(HttpClient client, string defaultModel = "", JsonSerializerContext? jsonSerializerContext = null)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		Config = new Configuration
		{
			Uri = client.BaseAddress ?? throw new InvalidOperationException("HttpClient base address is not set!"),
			Model = defaultModel,
			JsonSerializerContext = jsonSerializerContext
		};
		SelectedModel = defaultModel;

		// Configure JSON serialization options
		if (jsonSerializerContext is null)
		{
			// Use standard serialization without source generation for better compatibility
			OutgoingJsonSerializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
			IncomingJsonSerializerOptions = new JsonSerializerOptions();
		}
		else
		{
			// Use source generation for NativeAOT scenarios
			OutgoingJsonSerializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, TypeInfoResolver = jsonSerializerContext };
			IncomingJsonSerializerOptions = new JsonSerializerOptions { TypeInfoResolver = jsonSerializerContext };
		}
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<CreateModelRequest, CreateModelResponse?>(Endpoints.CreateModel, request, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
	{
		using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, Endpoints.DeleteModel);
		requestMessage.Content = new StringContent(JsonSerializer.Serialize(request, OutgoingJsonSerializerOptions), Encoding.UTF8, MimeTypes.Json);

		await SendToOllamaAsync(requestMessage, request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<IEnumerable<Model>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListModelsResponse>(Endpoints.ListLocalModels, cancellationToken).ConfigureAwait(false);
		return data.Models;
	}

	/// <inheritdoc />
	public async Task<IEnumerable<RunningModel>> ListRunningModelsAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListRunningModelsResponse>(Endpoints.ListRunningModels, cancellationToken).ConfigureAwait(false);
		return data.RunningModels;
	}

	/// <inheritdoc />
	public Task<ShowModelResponse> ShowModelAsync(ShowModelRequest request, CancellationToken cancellationToken = default)
		=> PostAsync<ShowModelRequest, ShowModelResponse>(Endpoints.ShowModel, request, cancellationToken);

	/// <inheritdoc />
	public Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
		=> PostAsync(Endpoints.CopyModel, request, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<PullModelResponse?> PullModelAsync(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<PullModelRequest, PullModelResponse?>(Endpoints.PullModel, request, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<PushModelResponse?> PushModelAsync(PushModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<PushModelRequest, PushModelResponse?>(Endpoints.PushModel, request, cancellationToken).ConfigureAwait(false);

		await foreach (var result in stream.ConfigureAwait(false))
			yield return result;
	}

	/// <inheritdoc />
	public Task<EmbedResponse> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(request.Model))
			request.Model = SelectedModel;

		return PostAsync<EmbedRequest, EmbedResponse>(Endpoints.Embed, request, cancellationToken);
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

		using var requestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.Chat);
		requestMessage.Content = new StringContent(JsonSerializer.Serialize(request, OutgoingJsonSerializerOptions), Encoding.UTF8, MimeTypes.Json);

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
		using var requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Empty); // without route returns "Ollama is running"

		using var response = await SendToOllamaAsync(requestMessage, null, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		return !string.IsNullOrWhiteSpace(stringContent);
	}

	/// <inheritdoc />
	public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<JsonNode>(Endpoints.Version, cancellationToken).ConfigureAwait(false);
		return data["version"]?.ToString() ?? string.Empty;
	}

	/// <inheritdoc />
	public async Task PushBlobAsync(string digest, byte[] bytes, CancellationToken cancellationToken = default)
	{
		using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/blobs/" + digest);
		requestMessage.Content = new ByteArrayContent(bytes);
		using var response = await SendToOllamaAsync(requestMessage, null, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();
	}

	/// <inheritdoc />
	public async Task<bool> IsBlobExistsAsync(string digest, CancellationToken cancellationToken = default)
	{
		using var requestMessage = new HttpRequestMessage(HttpMethod.Head, "api/blobs/" + digest);
		requestMessage.ApplyCustomHeaders(DefaultRequestHeaders, null);
		var response = await _client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
		return response.StatusCode == HttpStatusCode.OK;
	}

	private async IAsyncEnumerable<GenerateResponseStream?> GenerateCompletionAsync(GenerateRequest generateRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var requestMessage = CreateRequestMessage(HttpMethod.Post, Endpoints.Generate, generateRequest);

		var completion = generateRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await SendToOllamaAsync(requestMessage, generateRequest, completion, cancellationToken).ConfigureAwait(false);

		await foreach (var result in ProcessStreamedCompletionResponseAsync(response, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	private async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken)
	{
		using var requestMessage = CreateRequestMessage(HttpMethod.Get, endpoint);

		using var response = await SendToOllamaAsync(requestMessage, null, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return (await JsonSerializer.DeserializeAsync<TResponse>(responseStream, IncomingJsonSerializerOptions, cancellationToken))!;
	}

	private async Task PostAsync<TRequest>(string endpoint, TRequest ollamaRequest, CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		using var requestMessage = CreateRequestMessage(HttpMethod.Post, endpoint, ollamaRequest);

		await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
	}

	private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest ollamaRequest, CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		using var requestMessage = CreateRequestMessage(HttpMethod.Post, endpoint, ollamaRequest);

		using var response = await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

		using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return (await JsonSerializer.DeserializeAsync<TResponse>(responseStream, IncomingJsonSerializerOptions, cancellationToken))!;
	}

	private async IAsyncEnumerable<TResponse?> StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest ollamaRequest, [EnumeratorCancellation] CancellationToken cancellationToken) where TRequest : OllamaRequest
	{
		using var requestMessage = CreateRequestMessage(HttpMethod.Post, endpoint, ollamaRequest);

		using var response = await SendToOllamaAsync(requestMessage, ollamaRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

		await foreach (var result in ProcessStreamedResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false))
			yield return result;
	}

	private HttpRequestMessage CreateRequestMessage(HttpMethod method, string endpoint) => new(method, endpoint);

	private HttpRequestMessage CreateRequestMessage<TRequest>(HttpMethod method, string endpoint, TRequest ollamaRequest) where TRequest : OllamaRequest
	{
		var requestMessage = new HttpRequestMessage(method, endpoint)
		{
			Content = GetJsonContent(ollamaRequest)
		};
		return requestMessage;
	}

	private StringContent GetJsonContent<TRequest>(TRequest ollamaRequest) where TRequest : OllamaRequest =>
		new(JsonSerializer.Serialize(ollamaRequest, OutgoingJsonSerializerOptions), Encoding.UTF8, MimeTypes.Json);

	private async IAsyncEnumerable<TLine?> ProcessStreamedResponseAsync<TLine>(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (!cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line == null)
				break;

			var error = JsonSerializer.Deserialize<ErrorResponse?>(line, IncomingJsonSerializerOptions);
			if (!string.IsNullOrEmpty(error?.Message))
				throw new ResponseError(error!.Message);

			yield return JsonSerializer
				.Deserialize<TLine?>(
					line,
					IncomingJsonSerializerOptions
				);
		}
	}

	private async IAsyncEnumerable<GenerateResponseStream?> ProcessStreamedCompletionResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (!cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line == null)
				break;
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

		while (!cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line == null)
				break;
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
		if (response.StatusCode == HttpStatusCode.BadRequest)
		{
			var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false) ?? string.Empty;

			var errorElement = new JsonElement();

			var couldParse = false;

			try
			{
				couldParse = JsonDocument.Parse(body).RootElement.TryGetProperty("error", out errorElement);
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

	#region IChatClient and IEmbeddingGenerator implementation

	private ChatClientMetadata? _chatClientMetadata;
	private EmbeddingGeneratorMetadata? _embeddingGeneratorMetadata;

	/// <inheritdoc/>
	async Task<ChatResponse> IChatClient.GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(this, messages, options, stream: false, OutgoingJsonSerializerOptions);
		var response = await ChatAsync(request, cancellationToken).StreamToEndAsync().ConfigureAwait(false);
		return AbstractionMapper.ToChatResponse(response, response?.Model ?? request.Model ?? SelectedModel) ?? new ChatResponse([]);
	}

	/// <inheritdoc/>
	async IAsyncEnumerable<ChatResponseUpdate> IChatClient.GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = AbstractionMapper.ToOllamaSharpChatRequest(this, messages, options, stream: true, OutgoingJsonSerializerOptions);

		string responseId = Guid.NewGuid().ToString("N");
		await foreach (var response in ChatAsync(request, cancellationToken).ConfigureAwait(false))
			yield return AbstractionMapper.ToChatResponseUpdate(response, responseId);
	}

	/// <inheritdoc/>
	async Task<GeneratedEmbeddings<Embedding<float>>> IEmbeddingGenerator<string, Embedding<float>>.GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options, CancellationToken cancellationToken)
	{
		var request = AbstractionMapper.ToOllamaEmbedRequest(values, options);
		var result = await EmbedAsync(request, cancellationToken).ConfigureAwait(false);
		return AbstractionMapper.ToGeneratedEmbeddings(request, result, request.Model ?? SelectedModel);
	}

	/// <inheritdoc/>
	object? IChatClient.GetService(Type serviceKey, object? key) =>
		key is not null ? null :
		serviceKey == typeof(ChatClientMetadata) ? (_chatClientMetadata = new(Application.Ollama, Uri, SelectedModel)) :
		serviceKey?.IsInstanceOfType(this) is true ? this :
		null;

	/// <inheritdoc />
	object? IEmbeddingGenerator.GetService(Type serviceKey, object? key) =>
		key is not null ? null :
		serviceKey == typeof(EmbeddingGeneratorMetadata) ? (_embeddingGeneratorMetadata = new(Application.Ollama, Uri, SelectedModel)) :
		serviceKey?.IsInstanceOfType(this) is true ? this :
		null;

	/// <summary>
	/// Releases the resources used by the <see cref="OllamaApiClient"/> instance.
	/// Disposes the internal HTTP client if it was created internally.
	/// </summary>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		if (_disposeHttpClient)
			_client.Dispose();
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

		/// <summary>
		/// Gets or sets the JSON serializer context for source generation (optional, for NativeAOT scenarios).
		/// When null, standard System.Text.Json serialization is used without source generation for better compatibility.
		/// </summary>
		public JsonSerializerContext? JsonSerializerContext { get; set; } = null;
	}
}

/// <summary>
/// Represents a conversation context containing context data.
/// </summary>
public record ConversationContext(long[] Context);
