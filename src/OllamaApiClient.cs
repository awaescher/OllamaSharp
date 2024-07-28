using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Streamer;

namespace OllamaSharp;

/// <summary>
/// The default client to use the Ollama API conveniently
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md
/// </summary>
public class OllamaApiClient : IOllamaApiClient
{
	private readonly HttpClient _client;

	/// <summary>
	/// Gets the current configuration of the API client
	/// </summary>
	public Configuration Config { get; }

	/// <inheritdoc />
	public string SelectedModel { get; set; }

	/// <summary>
	/// Creates a new instace of the Ollama API client
	/// </summary>
	/// <param name="uriString">The uri of the Ollama API endpoint</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	public OllamaApiClient(string uriString, string defaultModel = "")
		: this(new Uri(uriString), defaultModel)
	{
	}

	/// <summary>
	/// Creates a new instace of the Ollama API client
	/// </summary>
	/// <param name="uri">The uri of the Ollama API endpoint</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	public OllamaApiClient(Uri uri, string defaultModel = "")
		: this(new Configuration { Uri = uri, Model = defaultModel })
	{
	}

	/// <summary>
	/// Creates a new instace of the Ollama API client
	/// </summary>
	/// <param name="config">The configuration for the Ollama API client</param>
	public OllamaApiClient(Configuration config)
		: this(new HttpClient() { BaseAddress = config.Uri }, config.Model)
	{
	}

	/// <summary>
	/// Creates a new instace of the Ollama API client
	/// </summary>
	/// <param name="client">The Http client to access the Ollama API with</param>
	/// <param name="defaultModel">The default model that should be used with Ollama</param>
	/// <exception cref="ArgumentNullException"></exception>
	public OllamaApiClient(HttpClient client, string defaultModel = "")
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		Config = new Configuration
		{
			Uri = client.BaseAddress ?? throw new ArgumentNullException(nameof(client.BaseAddress)),
			Model = defaultModel
		};
		SelectedModel = defaultModel;
	}

	/// <inheritdoc />
	public Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateModelResponse> streamer, CancellationToken cancellationToken = default)
	 => StreamPostAsync("api/create", request, streamer, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<CreateModelResponse?> CreateModel(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<CreateModelRequest, CreateModelResponse?>("api/create", request, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public async Task DeleteModel(string model, CancellationToken cancellationToken = default)
	{
		var request = new HttpRequestMessage(HttpMethod.Delete, "api/delete")
		{
			Content = new StringContent(JsonSerializer.Serialize(new DeleteModelRequest { Model = model }), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	/// <inheritdoc />
	public async Task<IEnumerable<Model>> ListLocalModels(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListModelsResponse>("api/tags", cancellationToken);
		return data.Models;
	}

	/// <inheritdoc />
	public async Task<IEnumerable<RunningModel>> ListRunningModels(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<ListRunningModelsResponse>("api/ps", cancellationToken);
		return data.RunningModels;
	}

	/// <inheritdoc />
	public Task<ShowModelResponse> ShowModelInformation(string model, CancellationToken cancellationToken = default)
	 => PostAsync<ShowModelRequest, ShowModelResponse>("api/show", new ShowModelRequest { Model = model }, cancellationToken);

	/// <inheritdoc />
	public Task CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default)
	 => PostAsync("api/copy", request, cancellationToken);

	/// <inheritdoc />
	public Task PullModel(PullModelRequest request, IResponseStreamer<PullModelResponse> streamer, CancellationToken cancellationToken = default)
	 => StreamPostAsync("api/pull", request, streamer, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<PullModelResponse?> PullModel(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<PullModelRequest, PullModelResponse?>("api/pull", request, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public Task PushModel(PushModelRequest modelRequest, IResponseStreamer<PushModelResponse> streamer, CancellationToken cancellationToken = default)
	 => StreamPostAsync("api/push", modelRequest, streamer, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<PushModelResponse?> PushModel(PushModelRequest modelRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<PushModelRequest, PushModelResponse?>(
			"api/push", modelRequest, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request, CancellationToken cancellationToken = default)
	 => PostAsync<GenerateEmbeddingRequest, GenerateEmbeddingResponse>("api/embeddings", request, cancellationToken);

	/// <inheritdoc />
	public Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream?> streamer, CancellationToken cancellationToken = default)
	 => GenerateCompletion(request, streamer, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<GenerateCompletionResponseStream?> StreamCompletion(GenerateCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = GenerateCompletion(request, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public async Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request, CancellationToken cancellationToken = default)
	{
		var builder = new StringBuilder();
		var streamer = new ActionResponseStreamer<GenerateCompletionResponseStream?>(status => builder.Append(status?.Response));

		var result = await GenerateCompletion(request, streamer, cancellationToken);
		return new ConversationContextWithResponse(builder.ToString(), result.Context);
	}

	/// <inheritdoc />
	public async Task<ChatResponse> Chat(ChatRequest chatRequest, CancellationToken cancellationToken = default)
	{
		chatRequest.Stream = false;
		var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
		{
			Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
		};

		var completion = chatRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ProcessChatResponseAsync(response);
	}

	/// <inheritdoc />
	public async Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream?> streamer, CancellationToken cancellationToken = default)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
		{
			Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
		};

		var completion = chatRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ProcessStreamedChatResponseAsync(chatRequest, response, streamer, cancellationToken);
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseStream?> StreamChat(ChatRequest chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
		{
			Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
		};

		var completion = chatRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);

		response.EnsureSuccessStatusCode();

		var stream = ProcessStreamedChatResponseAsync(response, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public async Task<bool> IsRunning(CancellationToken cancellationToken = default)
	{
		var response = await _client.GetAsync("", cancellationToken); // without route returns "Ollama is running"
		response.EnsureSuccessStatusCode();
		var stringContent = await response.Content.ReadAsStringAsync();
		return !string.IsNullOrWhiteSpace(stringContent);
	}

	/// <inheritdoc />
	public async Task<Version> GetVersion(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<JsonNode>("api/version", cancellationToken);
		return Version.Parse(data["version"]?.ToString());
	}

	private async Task<ConversationContext> GenerateCompletion(GenerateCompletionRequest generateRequest, IResponseStreamer<GenerateCompletionResponseStream?> streamer, CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
		};

		var completion = generateRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await ProcessStreamedCompletionResponseAsync(response, streamer, cancellationToken);
	}

	private async IAsyncEnumerable<GenerateCompletionResponseStream?> GenerateCompletion(GenerateCompletionRequest generateRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
		};

		var completion = generateRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);
		response.EnsureSuccessStatusCode();

		var stream = ProcessStreamedCompletionResponseAsync(response, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	private async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken)
	{
		var response = await _client.GetAsync(endpoint, cancellationToken);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody)!;
	}

	private async Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content, cancellationToken);
		response.EnsureSuccessStatusCode();
	}

	private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content, cancellationToken);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody)!;
	}

	private async Task StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest requestModel, IResponseStreamer<TResponse> streamer, CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		response.EnsureSuccessStatusCode();

		await ProcessStreamedResponseAsync(response, streamer, cancellationToken);
	}

	private async IAsyncEnumerable<TResponse?> StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest requestModel, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		response.EnsureSuccessStatusCode();

		var stream = ProcessStreamedResponseAsync<TResponse>(response, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	private static async Task ProcessStreamedResponseAsync<TLine>(HttpResponseMessage response, IResponseStreamer<TLine> streamer, CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<TLine>(line);
			streamer.Stream(streamedResponse!);
		}
	}

	private static async IAsyncEnumerable<TLine?> ProcessStreamedResponseAsync<TLine>(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			yield return JsonSerializer.Deserialize<TLine?>(line);
		}
	}

	private static async Task<ConversationContext> ProcessStreamedCompletionResponseAsync(
		HttpResponseMessage response,
		IResponseStreamer<GenerateCompletionResponseStream?> streamer,
		CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<GenerateCompletionResponseStream>(line);
			streamer.Stream(streamedResponse);

			if (streamedResponse?.Done ?? false)
			{
				var doneResponse = JsonSerializer.Deserialize<GenerateCompletionDoneResponseStream>(line)!;
				return new ConversationContext(doneResponse.Context);
			}
		}

		return new ConversationContext(Array.Empty<long>());
	}

	private static async IAsyncEnumerable<GenerateCompletionResponseStream?> ProcessStreamedCompletionResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<GenerateCompletionResponseStream>(line);

			yield return streamedResponse?.Done ?? false
				? JsonSerializer.Deserialize<GenerateCompletionDoneResponseStream>(line)!
				: streamedResponse;
		}
	}

	private static async Task<ChatResponse> ProcessChatResponseAsync(HttpResponseMessage response)
	{
		var responseBody = await response.Content.ReadAsStringAsync();
		var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseBody);

		return chatResponse!;
	}

	private static async Task<IEnumerable<Message>> ProcessStreamedChatResponseAsync(ChatRequest chatRequest, HttpResponseMessage response, IResponseStreamer<ChatResponseStream?> streamer, CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		ChatRole? responseRole = null;
		var responseContent = new StringBuilder();

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();

			var streamedResponse = JsonSerializer.Deserialize<ChatResponseStream>(line);

			// keep the streamed content to build the last message
			// to return the list of messages
			responseRole ??= streamedResponse?.Message.Role;
			responseContent.Append(streamedResponse?.Message.Content ?? "");

			streamer.Stream(streamedResponse);

			if (streamedResponse?.Done ?? false)
			{
				var messages = chatRequest.Messages?.ToList() ?? new List<Message>();
				var message = new Message(responseRole, responseContent.ToString());
				if (streamedResponse.Message.ToolCalls?.Any() ?? false)
					message.ToolCalls = streamedResponse.Message.ToolCalls.ToList();
				messages.Add(message);
				return messages;
			}
		}

		return Array.Empty<Message>();
	}

	private static async IAsyncEnumerable<ChatResponseStream?> ProcessStreamedChatResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			yield return JsonSerializer.Deserialize<ChatResponseStream>(line);
		}
	}

	/// <summary>
	/// The configuration for the Ollama API client
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// Gets or sets the uri of the Ollama API endpoint
		/// </summary>
		public Uri Uri { get; set; } = null!;

		/// <summary>
		/// Gets or sets the model that should be used
		/// </summary>
		public string Model { get; set; } = null!;
	}
}

public record ConversationContext(long[] Context);

public record ConversationContextWithResponse(string Response, long[] Context) :
	ConversationContext(Context);