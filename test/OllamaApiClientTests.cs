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
using OllamaSharp.Models.Exceptions;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class OllamaApiClientTests
{
	private OllamaApiClient _client;
	private HttpResponseMessage? _response;
	private HttpRequestMessage? _request;
	private Dictionary<string, string>? _expectedRequestHeaders;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

		mockHandler
			.Protected()
			.Setup<Task<HttpResponseMessage?>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(r => ValidateExpectedRequestHeaders(r)),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(() => _response);

		var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://empty") };
		_client = new OllamaApiClient(httpClient);

		_client.DefaultRequestHeaders["default_header"] = "ok";
	}

	[SetUp]
	public void SetUp()
	{
		_expectedRequestHeaders = null;
	}

	/// <summary>
	/// Validates if the http request message has the same headers as defined in _expectedRequestHeaders.
	/// This method does nothing if _expectedRequestHeaders is null.
	/// </summary>
	private bool ValidateExpectedRequestHeaders(HttpRequestMessage request)
	{
		this._request = request;

		if (_expectedRequestHeaders is null)
			return true;

		if (_expectedRequestHeaders.Count != request.Headers.Count())
			throw new InvalidOperationException($"Expected {_expectedRequestHeaders.Count} request header(s) but found {request.Headers.Count()}!");

		foreach (var expectedHeader in _expectedRequestHeaders)
		{
			if (!request.Headers.Contains(expectedHeader.Key))
				throw new InvalidOperationException($"Expected request header '{expectedHeader.Key}' was not found!");

			var actualHeaderValue = request.Headers.GetValues(expectedHeader.Key).Single();
			if (!string.Equals(actualHeaderValue, expectedHeader.Value))
				throw new InvalidOperationException($"Request request header '{expectedHeader.Key}' has value '{actualHeaderValue}' while '{expectedHeader.Value}' was expected!");
		}

		return true;
	}

	public class CreateModelMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
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
			var modelStream = _client.CreateModel(new CreateModelRequest(), CancellationToken.None);

			await foreach (var status in modelStream)
				builder.Append(status?.Status);

			builder.ToString().Should().Be("Creating modelDownloading modelModel created");
		}

		/// <summary>
		/// Applies to all methods on the OllamaApiClient
		/// </summary>
		[Test, NonParallelizable]
		public async Task Sends_Default_Request_Headers()
		{
			_expectedRequestHeaders = new Dictionary<string, string>
			{
				["default_header"] = "ok" // set as default on the OllamaApiClient (see above)
			};

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(new MemoryStream())
			};

			var builder = new StringBuilder();
			await foreach (var status in _client.CreateModel(new CreateModelRequest(), CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.Should().Be(0); // assert anything, the test will fail if the expected headers are not available
		}

		/// <summary>
		/// Applies to all methods on the OllamaApiClient
		/// </summary>
		[Test, NonParallelizable]
		public async Task Sends_Custom_Request_Headers()
		{
			_expectedRequestHeaders = new Dictionary<string, string>
			{
				["default_header"] = "ok", // set as default on the OllamaApiClient (see above)
				["api_method"] = "create" // set as custom request header (see below)
			};

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(new MemoryStream())
			};

			var request = new CreateModelRequest();
			request.CustomHeaders["api_method"] = "create"; // set custom request headers

			var builder = new StringBuilder();
			await foreach (var status in _client.CreateModel(request, CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.Should().Be(0); // assert anything, the test will fail if the expected headers are not available
		}

		/// <summary>
		/// Applies to all methods on the OllamaApiClient
		/// </summary>
		[Test, NonParallelizable]
		public async Task Overwrites_Http_Headers()
		{
			_expectedRequestHeaders = new Dictionary<string, string>
			{
				["default_header"] = "overwritten" // default header value on the OllamaApiClient is 1, but it's overwritten below
			};

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(new MemoryStream())
			};

			var request = new CreateModelRequest();
			request.CustomHeaders["default_header"] = "overwritten";  // overwrites the default header defined on the OllamaApiClient

			var builder = new StringBuilder();
			await foreach (var status in _client.CreateModel(request, CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.Should().Be(0); // assert anything, the test will fail if the expected headers are not available
		}
	}

	public class GenerateMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
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
			await writer.FinishCompletionStreamResponse("blue.", context: [1, 2, 3]);
			stream.Seek(0, SeekOrigin.Begin);

			var context = await _client.Generate("prompt").StreamToEnd();

			context.Should().NotBeNull();
			context.Response.Should().Be("The sky is blue.");
			var expectation = new int[] { 1, 2, 3 };
			context.Context.Should().BeEquivalentTo(expectation);
		}
	}

	public class ChatMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
		public async Task Receives_Response_Message_With_Metadata()
		{
			var payload = """
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
				""".ReplaceLineEndings(""); // the JSON stream reader reads by line, so we need to make this one single line

			await using var stream = new MemoryStream();

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteAsync(payload);
			stream.Seek(0, SeekOrigin.Begin);

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			var chat = new ChatRequest
			{
				Model = "model",
				Messages = [
					new(ChatRole.User, "Why?"),
					new(ChatRole.Assistant, "Because!"),
					new(ChatRole.User, "And where?")]
			};

			var result = await _client.Chat(chat, CancellationToken.None).StreamToEnd();

			result.Should().NotBeNull();
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

			// Ensure that the request body does not contain the images, tools or tool_calls properties when not provided
			var requestBody = await _request.Content.ReadAsStringAsync();
			requestBody.Should().NotContain("tools");
			requestBody.Should().NotContain("tool_calls");
			requestBody.Should().NotContain("images");
		}

		[Test, NonParallelizable]
		public async Task Receives_Response_Message_With_ToolsCalls()
		{
			var payload = """
				{
				    "model": "llama3.1:latest",
				    "created_at": "2024-09-01T16:12:28.639564938Z",
				    "message": {
				        "role": "assistant",
				        "content": "",
				        "tool_calls": [
				            {
				                "function": {
				                    "name": "get_current_weather",
				                    "arguments": {
				                        "format": "celsius",
				                        "location": "Los Angeles, CA"
				                    }
				                }
				            }
				        ]
				    },
				    "done_reason": "stop",
				    "done": true,
				    "total_duration": 24808639002,
				    "load_duration": 5084890970,
				    "prompt_eval_count": 311,
				    "prompt_eval_duration": 15120086000,
				    "eval_count": 28,
				    "eval_duration": 4602334000
				}
				""".ReplaceLineEndings(""); // the JSON stream reader reads by line, so we need to make this one single line

			await using var stream = new MemoryStream();

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;
			await writer.WriteAsync(payload);
			stream.Seek(0, SeekOrigin.Begin);

			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			var chat = new ChatRequest
			{
				Model = "llama3.1:latest",
				Messages = [
					new(ChatRole.User, "How is the weather in LA?"),
				],
				Tools = [
					new Tool
					{
						Function = new Function
						{
							Description = "Get the current weather for a location",
							Name = "get_current_weather",
							Parameters = new Parameters
							{
								Properties = new Dictionary<string, Properties>
								{
									["location"] = new()
									{
										Type = "string",
										Description = "The location to get the weather for, e.g. San Francisco, CA"
									},
									["format"] = new()
									{
										Type = "string",
										Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'",
										Enum = ["celsius", "fahrenheit"]
									},
								},
								Required = ["location", "format"],
							}
						},
						Type = "function"
					}
				]
			};

			var result = await _client.Chat(chat, CancellationToken.None).StreamToEnd();

			result.Should().NotBeNull();
			result.Message.Role.Should().Be(ChatRole.Assistant);
			result.Done.Should().BeTrue();
			result.DoneReason.Should().Be("stop");

			result.Message.ToolCalls.Should().HaveCount(1);

			var toolsFunction = result.Message.ToolCalls!.ElementAt(0).Function;
			toolsFunction.Name.Should().Be("get_current_weather");
			toolsFunction.Arguments!.ElementAt(0).Key.Should().Be("format");
			toolsFunction.Arguments!.ElementAt(0).Value.Should().Be("celsius");

			toolsFunction.Arguments!.ElementAt(1).Key.Should().Be("location");
			toolsFunction.Arguments!.ElementAt(1).Value.Should().Be("Los Angeles, CA");
		}
	}

	public class StreamChatMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
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
				Messages =
				[
					new(ChatRole.User, "Why?"),
					new(ChatRole.Assistant, "Because!"),
					new(ChatRole.User, "And where?"),
				]
			};

			var chatStream = _client.Chat(chat, CancellationToken.None);

			var builder = new StringBuilder();
			var responses = new List<Message?>();

			await foreach (var response in chatStream)
			{
				builder.Append(response?.Message.Content);
				responses.Add(response?.Message);
			}

			builder.ToString().Should().BeEquivalentTo("Leave me alone.");

			responses.Should().HaveCount(3);
			responses[0]!.Role.Should().Be(ChatRole.Assistant);
			responses[1]!.Role.Should().Be(ChatRole.Assistant);
			responses[2]!.Role.Should().Be(ChatRole.Assistant);
		}

		[Test, NonParallelizable]
		public async Task Throws_Known_Exception_For_Models_That_Dont_Support_Tools()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("{ error: llama2 does not support tools }")
			};

			var act = () => _client.Chat(new ChatRequest(), CancellationToken.None).StreamToEnd();
			await act.Should().ThrowAsync<ModelDoesNotSupportToolsException>();
		}

		[Test, NonParallelizable]
		public async Task Throws_OllamaException_If_Parsing_Of_BadRequest_Errors_Fails()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("panic!")
			};

			var act = () => _client.Chat(new ChatRequest(), CancellationToken.None).StreamToEnd();
			await act.Should().ThrowAsync<OllamaException>();
		}
	}

	public class ListLocalModelsMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
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
			first.ModifiedAt.Date.Should().Be(new DateTime(2023, 10, 12, 0, 0, 0, DateTimeKind.Local));
			first.Size.Should().Be(3791811617);
			first.Digest.Should().StartWith("36893bf9bc7ff7ace5655");
		}
	}

	public class ShowMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n  \"license\": \"<contents of license block>\",\r\n  \"modelfile\": \"# Modelfile generated by \\\"ollama show\\\"\\n\\n\",\r\n  \"parameters\": \"stop                           [INST]\\nstop [/INST]\\nstop <<SYS>>\\nstop <</SYS>>\",\r\n  \"template\": \"[INST] {{ if and .First .System }}<<SYS>>{{ .System }}<</SYS>>\\n\\n{{ end }}{{ .Prompt }} [/INST] \"\r\n}")
			};

			var info = await _client.ShowModel("codellama:latest", CancellationToken.None);

			info.License.Should().Contain("contents of license block");
			info.Modelfile.Should().StartWith("# Modelfile generated");
			info.Parameters.Should().StartWith("stop");
			info.Template.Should().StartWith("[INST]");
		}

		[Test, NonParallelizable]
		public async Task Returns_Deserialized_Model_WithSystem()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\"modelfile\":\"# Modelfile generated by \\\"ollama show\\\"\\n# To build a new Modelfile based on this, replace FROM with:\\n# FROM magicoder:latest\\n\\nFROM C:\\\\Users\\\\jd\\\\.ollama\\\\models\\\\blobs\\\\sha256-4a501ed4ce55e5611922b3ee422501ff7cc773b472d196c3c416859b6d375273\\nTEMPLATE \\\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\\\"\\nSYSTEM You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\\nPARAMETER num_ctx 16384\\n\",\"parameters\":\"num_ctx                        16384\",\"template\":\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\",\"system\":\"You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":null,\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"},\"model_info\":{\"general.architecture\":\"llama\",\"general.file_type\":2,\"general.parameter_count\":8829407232,\"general.quantization_version\":2,\"llama.attention.head_count\":32,\"llama.attention.head_count_kv\":4,\"llama.attention.layer_norm_rms_epsilon\":0.000001,\"llama.block_count\":48,\"llama.context_length\":4096,\"llama.embedding_length\":4096,\"llama.feed_forward_length\":11008,\"llama.rope.dimension_count\":128,\"llama.rope.freq_base\":5000000,\"llama.vocab_size\":64000,\"tokenizer.ggml.add_bos_token\":false,\"tokenizer.ggml.add_eos_token\":false,\"tokenizer.ggml.bos_token_id\":1,\"tokenizer.ggml.eos_token_id\":2,\"tokenizer.ggml.model\":\"llama\",\"tokenizer.ggml.padding_token_id\":0,\"tokenizer.ggml.pre\":\"default\",\"tokenizer.ggml.scores\":[],\"tokenizer.ggml.token_type\":[],\"tokenizer.ggml.tokens\":[]},\"modified_at\":\"2024-05-14T23:33:07.4166573+08:00\"}")
			};

			var info = await _client.ShowModel("starcoder:latest", CancellationToken.None);

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
		[Test, NonParallelizable]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n  \"embeddings\": [[\r\n    0.5670403838157654, 0.009260174818336964, 0.23178744316101074, -0.2916173040866852, -0.8924556970596313  ]]\r\n}")
			};

			var info = await _client.Embed(new EmbedRequest { Model = "", Input = [""] }, CancellationToken.None);

			info.Embeddings[0].Should().HaveCount(5);
			info.Embeddings[0][0].Should().BeApproximately(0.567, precision: 0.01);
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

#pragma warning restore CS8602 // Dereference of a possibly null reference.