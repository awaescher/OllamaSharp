using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// https://github.com/jmorganca/ollama/blob/main/docs/api.md
public class OllamaApiClient
{
	private readonly HttpClient _client;

	public OllamaApiClient(Uri baseAddress)
		: this(new HttpClient() { BaseAddress = baseAddress })
	{
	}

	public OllamaApiClient(HttpClient client)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
	}

	public async Task CreateModel(string name, string path, Action<CreateStatus> streamer)
	{
		await CreateModel(name, path, new ActionResponseStreamer<CreateStatus>(streamer));
	}

	public async Task CreateModel(string name, string path, IResponseStreamer<CreateStatus> streamer)
	{
		await CreateModel(new CreateModelRequest { Name = name, Path = path, Stream = true }, streamer);
	}

	public async Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer)
	{
		await StreamPostAsync("/api/create", request, streamer);
	}

	public async Task DeleteModel(string model)
	{
		var request = new HttpRequestMessage(HttpMethod.Delete, "/api/delete")
		{
			Content = new StringContent(JsonSerializer.Serialize(new DeleteModelRequest { Name = model }), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request);
		response.EnsureSuccessStatusCode();
	}

	public async Task<IEnumerable<Model>> ListLocalModels()
	{
		var data = await GetAsync<ListModelsResponse>("/api/tags");
		return data.Models;
	}

	public async Task<ShowModelResponse> ShowModelInformation(string model)
	{
		return await PostAsync<ShowModelRequest, ShowModelResponse>("/api/show", new ShowModelRequest { Name = model });
	}

	public async Task CopyModel(string source, string destination)
	{
		await CopyModel(new CopyModelRequest { Source = source, Destination = destination });
	}

	public async Task CopyModel(CopyModelRequest request)
	{
		await PostAsync("/api/copy", request);
	}

	public async Task PullModel(string model, Action<PullStatus> streamer)
	{
		await PullModel(model, new ActionResponseStreamer<PullStatus>(streamer));
	}

	public async Task PullModel(string model, IResponseStreamer<PullStatus> streamer)
	{
		await PullModel(new PullModelRequest { Name = model }, streamer);
	}

	public async Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer)
	{
		await StreamPostAsync("/api/pull", request, streamer);
	}

	public async Task PushModel(string name, Action<PushStatus> streamer)
	{
		await PushModel(name, new ActionResponseStreamer<PushStatus>(streamer));
	}

	public async Task PushModel(string name, IResponseStreamer<PushStatus> streamer)
	{
		await PushModel(new PushRequest { Name = name, Stream = true }, streamer);
	}

	public async Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer)
	{
		await StreamPostAsync("/api/push", request, streamer);
	}

	public async Task<GenerateEmbeddingResponse> GenerateEmbeddings(string model, string prompt)
	{
		return await GenerateEmbeddings(new GenerateEmbeddingRequest { Model = model, Prompt = prompt });
	}

	public async Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request)
	{
		return await PostAsync<GenerateEmbeddingRequest, GenerateEmbeddingResponse>("/api/embeddings", request);
	}

	private async Task<TResponse> GetAsync<TResponse>(string endpoint)
	{
		var response = await _client.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<TResponse>(responseBody);
	}

	private async Task PostAsync<TRequest>(string endpoint, TRequest request)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content);
		response.EnsureSuccessStatusCode();
	}

	private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
	{
		var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(endpoint, content);
		response.EnsureSuccessStatusCode();

		var responseBody = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TResponse>(responseBody);
	}

	private async Task StreamPostAsync<TRequest, TResponse>(string endpoint, TRequest requestModel, IResponseStreamer<TResponse> streamer)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json")
		};

		using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
		response.EnsureSuccessStatusCode();

		await ProcessStreamedResponseAsync(response, streamer);
	}

	private static async Task ProcessStreamedResponseAsync<TLine>(HttpResponseMessage response, IResponseStreamer<TLine> streamer)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream)
		{
			string line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<TLine>(line);
			streamer.Stream(streamedResponse);
		}
	}

	public async Task<ConversationContext> StreamCompletion(string prompt, string model, ConversationContext context, Action<GenerateCompletionResponseStream> streamer)
	{
		return await StreamCompletion(prompt, model, context, new ActionResponseStreamer<GenerateCompletionResponseStream>(streamer));
	}

	public async Task<ConversationContext> StreamCompletion(string prompt, string model, ConversationContext context, IResponseStreamer<GenerateCompletionResponseStream> streamer)
	{
		var generateRequest = new GenerateCompletionRequest
		{
			Prompt = prompt,
			Model = model,
			Stream = true,
			Context = context?.Context ?? Array.Empty<long>()
		};

		return await GenerateCompletion(generateRequest, streamer);
	}

	public async Task<ConversationContextWithResponse> GetCompletion(string prompt, string model, ConversationContext context)
	{
		var generateRequest = new GenerateCompletionRequest
		{
			Prompt = prompt,
			Model = model,
			Stream = false,
			Context = context?.Context ?? Array.Empty<long>()
		};

		var builder = new StringBuilder();
		var result = await GenerateCompletion(generateRequest, new ActionResponseStreamer<GenerateCompletionResponseStream>(status => builder.Append(status.Response)));
		return new ConversationContextWithResponse(builder.ToString(), result.Context);
	}

	public async Task<ConversationContext> GenerateCompletion(GenerateCompletionRequest generateRequest, IResponseStreamer<GenerateCompletionResponseStream> streamer)
	{
		var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
		{
			Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
		};

		var completionOption = generateRequest.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

		using var response = await _client.SendAsync(request, completionOption);
		response.EnsureSuccessStatusCode();

		return await ProcessStreamedCompletionResponseAsync(response, streamer);
	}

	private static async Task<ConversationContext> ProcessStreamedCompletionResponseAsync(HttpResponseMessage response, IResponseStreamer<GenerateCompletionResponseStream> streamer)
	{
		using var stream = await response.Content.ReadAsStreamAsync();
		using var reader = new StreamReader(stream);

		while (!reader.EndOfStream)
		{
			string line = await reader.ReadLineAsync();
			var streamedResponse = JsonSerializer.Deserialize<GenerateCompletionResponseStream>(line);
			streamer.Stream(streamedResponse);

			if (streamedResponse?.Done ?? false)
			{
				var doneResponse = JsonSerializer.Deserialize<GenerateCompletionDoneResponseStream>(line);
				return new ConversationContext(doneResponse.Context);
			}
		}

		return new ConversationContext(Array.Empty<long>());
	}
}

public record ConversationContext(long[] Context);

public record ConversationContextWithResponse(string Response, long[] Context) : ConversationContext(Context);
