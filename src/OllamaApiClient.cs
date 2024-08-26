using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;

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
			Uri = client.BaseAddress ?? throw new InvalidOperationException("HttpClient base address is not set!"),
			Model = defaultModel
		};
		SelectedModel = defaultModel;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<CreateModelResponse?> CreateModel(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<CreateModelRequest, CreateModelResponse?>("api/create", request, cancellationToken))
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

		await EnsureSuccessStatusCode(response);
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
	public Task<ShowModelResponse> ShowModel(string model, CancellationToken cancellationToken = default)
		=> PostAsync<ShowModelRequest, ShowModelResponse>("api/show", new ShowModelRequest { Model = model }, cancellationToken);

	/// <inheritdoc />
	public Task CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default)
		=> PostAsync("api/copy", request, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<PullModelResponse?> PullModel(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in StreamPostAsync<PullModelRequest, PullModelResponse?>("api/pull", request, cancellationToken))
			yield return result;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<PushModelResponse?> PushModel(PushModelRequest modelRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var stream = StreamPostAsync<PushModelRequest, PushModelResponse?>(
			"api/push", modelRequest, cancellationToken);

		await foreach (var result in stream)
			yield return result;
	}

	/// <inheritdoc />
	public Task<EmbedResponse> Embed(EmbedRequest request, CancellationToken cancellationToken = default)
		=> PostAsync<EmbedRequest, EmbedResponse>("api/embed", request, cancellationToken);

	/// <inheritdoc />
	public async IAsyncEnumerable<GenerateResponseStream?> Generate(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var result in GenerateCompletion(request, cancellationToken))
			yield return result;
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<ChatResponseStream?> Chat(ChatRequest chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
		{
			Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
		};

		var completion = chatRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);

		await EnsureSuccessStatusCode(response);

		await foreach (var result in ProcessStreamedChatResponseAsync(response, cancellationToken))
			yield return result;
	}

	/// <inheritdoc />
	public async Task<bool> IsRunning(CancellationToken cancellationToken = default)
	{
		var response = await _client.GetAsync("", cancellationToken); // without route returns "Ollama is running"
		await EnsureSuccessStatusCode(response);
		var stringContent = await response.Content.ReadAsStringAsync();
		return !string.IsNullOrWhiteSpace(stringContent);
	}

	/// <inheritdoc />
	public async Task<Version> GetVersion(CancellationToken cancellationToken = default)
	{
		var data = await GetAsync<JsonNode>("api/version", cancellationToken);
		return Version.Parse(data["version"]?.ToString());
	}

	private async IAsyncEnumerable<GenerateResponseStream?> GenerateCompletion(GenerateRequest generateRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
		};

		var completion = generateRequest.Stream
			? HttpCompletionOption.ResponseHeadersRead
			: HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completion, cancellationToken);

		await EnsureSuccessStatusCode(response);

		await foreach (var result in ProcessStreamedCompletionResponseAsync(response, cancellationToken))
			yield return result;
	}

	private async Task<TResponse> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken)
	{
		var response = await _client.GetAsync(endpoint, cancellationToken);

		await EnsureSuccessStatusCode(response);

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody)!;
	}

	private async Task PostAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content, cancellationToken);

		await EnsureSuccessStatusCode(response);
	}

	private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content, cancellationToken);

		await EnsureSuccessStatusCode(response);

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody)!;
	}

	private async IAsyncEnumerable<TResponse?> StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest requestModel, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		await EnsureSuccessStatusCode(response);

		await foreach (var result in ProcessStreamedResponseAsync<TResponse>(response, cancellationToken))
			yield return result;
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

	private static async IAsyncEnumerable<GenerateResponseStream?> ProcessStreamedCompletionResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<GenerateResponseStream>(line);

			yield return streamedResponse?.Done ?? false
				? JsonSerializer.Deserialize<GenerateDoneResponseStream>(line)!
				: streamedResponse;
		}
	}

	private static async IAsyncEnumerable<ChatResponseStream?> ProcessStreamedChatResponseAsync(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
		{
			var line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<ChatResponseStream>(line);

			yield return streamedResponse?.Done ?? false
				? JsonSerializer.Deserialize<ChatDoneResponseStream>(line)!
				: streamedResponse;
		}
	}

	private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
	{
		if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
		{
			var body = await response.Content.ReadAsStringAsync() ?? string.Empty;

			var errorElement = new JsonElement();
			var couldParse = JsonDocument.Parse(body)?.RootElement.TryGetProperty("error", out errorElement) ?? false;
			var errorString = (couldParse ? errorElement.GetString() : body) ?? string.Empty;

			if (errorString.Contains("does not support tools"))
				throw new ModelDoesNotSupportToolsException(errorString);

			throw new OllamaException(errorString);
		}

		response.EnsureSuccessStatusCode();
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