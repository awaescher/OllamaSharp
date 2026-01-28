using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.AI;

using Moq;
using Moq.Protected;

using NUnit.Framework;

using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;

using Shouldly;

using ChatRole = OllamaSharp.Models.Chat.ChatRole;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

/// <summary>
/// Contains integration tests for the <see cref="IOllamaApiClient"/> implementation.
/// </summary>
public class OllamaApiClientTests
{
	private IOllamaApiClient _client;
	private HttpResponseMessage? _response;
	private HttpRequestMessage? _request;
	private string? _requestContent;
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
		var client = new OllamaApiClient(httpClient);

		client.DefaultRequestHeaders["default_header"] = "ok";
		_client = client;
	}

	[SetUp]
	public void SetUp()
	{
		_expectedRequestHeaders = null;
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		(_client as IDisposable)?.Dispose();
	}

	/// <summary>
	/// Validates if the http request message has the same headers as defined in _expectedRequestHeaders.
	/// This method does nothing if _expectedRequestHeaders is null.
	/// </summary>
	private bool ValidateExpectedRequestHeaders(HttpRequestMessage request)
	{
		_request = request;
		_requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

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

	/// <summary>
	/// Contains tests for the PullModel method.
	/// </summary>
	public class PullModelMethod : OllamaApiClientTests
	{
		/// <summary>
		/// Simulates a failing PullModel request when behind a proxy like Zscaler.
		/// </summary>
		[Test, NonParallelizable]
		public async Task Streams_Status_Error()
		{
			var pullModelRequest = new PullModelRequest()
			{
				Model = "llama3.2:1b"
			};

			await using var stream = new MemoryStream();
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(stream)
			};

			await using var writer = new StreamWriter(stream, leaveOpen: true);
			writer.AutoFlush = true;

			var message = $"pull model manifest: Get '{pullModelRequest.Model}' : tls failed to verify certificate: x509: certificate signed by unknown authority";

			await writer.WriteLineAsync("{\"status\":\"pulling manifest\"}");
			await writer.WriteLineAsync($"{{\"error\":\"{message}\"}}");

			stream.Seek(0, SeekOrigin.Begin);

			var builder = new StringBuilder();

			try
			{
				var modelStream = _client.PullModelAsync(
					pullModelRequest, CancellationToken.None);
				await foreach (var status in modelStream)
					builder.Append(status?.Status);

				Assert.Fail("PullModelRequest didn't throw");
			}
			catch (ResponseError ex)
			{
				Assert.Pass(ex.Message);
			}
			finally
			{

			}
		}
	}

	/// <summary>
	/// Contains tests for the CreateModel method.
	/// </summary>
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
			var modelStream = _client.CreateModelAsync(new CreateModelRequest(), CancellationToken.None);

			await foreach (var status in modelStream)
				builder.Append(status?.Status);

			builder.ToString().ShouldBe("Creating modelDownloading modelModel created");
		}

		/// <summary>
		/// Verifies that default request headers are sent for all methods on the <see cref="IOllamaApiClient"/>.
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
			await foreach (var status in _client.CreateModelAsync(new CreateModelRequest(), CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.ShouldBe(0); // assert anything, the test will fail if the expected headers are not available
		}

		/// <summary>
		/// Verifies that custom request headers are sent for all methods on the <see cref="IOllamaApiClient"/>.
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
			await foreach (var status in _client.CreateModelAsync(request, CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.ShouldBe(0); // assert anything, the test will fail if the expected headers are not available
		}

		/// <summary>
		/// Verifies that custom request headers can overwrite default HTTP headers.
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
			await foreach (var status in _client.CreateModelAsync(request, CancellationToken.None))
				builder.Append(status?.Status);

			builder.Length.ShouldBe(0); // assert anything, the test will fail if the expected headers are not available
		}
	}

	/// <summary>
	/// Contains tests for the Generate method.
	/// </summary>
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

			var context = await _client.GenerateAsync("prompt").StreamToEndAsync();

			context.ShouldNotBeNull();
			context.Response.ShouldBe("The sky is blue.");
			context.Context.ShouldBe([1, 2, 3], ignoreOrder: true);
		}
	}

	/// <summary>
	/// Contains tests for the Complete method.
	/// </summary>
	public class CompleteMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
		public async Task Sends_Parameters_With_Request()
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

			List<Microsoft.Extensions.AI.ChatMessage> chatHistory = [];
			chatHistory.Add(new(Microsoft.Extensions.AI.ChatRole.User, "Why?"));
			chatHistory.Add(new(Microsoft.Extensions.AI.ChatRole.Assistant, "Because!"));
			chatHistory.Add(new(Microsoft.Extensions.AI.ChatRole.User, "And where?"));

			var chatClient = _client as Microsoft.Extensions.AI.IChatClient;

			var options = new ChatOptions
			{
				ModelId = "model",
				TopP = 100,
				TopK = 50,
				Temperature = 0.5f,
				FrequencyPenalty = 0.1f,
				PresencePenalty = 0.2f,
				StopSequences = ["stop me"],
			};

			await chatClient.GetResponseAsync(chatHistory, options, CancellationToken.None);

			_request.ShouldNotBeNull();
			_requestContent.ShouldNotBeNull();

			_requestContent.ShouldContain("Why?");
			_requestContent.ShouldContain("Because!");
			_requestContent.ShouldContain("And where?");
			_requestContent.ShouldContain("\"top_p\":100");
			_requestContent.ShouldContain("\"top_k\":50");
			_requestContent.ShouldContain("\"temperature\":0.5");
			_requestContent.ShouldContain("\"frequency_penalty\":0.1");
			_requestContent.ShouldContain("\"presence_penalty\":0.2");
			_requestContent.ShouldContain("\"stop\":[\"stop me\"]");

			// Ensure that the request does not contain any other properties when not provided.
			_requestContent.ShouldNotContain("tools");
			_requestContent.ShouldNotContain("tool_calls");
			_requestContent.ShouldNotContain("images");
		}
	}

	/// <summary>
	/// Contains tests for the Chat method.
	/// </summary>
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

			var result = await _client.ChatAsync(chat, CancellationToken.None).StreamToEndAsync();

			result.ShouldNotBeNull();
			result.Message.Role.ShouldBe(ChatRole.Assistant);
			result.Message.Content.ShouldBe("Test content.");
			result.Done.ShouldBeTrue();
			result.DoneReason.ShouldBe("stop");
			result.TotalDuration.ShouldBe(137729492272);
			result.LoadDuration.ShouldBe(133071702768);
			result.PromptEvalCount.ShouldBe(26);
			result.PromptEvalDuration.ShouldBe(35137000);
			result.EvalCount.ShouldBe(323);
			result.EvalDuration.ShouldBe(4575154000);
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
			                        "location": "Los Angeles, CA",
									"number": 42
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
				Stream = false,
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
							Properties = new Dictionary<string, OllamaSharp.Models.Chat.Property>
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
								["number"] = new()
								{
									Type = "integer",
									Description = "The number of the day to get the weather for, e.g. 42"
								}
							},
							Required = ["location", "format"],
						}
					},
					Type = "function"
				}
				]
			};

			var result = await _client.ChatAsync(chat, CancellationToken.None).StreamToEndAsync();

			result.ShouldNotBeNull();
			result.Message.Role.ShouldBe(ChatRole.Assistant);
			result.Done.ShouldBeTrue();
			result.DoneReason.ShouldBe("stop");

			result.Message.ToolCalls.Count().ShouldBe(1);

			var toolsFunction = result.Message.ToolCalls.ElementAt(0).Function;
			toolsFunction.Name.ShouldBe("get_current_weather");
			toolsFunction.Arguments.ElementAt(0).Key.ShouldBe("format");
			toolsFunction.Arguments.ElementAt(0).Value.ToString().ShouldBe("celsius");

			toolsFunction.Arguments.ElementAt(1).Key.ShouldBe("location");
			toolsFunction.Arguments.ElementAt(1).Value.ToString().ShouldBe("Los Angeles, CA");

			toolsFunction.Arguments.ElementAt(2).Key.ShouldBe("number");
			toolsFunction.Arguments.ElementAt(2).Value.ToString().ShouldBe("42");
		}
	}

	/// <summary>
	/// Contains tests for streaming chat responses.
	/// </summary>
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

			var chatStream = _client.ChatAsync(chat, CancellationToken.None);

			var builder = new StringBuilder();
			var responses = new List<Message?>();

			await foreach (var response in chatStream)
			{
				builder.Append(response?.Message.Content);
				responses.Add(response?.Message);
			}

			builder.ToString().ShouldBe("Leave me alone.");

			responses.Count.ShouldBe(3);
			responses[0].Role.ShouldBe(ChatRole.Assistant);
			responses[1].Role.ShouldBe(ChatRole.Assistant);
			responses[2].Role.ShouldBe(ChatRole.Assistant);
		}

		[Test, NonParallelizable]
		public async Task Throws_Known_Exception_For_Models_That_Dont_Support_Tools()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("{ error: llama2 does not support tools }")
			};

			var act = () => _client.ChatAsync(new ChatRequest() { Stream = false }, CancellationToken.None).StreamToEndAsync();
			await act.ShouldThrowAsync<ModelDoesNotSupportToolsException>();
		}

		[Test, NonParallelizable]
		public async Task Throws_OllamaException_If_Parsing_Of_BadRequest_Errors_Fails()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("panic!")
			};

			var act = () => _client.ChatAsync(new ChatRequest(), CancellationToken.None).StreamToEndAsync();
			await act.ShouldThrowAsync<OllamaException>();
		}
	}

	/// <summary>
	/// Contains tests for listing local models.
	/// </summary>
	public class ListLocalModelsMethod : OllamaApiClientTests
	{
		[Test, NonParallelizable]
		public async Task Returns_Deserialized_Models()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\r\n\"models\": [\r\n{\r\n\"name\": \"codellama:latest\",\r\n\"modified_at\": \"2023-10-12T14:17:04.967950259+02:00\",\r\n\"size\": 3791811617,\r\n\"digest\": \"36893bf9bc7ff7ace56557cd28784f35f834290c85d39115c6b91c00a031cfad\"\r\n},\r\n{\r\n\"name\": \"llama2:latest\",\r\n\"modified_at\": \"2023-10-02T14:10:14.78152065+02:00\",\r\n\"size\": 3791737662,\r\n\"digest\": \"d5611f7c428cf71fb05660257d18e043477f8b46cf561bf86940c687c1a59f70\"\r\n},\r\n{\r\n\"name\": \"mistral:latest\",\r\n\"modified_at\": \"2023-10-02T14:16:24.841447764+02:00\",\r\n\"size\": 4108916688,\r\n\"digest\": \"8aa307f73b2622af521e8f22d46e4b777123c4df91898dcb2e4079dc8fdf579e\"\r\n},\r\n{\r\n\"name\": \"vicuna:latest\",\r\n\"modified_at\": \"2023-10-06T09:44:16.936312659+02:00\",\r\n\"size\": 3825517709,\r\n\"digest\": \"675fa173a76abc48325d395854471961abf74b664d91e92ffb4fc03e0bde652b\"\r\n}\r\n]\r\n}")
			};

			var models = await _client.ListLocalModelsAsync(CancellationToken.None);
			models.Count().ShouldBe(4);

			var first = models.First();
			first.Name.ShouldBe("codellama:latest");
			first.ModifiedAt.Date.ShouldBe(new DateTime(2023, 10, 12, 0, 0, 0, DateTimeKind.Local));
			first.Size.ShouldBe(3791811617);
			first.Digest.ShouldStartWith("36893bf9bc7ff7ace5655");
		}
	}

	/// <summary>
	/// Contains tests for the Show method.
	/// </summary>
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

			var info = await _client.ShowModelAsync("codellama:latest", CancellationToken.None);

			info.License.ShouldContain("contents of license block");
			info.Modelfile.ShouldStartWith("# Modelfile generated");
			info.Parameters.ShouldStartWith("stop");
			info.Template.ShouldStartWith("[INST]");
		}

		[Test, NonParallelizable]
		public async Task Returns_Deserialized_Model_WithSystem()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\"modelfile\":\"# Modelfile generated by \\\"ollama show\\\"\\n# To build a new Modelfile based on this, replace FROM with:\\n# FROM magicoder:latest\\n\\nFROM C:\\\\Users\\\\jd\\\\.ollama\\\\models\\\\blobs\\\\sha256-4a501ed4ce55e5611922b3ee422501ff7cc773b472d196c3c416859b6d375273\\nTEMPLATE \\\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\\\"\\nSYSTEM You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\\nPARAMETER num_ctx 16384\\n\",\"parameters\":\"num_ctx                        16384\",\"template\":\"{{ .System }}\\n\\n@@ Instruction\\n{{ .Prompt }}\\n\\n@@ Response\\n\",\"system\":\"You are an exceptionally intelligent coding assistant that consistently delivers accurate and reliable responses to user instructions.\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":null,\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"},\"model_info\":{\"general.architecture\":\"llama\",\"general.file_type\":2,\"general.parameter_count\":8829407232,\"general.quantization_version\":2,\"llama.attention.head_count\":32,\"llama.attention.head_count_kv\":4,\"llama.attention.layer_norm_rms_epsilon\":0.000001,\"llama.block_count\":48,\"llama.context_length\":4096,\"llama.embedding_length\":4096,\"llama.feed_forward_length\":11008,\"llama.rope.dimension_count\":128,\"llama.rope.freq_base\":5000000,\"llama.vocab_size\":64000,\"tokenizer.ggml.add_bos_token\":false,\"tokenizer.ggml.add_eos_token\":false,\"tokenizer.ggml.bos_token_id\":1,\"tokenizer.ggml.eos_token_id\":2,\"tokenizer.ggml.model\":\"llama\",\"tokenizer.ggml.padding_token_id\":0,\"tokenizer.ggml.pre\":\"default\",\"tokenizer.ggml.scores\":[],\"tokenizer.ggml.token_type\":[],\"tokenizer.ggml.tokens\":[]},\"modified_at\":\"2024-05-14T23:33:07.4166573+08:00\"}")
			};

			var info = await _client.ShowModelAsync("starcoder:latest", CancellationToken.None);

			info.License.ShouldBeNullOrEmpty();
			info.Modelfile.ShouldStartWith("# Modelfile generated");
			info.Parameters.ShouldStartWith("num_ctx");
			info.Template.ShouldStartWith("{{ .System }}");
			info.System.ShouldStartWith("You are an exceptionally intelligent coding assistant");
			info.Details.ParentModel.ShouldBeNullOrEmpty();
			info.Details.Format.ShouldBe("gguf");
			info.Details.Family.ShouldBe("llama");
			info.Details.Families.ShouldBeNull();
			info.Details.ParameterSize.ShouldBe("7B");
			info.Details.QuantizationLevel.ShouldBe("Q4_0");
			info.Info.Architecture.ShouldBe("llama");
			info.Info.QuantizationVersion.ShouldBe(2);
			info.Info.FileType.ShouldBe(2);
			info.Info.ExtraInfo.ShouldNotBeEmpty();
		}
	}

	/// <summary>
	/// Contains tests for the GenerateEmbeddings method.
	/// </summary>
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

			var info = await _client.EmbedAsync(new EmbedRequest { Model = "", Input = [""] }, CancellationToken.None);

			info.Embeddings[0].Length.ShouldBe(5);
			info.Embeddings[0][0].ShouldBe(0.567f, tolerance: 0.01f);
		}
	}

	/// <summary>
	/// Contains tests for the GetVersion method.
	/// </summary>
	public class GetVersionMethod : OllamaApiClientTests
	{
		[Test]
		public async Task Returns_Empty_String_For_Empty_Version()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{ }")
			};

			var version = await _client.GetVersionAsync(CancellationToken.None);

			version.ShouldBe("");
		}

		[Test]
		public async Task Returns_Simple_Version_Number()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\"version\":\"0.6.8\"}")
			};

			var version = await _client.GetVersionAsync(CancellationToken.None);

			version.ShouldBe("0.6.8");
		}

		[Test]
		public async Task Supports_Alphanumerical_Versions()
		{
			_response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{\"version\":\"0.6.8-rc1\"}")
			};

			var version = await _client.GetVersionAsync(CancellationToken.None);

			version.ShouldBe("0.6.8-rc1");
		}
	}
}

/// <summary>
/// Provides extension methods for writing simulated Ollama streaming responses in tests.
/// </summary>
public static class WriterExtensions
{
	/// <summary>
	/// Writes a completion stream response line with the specified partial <paramref name="response"/>.
	/// The generated JSON contains <c>done = false</c>.
	/// </summary>
	/// <param name="writer">The writer to which the JSON line is written.</param>
	/// <param name="response">The partial response text.</param>
	public static async Task WriteCompletionStreamResponse(this StreamWriter writer, string response)
	{
		var json = new { response, done = false };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	/// <summary>
	/// Writes the final completion stream response line with the specified <paramref name="response"/> and <paramref name="context"/>.
	/// The generated JSON contains <c>done = true</c>.
	/// </summary>
	/// <param name="writer">The writer to which the JSON line is written.</param>
	/// <param name="response">The final response text.</param>
	/// <param name="context">An array of integer context identifiers.</param>
	public static async Task FinishCompletionStreamResponse(this StreamWriter writer, string response, int[] context)
	{
		var json = new { response, done = true, context };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	/// <summary>
	/// Writes a chat stream response line with the specified <paramref name="content"/> and <paramref name="role"/>.
	/// The generated JSON contains <c>done = false</c>.
	/// </summary>
	/// <param name="writer">The writer to which the JSON line is written.</param>
	/// <param name="content">The partial chat message content.</param>
	/// <param name="role">The role of the message sender.</param>
	public static async Task WriteChatStreamResponse(this StreamWriter writer, string content, ChatRole role)
	{
		var json = new { message = new { content, role }, role, done = false };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}

	/// <summary>
	/// Writes the final chat stream response line with the specified <paramref name="content"/> and <paramref name="role"/>.
	/// The generated JSON contains <c>done = true</c>.
	/// </summary>
	/// <param name="writer">The writer to which the JSON line is written.</param>
	/// <param name="content">The final chat message content.</param>
	/// <param name="role">The role of the message sender.</param>
	public static async Task FinishChatStreamResponse(this StreamWriter writer, string content, ChatRole role)
	{
		var json = new { message = new { content, role = role.ToString() }, role = role.ToString(), done = true };
		await writer.WriteLineAsync(JsonSerializer.Serialize(json));
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.