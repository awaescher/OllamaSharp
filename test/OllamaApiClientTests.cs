using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

public class OllamaApiClientTests
{
	private OllamaApiClient _client;
	private HttpResponseMessage? _response;

	[OneTimeSetUp]
	public void Setup()
	{
		var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

		mockHandler
			.Protected()
			.Setup<Task<HttpResponseMessage?>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(_ => true),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(() => _response);

		var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://empty") };
		_client = new OllamaApiClient(httpClient);
	}

	public class CreateModelMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Streams_Status_Updates()
		{
			await using var stream = new MemoryStream();

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteLineAsync("{\"status\": \"Creating model\"}");
			await writer.WriteLineAsync("{\"status\": \"Downloading model\"}");
			await writer.WriteLineAsync("{\"status\": \"Model created\"}");
			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();
			var modelStream = _client.CreateModel(
				new CreateModelRequest(),
				CancellationToken.None);

			await foreach (var status in modelStream)
				builder.Append(status?.Status);

			builder.ToString().Should().Be("Creating modelDownloading modelModel created");
		}
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

			var context = await _client.GetCompletion("prompt", null, CancellationToken.None);

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
			var context = await _client.StreamCompletion("prompt", null, s => builder.Append(s?.Response), CancellationToken.None);

			builder.ToString().Should().Be("The sky is blue.");
			context.Context.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
		}

		[Test]
		public async Task Streams_Response_Chunks_As_AsyncEnumerable()
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
			await writer.FinishCompletionStreamResponse("blue.", context: [1, 2, 3]);
			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();
			var completionStream = _client.StreamCompletion("prompt", null, CancellationToken.None);
			GenerateCompletionDoneResponseStream? final = null!;
			await foreach (var response in completionStream)
			{
				builder.Append(response?.Response);
				if (response?.Done ?? false)
					final = (GenerateCompletionDoneResponseStream)response;
			}

			builder.ToString().Should().Be("The sky is blue.");
			final.Should().NotBeNull();
			final.Context.Should().NotBeNull();
			final.Context.Should().BeEquivalentTo(new[] { 1, 2, 3 });
		}
	}

	public class SendChatMethod : OllamaApiClientTests
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
			await writer.WriteChatStreamResponse("Leave ", ChatRole.Assistant);
			await writer.WriteChatStreamResponse("me ", ChatRole.Assistant);
			await writer.FinishChatStreamResponse("alone.", ChatRole.Assistant);
			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();

			var chat = new ChatRequest
			{
				Model = "model",
				Messages = new Message[]
				{
					new(ChatRole.User, "Why?"),
					new(ChatRole.Assistant, "Because!"),
					new(ChatRole.User, "And where?"),
				}
			};

			var messages = (await _client.SendChat(chat, s => builder.Append(s?.Message), CancellationToken.None)).ToArray();

			messages.Length.Should().Be(4);

			messages[0].Role.Should().Be(ChatRole.User);
			messages[0].Content.Should().Be("Why?");

			messages[1].Role.Should().Be(ChatRole.Assistant);
			messages[1].Content.Should().Be("Because!");

			messages[2].Role.Should().Be(ChatRole.User);
			messages[2].Content.Should().Be("And where?");

			messages[3].Role.Should().Be(ChatRole.Assistant);
			messages[3].Content.Should().Be("Leave me alone.");
		}
	}

	public class ChatMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Receives_Response_Message_With_Metadata()
		{
			await using var stream = new MemoryStream();

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteAsync(
				"""
				{
				    "model": "llama2",
				    "created_at": "2024-07-12T12:34:39.63897616Z",
				    "message": {
				        "role": "assistant",
				        "content": "Test content."
				    },
				    "done_reason": "stop",
				    "done": true,
				    "total_duration": 137729492272,
				    "load_duration": 133071702768,
				    "prompt_eval_count": 26,
				    "prompt_eval_duration": 35137000,
				    "eval_count": 323,
				    "eval_duration": 4575154000
				}
				""");
			stream.Seek(0, SeekOrigin.Begin);

			var chat = new ChatRequest
			{
				Model = "model",
				Messages = [
					new(ChatRole.User, "Why?"),
					new(ChatRole.Assistant, "Because!"),
					new(ChatRole.User, "And where?")]
			};

			var result = await _client.Chat(chat, CancellationToken.None);

			result.Message.Role.Should().Be(ChatRole.Assistant);
			result.Message.Content.Should().Be("Test content.");
			result.Done.Should().BeTrue();
			result.DoneReason.Should().Be("stop");
			result.TotalDuration.Should().Be(137729492272);
			result.LoadDuration.Should().Be(133071702768);
			result.PromptEvalCount.Should().Be(26);
			result.PromptEvalDuration.Should().Be(35137000);
			result.EvalCount.Should().Be(323);
			result.EvalDuration.Should().Be(4575154000);
		}
	}

	public class StreamChatMethod : OllamaApiClientTests
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
			await writer.WriteChatStreamResponse("Leave ", ChatRole.Assistant);
			await writer.WriteChatStreamResponse("me ", ChatRole.Assistant);
			await writer.FinishChatStreamResponse("alone.", ChatRole.Assistant);
			stream.Seek(0, SeekOrigin.Begin);

			var chat = new ChatRequest
			{
				Model = "model",
				Messages = new Message[]
				{
					new(ChatRole.User, "Why?"),
					new(ChatRole.Assistant, "Because!"),
					new(ChatRole.User, "And where?"),
				}
			};

			var chatStream = _client.StreamChat(chat, CancellationToken.None);
			var builder = new StringBuilder();
			var responses = new List<Message?>();

			await foreach (var response in chatStream)
			{
				builder.Append(response?.Message.Content);
				responses.Add(response?.Message);
			}

			var chatResponse = builder.ToString();

			chatResponse.Should().BeEquivalentTo("Leave me alone.");

			responses.Should().HaveCount(3);
			responses[0]!.Role.Should().Be(ChatRole.Assistant);
			responses[1]!.Role.Should().Be(ChatRole.Assistant);
			responses[2]!.Role.Should().Be(ChatRole.Assistant);
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

			var models = await _client.ListLocalModels(CancellationToken.None);
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

			var info = await _client.ShowModelInformation("codellama:latest", CancellationToken.None);

			info.License.Should().Contain("contents of license block");
			info.Modelfile.Should().StartWith("# Modelfile generated");
			info.Parameters.Should().StartWith("stop");
			info.Template.Should().StartWith("[INST]");
		}

		[Test]
		public async Task Returns_Deserialized_Model_WithSystem()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\"modelfile\":\"# Modelfile generated by \\\"ollama show\\\"\\n# To build a new Modelfile based on this, replace FROM with:\\n# FROM magicoder:latest\\n\\nFROM C:\\\\Users\\\\jd\\\\.ollama\\\\models\\\\blobs\\\\sha256-4a501ed4ce55e5611922b3ee422501ff7cc773b472d196c3c416859b6d375273\\nTEMPLATE \\\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\\\"\\nSYSTEM You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\\nPARAMETER num_ctx 16384\\n\",\"parameters\":\"num_ctx                        16384\",\"template\":\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\",\"system\":\"You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":null,\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"},\"model_info\":{\"general.architecture\":\"llama\",\"general.file_type\":2,\"general.parameter_count\":8829407232,\"general.quantization_version\":2,\"llama.attention.head_count\":32,\"llama.attention.head_count_kv\":4,\"llama.attention.layer_norm_rms_epsilon\":0.000001,\"llama.block_count\":48,\"llama.context_length\":4096,\"llama.embedding_length\":4096,\"llama.feed_forward_length\":11008,\"llama.rope.dimension_count\":128,\"llama.rope.freq_base\":5000000,\"llama.vocab_size\":64000,\"tokenizer.ggml.add_bos_token\":false,\"tokenizer.ggml.add_eos_token\":false,\"tokenizer.ggml.bos_token_id\":1,\"tokenizer.ggml.eos_token_id\":2,\"tokenizer.ggml.model\":\"llama\",\"tokenizer.ggml.padding_token_id\":0,\"tokenizer.ggml.pre\":\"default\",\"tokenizer.ggml.scores\":[],\"tokenizer.ggml.token_type\":[],\"tokenizer.ggml.tokens\":[]},\"modified_at\":\"2024-05-14T23:33:07.4166573+08:00\"}")
			};

			var info = await _client.ShowModelInformation("starcoder:latest", CancellationToken.None);

			info.License.Should().BeNullOrEmpty();
			info.Modelfile.Should().StartWith("# Modelfile generated");
			info.Parameters.Should().StartWith("num_ctx");
			info.Template.Should().StartWith("{{ .System }}");
			info.System.Should().StartWith("You are an exceptionally intelligent coding assistant");
			info.Details.ParentModel.Should().BeNullOrEmpty();
			info.Details.Format.Should().Be("gguf");
			info.Details.Family.Should().Be("llama");
			info.Details.Families.Should().BeNull();
			info.Details.ParameterSize.Should().Be("7B");
			info.Details.QuantizationLevel.Should().Be("Q4_0");
			info.Info.Architecture.Should().Be("llama");
			info.Info.QuantizationVersion.Should().Be(2);
			info.Info.FileType.Should().Be(2);
			info.Info.ExtraInfo.Should().NotBeNullOrEmpty();
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

			var info = await _client.GenerateEmbeddings(new GenerateEmbeddingRequest { Model = "", Prompt = "" }, CancellationToken.None);

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

	public static async Task WriteChatStreamResponse(this StreamWriter writer, string content, ChatRole role)
	{
		var json = new { message = new { content, role }, role, done = false };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	public static async Task FinishChatStreamResponse(this StreamWriter writer, string content, ChatRole role)
	{
		var json = new { message = new { content, role = role.ToString() }, role = role.ToString(), done = true };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}
}
