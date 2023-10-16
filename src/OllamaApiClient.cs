using System.Text;
using System.Text.Json;

// https://github.com/jmorganca/ollama/blob/main/docs/api.md

public class OllamaApiClient
{
	private HttpClient _client;

	public OllamaApiClient(Uri baseAddress)
		: this(new HttpClient() { BaseAddress = baseAddress })
	{
	}

	public OllamaApiClient(HttpClient client)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
	}

	public async Task<string> PostAsync<TRequest>(string endpoint, TRequest request)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();

		return responseBody;
	}

	public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody);
	}

	public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
	{
		return await _client.PostAsync(requestUri, content);
	}

	public async Task<TResponse> DeleteAsync<TResponse>(string endpoint)
	{
		var response = await _client.DeleteAsync(endpoint);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<TResponse>(responseBody);
	}

	public async Task<TResponse> GetAsync<TResponse>(string endpoint)
	{
		var response = await _client.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<TResponse>(responseBody);
	}

	public async Task<string> GenerateAsync(GenerateRequest request)
	{
		return await PostAsync<GenerateRequest>("/api/generate", request);
	}

	public async Task<TResponse> CreateModelAsync<TResponse>(CreateRequest request)
	{
		return await PostAsync<CreateRequest, TResponse>("/api/create", request);
	}

	// Hier fügen Sie ähnliche Methoden für andere Endpunkte hinzu...

	public async Task<TResponse> DeleteModelAsync<TResponse>(string modelName)
	{
		return await DeleteAsync<TResponse>($"/api/delete?model={modelName}");
	}

	public async Task<IEnumerable<Model>> ListLocalModelsAsync()
	{
		var data = await GetAsync<TagResponse>("/api/tags");
		return data.Models;
	}

	public async Task<ShowResponse> ShowModelAsync(string model)
	{
		return await PostAsync<ShowRequest, ShowResponse>("/api/show", new ShowRequest { Name = model });
	}

	public async Task<TResponse> CopyModelAsync<TResponse>(CopyRequest request)
	{
		return await PostAsync<CopyRequest, TResponse>("/api/copy", request);
	}

	public async Task<TResponse> PullModelAsync<TResponse>(PullRequest request)
	{
		return await PostAsync<PullRequest, TResponse>("/api/pull", request);
	}

	public async Task<TResponse> PushModelAsync<TResponse>(PushRequest request)
	{
		return await PostAsync<PushRequest, TResponse>("/api/push", request);
	}

	public async Task<EmbeddingsResponse> GenerateEmbeddingsAsync(EmbeddingsRequest request)
	{
		return await PostAsync<EmbeddingsRequest, EmbeddingsResponse>("/api/embeddings", request);
	}

	public async Task<ConversationContext> GenerateAsync(string prompt, string model, IResponseStreamer streamer, ConversationContext context)
	{
		var generateRequest = new GenerateRequest
		{
			Prompt = prompt,
			Model = model,
			Stream = true,
			Context = context?.Context ?? Array.Empty<long>()
		};

		var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
		response.EnsureSuccessStatusCode();

		return await ProcessStreamedResponseAsync(response, streamer);
	}

	private async Task<ConversationContext> ProcessStreamedResponseAsync(HttpResponseMessage response, IResponseStreamer streamer)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream)
		{
			string line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<StreamedResponse>(line);
			streamer.Stream(streamedResponse);

			if (streamedResponse?.Done ?? false)
			{
				var doneResponse = JsonSerializer.Deserialize<DoneStreamedResponse>(line);
				return new ConversationContext(doneResponse.Context);
			}
		}

		return new ConversationContext(Array.Empty<long>());
	}
}


public record ConversationContext(long[] Context);