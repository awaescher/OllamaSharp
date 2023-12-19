using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Tests;

public class OllamaApiClientTests
{
	private OllamaApiClient _client;
	private HttpResponseMessage _response;

	[OneTimeSetUp]
	public void Setup()
	{
		var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

		mockHandler
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(_ => true),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(() => _response);

		var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://empty") };
		_client = new OllamaApiClient(httpClient);
	}

	public class GetCompletionMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Returns_Streamed_Responses_At_Once()
		{
			await using var stream = new MemoryStream();

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteCompletionStreamResponse("The ");
			await writer.WriteCompletionStreamResponse("sky ");
			await writer.WriteCompletionStreamResponse("is ");
			await writer.FinishCompletionStreamResponse("blue.", context: new int[] { 1, 2, 3 });
			stream.Seek(0, SeekOrigin.Begin);

			var context = await _client.GetCompletion("prompt", "model", null);

			context.Response.Should().Be("The sky is blue.");
			context.Context.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
		}
	}

	public class StreamCompletionMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Streams_Response_Chunks()
		{
			await using var stream = new MemoryStream();

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteCompletionStreamResponse("The ");
			await writer.WriteCompletionStreamResponse("sky ");
			await writer.WriteCompletionStreamResponse("is ");
			await writer.FinishCompletionStreamResponse("blue.", context: new int[] { 1, 2, 3 });
			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();
			var context = await _client.StreamCompletion("prompt", "model", null, s => builder.Append(s.Response));

			builder.ToString().Should().Be("The sky is blue.");
			context.Context.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
		}
	}

	public class ChatMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Streams_Response_Message_Chunks()
		{
			await using var stream = new MemoryStream();

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteChatStreamResponse("Leave ", "assistant");
			await writer.WriteChatStreamResponse("me ", "assistant");
			await writer.FinishChatStreamResponse("alone.", "assistant");
			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();

			var chat = new ChatRequest
			{
				Model = "model",
				Messages = new Message[]
				{
					new() { Role = "user", Content = "Why?" },
					new() { Role = "assistant", Content = "Because!" },
					new() { Role = "user", Content = "And where?" },
				}
			};

			var messages = (await _client.Chat(chat, s => builder.Append(s.Message))).ToArray();

			messages.Length.Should().Be(4);

			messages[0].Role.Should().Be("user");
			messages[0].Content.Should().Be("Why?");

			messages[1].Role.Should().Be("assistant");
			messages[1].Content.Should().Be("Because!");

			messages[2].Role.Should().Be("user");
			messages[2].Content.Should().Be("And where?");

			messages[3].Role.Should().Be("assistant");
			messages[3].Content.Should().Be("Leave me alone.");
		}
	}

	public class ListLocalModelsAsyncMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n\"models\": [\r\n{\r\n\"name\": \"codellama:latest\",\r\n\"modified_at\": \"2023-10-12T14:17:04.967950259+02:00\",\r\n\"size\": 3791811617,\r\n\"digest\": \"36893bf9bc7ff7ace56557cd28784f35f834290c85d39115c6b91c00a031cfad\"\r\n},\r\n{\r\n\"name\": \"llama2:latest\",\r\n\"modified_at\": \"2023-10-02T14:10:14.78152065+02:00\",\r\n\"size\": 3791737662,\r\n\"digest\": \"d5611f7c428cf71fb05660257d18e043477f8b46cf561bf86940c687c1a59f70\"\r\n},\r\n{\r\n\"name\": \"mistral:latest\",\r\n\"modified_at\": \"2023-10-02T14:16:24.841447764+02:00\",\r\n\"size\": 4108916688,\r\n\"digest\": \"8aa307f73b2622af521e8f22d46e4b777123c4df91898dcb2e4079dc8fdf579e\"\r\n},\r\n{\r\n\"name\": \"vicuna:latest\",\r\n\"modified_at\": \"2023-10-06T09:44:16.936312659+02:00\",\r\n\"size\": 3825517709,\r\n\"digest\": \"675fa173a76abc48325d395854471961abf74b664d91e92ffb4fc03e0bde652b\"\r\n}\r\n]\r\n}\r\n")
			};

			var models = await _client.ListLocalModels();
			models.Count().Should().Be(4);

			var first = models.First();
			first.Name.Should().Be("codellama:latest");
			first.ModifiedAt.Date.Should().Be(new DateTime(2023, 10, 12));
			first.Size.Should().Be(3791811617);
			first.Digest.Should().StartWith("36893bf9bc7ff7ace5655");
		}
	}

	public class ShowMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n  \"license\": \"<contents of license block>\",\r\n  \"modelfile\": \"# Modelfile generated by \\\"ollama show\\\"\\n\\n\",\r\n  \"parameters\": \"stop                           [INST]\\nstop [/INST]\\nstop <<SYS>>\\nstop <</SYS>>\",\r\n  \"template\": \"[INST] {{ if and .First .System }}<<SYS>>{{ .System }}<</SYS>>\\n\\n{{ end }}{{ .Prompt }} [/INST] \"\r\n}")
			};

			var info = await _client.ShowModelInformation("codellama:latest");

			info.License.Should().Contain("contents of license block");
			info.Modelfile.Should().StartWith("# Modelfile generated");
			info.Parameters.Should().StartWith("stop");
			info.Template.Should().StartWith("[INST]");
		}
	}

	public class GenerateEmbeddingsMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n  \"embedding\": [\r\n    0.5670403838157654, 0.009260174818336964, 0.23178744316101074, -0.2916173040866852, -0.8924556970596313  ]\r\n}")
			};

			var info = await _client.GenerateEmbeddings(new GenerateEmbeddingRequest { Model = "", Prompt = "" });

			info.Embedding.Should().HaveCount(5);
			info.Embedding.First().Should().BeApproximately(0.567, precision: 0.01);
		}
	}
}

public static class WriterExtensions
{
	public static async Task WriteCompletionStreamResponse(this StreamWriter writer, string response)
	{
		var json = new { response, done = false };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	public static async Task FinishCompletionStreamResponse(this StreamWriter writer, string response, int[] context)
	{
		var json = new { response, done = true, context };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	public static async Task WriteChatStreamResponse(this StreamWriter writer, string content, string role)
	{
		var json = new { message = new { content, role }, role, done = false };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	public static async Task FinishChatStreamResponse(this StreamWriter writer, string content, string role)
	{
		var json = new { message = new { content, role }, role, done = true };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}
}