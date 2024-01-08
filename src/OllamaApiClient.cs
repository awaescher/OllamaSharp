using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharp
{
	// https://github.com/jmorganca/ollama/blob/main/docs/api.md
	public class OllamaApiClient : IOllamaApiClient
	{
		public class Configuration
		{
			public Uri Uri { get; set; }

			public string Model { get; set; }
		}

		private readonly HttpClient _client;

		public Configuration Config { get; }

		public string SelectedModel { get; set; }

		public OllamaApiClient(string uriString, string defaultModel = "")
			: this(new Uri(uriString), defaultModel)
		{
		}

		public OllamaApiClient(Uri uri, string defaultModel = "")
			: this(new Configuration { Uri = uri, Model = defaultModel })
		{
		}

		public OllamaApiClient(Configuration config)
			: this(new HttpClient() { BaseAddress = config.Uri }, config.Model)
		{
		}

		public OllamaApiClient(HttpClient client, string defaultModel = "")
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			SelectedModel = defaultModel;
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

		public async Task CopyModel(CopyModelRequest request)
		{
			await PostAsync("/api/copy", request);
		}

		public async Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer)
		{
			await StreamPostAsync("/api/pull", request, streamer);
		}

		public async Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer)
		{
			await StreamPostAsync("/api/push", request, streamer);
		}

		public async Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request)
		{
			return await PostAsync<GenerateEmbeddingRequest, GenerateEmbeddingResponse>("/api/embeddings", request);
		}

		public async Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer)
		{
			return await GenerateCompletion(request, streamer);
		}

		public async Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request)
		{
			var builder = new StringBuilder();
			var result = await GenerateCompletion(request, new ActionResponseStreamer<GenerateCompletionResponseStream>(status => builder.Append(status.Response)));
			return new ConversationContextWithResponse(builder.ToString(), result.Context);
		}

		public async Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
			{
				Content = new StringContent(JsonSerializer.Serialize(chatRequest), Encoding.UTF8, "application/json")
			};

			var completion = chatRequest.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

			using var response = await _client.SendAsync(request, completion);
			response.EnsureSuccessStatusCode();

			return await ProcessStreamedChatResponseAsync(chatRequest, response, streamer);
		}

		private async Task<ConversationContext> GenerateCompletion(GenerateCompletionRequest generateRequest, IResponseStreamer<GenerateCompletionResponseStream> streamer)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
			{
				Content = new StringContent(JsonSerializer.Serialize(generateRequest), Encoding.UTF8, "application/json")
			};

			var completion = generateRequest.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

			using var response = await _client.SendAsync(request, completion);
			response.EnsureSuccessStatusCode();

			return await ProcessStreamedCompletionResponseAsync(response, streamer);
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

		private static async Task<IEnumerable<Message>> ProcessStreamedChatResponseAsync(ChatRequest chatRequest, HttpResponseMessage response, IResponseStreamer<ChatResponseStream> streamer)
		{
			using var stream = await response.Content.ReadAsStreamAsync();
			using var reader = new StreamReader(stream);

			ChatRole? responseRole = null;
			var responseContent = new StringBuilder();

			while (!reader.EndOfStream)
			{
				string line = await reader.ReadLineAsync();

				var streamedResponse = JsonSerializer.Deserialize<ChatResponseStream>(line);

				// keep the streamed content to build the last message
				// to return the list of messages
				responseRole = streamedResponse?.Message?.Role;
				responseContent.Append(streamedResponse?.Message?.Content ?? "");

				streamer.Stream(streamedResponse);

				if (streamedResponse?.Done ?? false)
				{
					var doneResponse = JsonSerializer.Deserialize<ChatDoneResponseStream>(line);
					var messages = chatRequest.Messages.ToList();
					messages.Add(new Message(responseRole, responseContent.ToString()));
					return messages;
				}
			}

			return Array.Empty<Message>();
		}
	}

	public record ConversationContext(long[] Context);

	public record ConversationContextWithResponse(string Response, long[] Context) : ConversationContext(Context);
}
