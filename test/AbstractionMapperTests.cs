using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.AI;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

public class AbstractionMapperTests
{
	public class ToOllamaSharpChatRequestMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Partial_Options_Class()
		{
			var messages = new List<ChatMessage>
			{
				new() { Role = Microsoft.Extensions.AI.ChatRole.Assistant, Text = "A" },
				new() { Role = Microsoft.Extensions.AI.ChatRole.User, Text = "B" },
			};

			var options = new ChatOptions { Temperature = 0.5f, /* other properties are left out */ };

			var request = AbstractionMapper.ToOllamaSharpChatRequest(messages, options, stream: true, JsonSerializerOptions.Default);

			request.Options.F16kv.Should().BeNull();
			request.Options.FrequencyPenalty.Should().BeNull();
			request.Options.LogitsAll.Should().BeNull();
			request.Options.LowVram.Should().BeNull();
			request.Options.MainGpu.Should().BeNull();
			request.Options.MinP.Should().BeNull();
			request.Options.MiroStat.Should().BeNull();
			request.Options.MiroStatEta.Should().BeNull();
			request.Options.MiroStatTau.Should().BeNull();
			request.Options.Numa.Should().BeNull();
			request.Options.NumBatch.Should().BeNull();
			request.Options.NumCtx.Should().BeNull();
			request.Options.NumGpu.Should().BeNull();
			request.Options.NumGqa.Should().BeNull();
			request.Options.NumKeep.Should().BeNull();
			request.Options.NumPredict.Should().BeNull();
			request.Options.NumThread.Should().BeNull();
			request.Options.PenalizeNewline.Should().BeNull();
			request.Options.PresencePenalty.Should().BeNull();
			request.Options.RepeatLastN.Should().BeNull();
			request.Options.RepeatPenalty.Should().BeNull();
			request.Options.Seed.Should().BeNull();
			request.Options.Stop.Should().BeNull();
			request.Options.Temperature.Should().Be(0.5f); // the only specified value
			request.Options.TfsZ.Should().BeNull();
			request.Options.TopK.Should().BeNull();
			request.Options.TopP.Should().BeNull();
			request.Options.TypicalP.Should().BeNull();
			request.Options.UseMlock.Should().BeNull();
			request.Options.UseMmap.Should().BeNull();
			request.Options.VocabOnly.Should().BeNull();
		}

		[Test]
		public void Maps_Messages()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [new TextContent("Hi there.")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Assistant,
					Text = "Hi there."
				},
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a2",
					Contents = [new TextContent("What is 3 + 4?")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User,
					Text = "What is 3 + 4?"
				},
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [new TextContent("3 + 4 is 7")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Assistant,
					Text = "3 + 4 is 7"
				},
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);

			chatRequest.Messages.Should().HaveCount(3);

			var message = chatRequest.Messages.ElementAt(0);
			message.Content.Should().Be("Hi there.");
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);

			message = chatRequest.Messages.ElementAt(1);
			message.Content.Should().Be("What is 3 + 4?");
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.User);

			message = chatRequest.Messages.ElementAt(2);
			message.Content.Should().Be("3 + 4 is 7");
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);
		}

		/// <summary>
		/// Ollama wants images without the metadata like "data:image/png;base64,"
		/// </summary>
		[Test]
		public void Maps_Base64_Images()
		{
			const string TRANSPARENT_PIXEL = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wcAAgEBAYkFNgAAAAAASUVORK5CYII=";
			const string TRANSPARENT_PIXEL_WITH_BASE64_META = "data:image/png;base64," + TRANSPARENT_PIXEL;

			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [
						new TextContent("Make me an image like this, but with beer."),
						new ImageContent(TRANSPARENT_PIXEL_WITH_BASE64_META)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				},
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a2",
					Contents = [
						new TextContent("Interesting idea, here we go:"),
						new ImageContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new ImageContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new ImageContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new ImageContent(TRANSPARENT_PIXEL_WITH_BASE64_META)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Assistant
				},
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);

			chatRequest.Messages.Should().HaveCount(2);

			var message = chatRequest.Messages.ElementAt(0);
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.User);
			message.Images.Single().Should().Be(TRANSPARENT_PIXEL); // <- WITHOUT BASE64_META

			message = chatRequest.Messages.ElementAt(1);
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);
			message.Images.Should().HaveCount(4);
		}

		[Test]
		public void Maps_Byte_Array_Images()
		{
			var bytes = System.Text.Encoding.ASCII.GetBytes("ABC");

			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [
						new TextContent("Make me an image like this, but with beer."),
						new ImageContent(bytes)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				}
			};

			var request = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);
			request.Messages.Single().Images.Single().Should().Be("QUJD");
		}

		/// <summary>
		/// Ollama only supports images provided as base64 string, that means with the image content
		/// Links to images are not supported
		/// </summary>
		[Test]
		public void Does_Not_Support_Image_Links()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [
						new TextContent("Make me an image like this, but with beer."),
						new ImageContent("https://unsplash.com/sunset.png")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				}
			};

			Action act = () =>
			{
				var request = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);
				request.Messages.Should().NotBeEmpty(); // access .Messages to invoke the evaluation of IEnumerable<Message>
			};

			act.Should().Throw<NotSupportedException>().Which.Message.Should().Contain("Images have to be provided as content");
		}

		[Test]
		public void Maps_Messages_With_Tools()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User,
					Text = "What's the weather in Honululu?"
				}
			};

			var options = new ChatOptions
			{
				Tools = [new WeatherFunction()]
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);

			var tool = chatRequest.Tools.Single();
			tool.Function.Description.Should().Be("Gets the current weather for a current location");
			tool.Function.Name.Should().Be("get_weather");
			tool.Function.Parameters.Properties.Should().HaveCount(2);
			tool.Function.Parameters.Properties["city"].Description.Should().Be("The city to get the weather for");
			tool.Function.Parameters.Properties["city"].Enum.Should().BeEmpty();
			tool.Function.Parameters.Properties["city"].Type.Should().Be("string");
			tool.Function.Parameters.Properties["unit"].Description.Should().Be("The unit to calculate the current temperature to");
			tool.Function.Parameters.Properties["unit"].Enum.Should().BeEmpty();
			tool.Function.Parameters.Properties["unit"].Type.Should().Be("string");
			tool.Function.Parameters.Required.Should().BeEquivalentTo("city");
			tool.Function.Parameters.Type.Should().Be("object");
			tool.Type.Should().Be("function");
		}

		[TestCaseSource(nameof(StopSequencesTestData))]
		public void Maps_Messages_With_IEnumerable_StopSequences(object? enumerable)
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User,
					Text = "What's the weather in Honululu?"
				}
			};

			var options = new ChatOptions()
			{
				AdditionalProperties = new AdditionalPropertiesDictionary() { ["stop"] = enumerable }
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);

			var stopSequences = chatRequest.Options.Stop;
			var typedEnumerable = (IEnumerable<string>?)enumerable;

			if (typedEnumerable == null)
			{
				stopSequences.Should().BeNull();
				return;
			}
			stopSequences.Should().HaveCount(typedEnumerable?.Count() ?? 0);
		}

		public static IEnumerable<TestCaseData> StopSequencesTestData
		{
			get
			{
				yield return new TestCaseData((object?)(JsonSerializer.Deserialize<JsonElement>("[\"stop1\", \"stop2\"]")).EnumerateArray().Select(e => e.GetString()));
				yield return new TestCaseData((object?)(IEnumerable<string>?)null);
				yield return new TestCaseData((object?)new List<string> { "stop1", "stop2", "stop3", "stop4" });
				yield return new TestCaseData((object?)new string[] { "stop1", "stop2", "stop3" });
				yield return new TestCaseData((object?)new HashSet<string> { "stop1", "stop2", });
				yield return new TestCaseData((object?)new Stack<string>(new[] { "stop1" }));
				yield return new TestCaseData((object?)new Queue<string>(new[] { "stop1" }));
			}
		}

		[Test]
		public void Maps_Messages_With_ToolResponse()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Tool,
					Text = "The weather in Honolulu is 25°C."
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, new(), stream: true, JsonSerializerOptions.Default);

			var tool = chatRequest.Messages.Single();
			tool.Content.Should().Contain("The weather in Honolulu is 25°C.");
			tool.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Tool);
		}

		[Test]
		public void Maps_Messages_With_MultipleToolResponse()
		{
			var aiChatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User,
					Contents = [
						new TextContent("I have found those 2 results"),
						new FunctionResultContent(
							callId: "123",
							name: "Function1",
							result: new { Temperature = 40 }),

						new FunctionResultContent(
							callId: "456",
							name: "Function2",
							result: new { Summary = "This is a tool result test" }
						),
					]
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(aiChatMessages, new(), stream: true, JsonSerializerOptions.Default);
			var chatMessages = chatRequest.Messages?.ToList();

			chatMessages.Should().HaveCount(3);

			var user = chatMessages[0];
			var tool1 = chatMessages[1];
			var tool2 = chatMessages[2];
			tool1.Content.Should().Contain("\"Temperature\":40");
			tool1.Content.Should().Contain("\"CallId\":\"123\"");
			tool1.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Tool);
			tool2.Content.Should().Contain("\"Summary\":\"This is a tool result test\"");
			tool2.Content.Should().Contain("\"CallId\":\"456\"");
			tool2.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Tool);
			user.Content.Should().Contain("I have found those 2 results");
			user.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.User);
		}

		[Test]
		public void Maps_Messages_WithoutContent_MultipleToolResponse()
		{
			var aiChatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User,
					Contents = [
						new FunctionResultContent(
							callId: "123",
							name: "Function1",
							result: new { Temperature = 40 }),

						new FunctionResultContent(
							callId: "456",
							name: "Function2",
							result: new { Summary = "This is a tool result test" }
						),
					]
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(aiChatMessages, new(), stream: true, JsonSerializerOptions.Default);
			var chatMessages = chatRequest.Messages?.ToList();

			chatMessages.Should().HaveCount(2);

			var tool1 = chatMessages[0];
			var tool2 = chatMessages[1];
			tool1.Content.Should().Contain("\"Temperature\":40");
			tool1.Content.Should().Contain("\"CallId\":\"123\"");
			tool1.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Tool);
			tool2.Content.Should().Contain("\"Summary\":\"This is a tool result test\"");
			tool2.Content.Should().Contain("\"CallId\":\"456\"");
			tool2.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Tool);
		}

		[Test]
		public void Maps_Options()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>();

			var options = new Microsoft.Extensions.AI.ChatOptions
			{
				FrequencyPenalty = 0.5f,
				MaxOutputTokens = 1000,
				ModelId = "llama3.1:405b",
				PresencePenalty = 0.3f,
				ResponseFormat = ChatResponseFormat.Json,
				Seed = 11,
				StopSequences = ["stop1", "stop2", "stop3"],
				Temperature = 0.1f,
				TopP = 10.1f
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);

			chatRequest.Format.Should().Be("json");
			chatRequest.Model.Should().Be("llama3.1:405b");
			chatRequest.Options.FrequencyPenalty.Should().Be(0.5f);
			chatRequest.Options.PresencePenalty.Should().Be(0.3f);
			chatRequest.Options.Stop.Should().BeEquivalentTo("stop1", "stop2", "stop3");
			chatRequest.Options.Temperature.Should().Be(0.1f);
			chatRequest.Options.TopP.Should().Be(10.1f);
			chatRequest.Options.Seed.Should().Be(11);
			chatRequest.Stream.Should().BeTrue();
			chatRequest.Template.Should().BeNull();

			// not defined in ChatOptions
			chatRequest.CustomHeaders.Should().BeEmpty();
			chatRequest.KeepAlive.Should().BeNull();
			chatRequest.Options.F16kv.Should().BeNull();
			chatRequest.Options.LogitsAll.Should().BeNull();
			chatRequest.Options.LowVram.Should().BeNull();
			chatRequest.Options.MainGpu.Should().BeNull();
			chatRequest.Options.MinP.Should().BeNull();
			chatRequest.Options.MiroStat.Should().BeNull();
			chatRequest.Options.MiroStatEta.Should().BeNull();
			chatRequest.Options.MiroStatTau.Should().BeNull();
			chatRequest.Options.Numa.Should().BeNull();
			chatRequest.Options.NumBatch.Should().BeNull();
			chatRequest.Options.NumCtx.Should().BeNull();
			chatRequest.Options.NumGpu.Should().BeNull();
			chatRequest.Options.NumGqa.Should().BeNull();
			chatRequest.Options.NumKeep.Should().BeNull();
			chatRequest.Options.NumPredict.Should().BeNull();
			chatRequest.Options.NumThread.Should().BeNull();
			chatRequest.Options.PenalizeNewline.Should().BeNull();
			chatRequest.Options.RepeatLastN.Should().BeNull();
			chatRequest.Options.RepeatPenalty.Should().BeNull();
			chatRequest.Options.TfsZ.Should().BeNull();
			chatRequest.Options.TopK.Should().BeNull();
			chatRequest.Options.TypicalP.Should().BeNull();
			chatRequest.Options.UseMlock.Should().BeNull();
			chatRequest.Options.UseMmap.Should().BeNull();
			chatRequest.Options.VocabOnly.Should().BeNull();
		}

		[Test]
		public void Maps_Ollama_Options()
		{
			var options = new Microsoft.Extensions.AI.ChatOptions()
				.AddOllamaOption(OllamaOption.F16kv, true)
				.AddOllamaOption(OllamaOption.FrequencyPenalty, 0.11f)
				.AddOllamaOption(OllamaOption.LogitsAll, false)
				.AddOllamaOption(OllamaOption.LowVram, true)
				.AddOllamaOption(OllamaOption.MainGpu, 1)
				.AddOllamaOption(OllamaOption.MinP, 0.22f)
				.AddOllamaOption(OllamaOption.MiroStat, 2)
				.AddOllamaOption(OllamaOption.MiroStatEta, 0.33f)
				.AddOllamaOption(OllamaOption.MiroStatTau, 0.44f)
				.AddOllamaOption(OllamaOption.Numa, false)
				.AddOllamaOption(OllamaOption.NumBatch, 3)
				.AddOllamaOption(OllamaOption.NumCtx, 4)
				.AddOllamaOption(OllamaOption.NumGpu, 5)
				.AddOllamaOption(OllamaOption.NumGqa, 6)
				.AddOllamaOption(OllamaOption.NumKeep, 7)
				.AddOllamaOption(OllamaOption.NumPredict, 8)
				.AddOllamaOption(OllamaOption.NumThread, 9)
				.AddOllamaOption(OllamaOption.PenalizeNewline, true)
				.AddOllamaOption(OllamaOption.PresencePenalty, 0.55f)
				.AddOllamaOption(OllamaOption.RepeatLastN, 10)
				.AddOllamaOption(OllamaOption.RepeatPenalty, 0.66f)
				.AddOllamaOption(OllamaOption.Seed, 11)
				.AddOllamaOption(OllamaOption.Stop, new string[] { "stop", "quit", "exit" })
				.AddOllamaOption(OllamaOption.Temperature, 0.77f)
				.AddOllamaOption(OllamaOption.TfsZ, 0.88f)
				.AddOllamaOption(OllamaOption.TopK, 12)
				.AddOllamaOption(OllamaOption.TopP, 0.99f)
				.AddOllamaOption(OllamaOption.TypicalP, 1.01f)
				.AddOllamaOption(OllamaOption.UseMlock, false)
				.AddOllamaOption(OllamaOption.UseMmap, true)
				.AddOllamaOption(OllamaOption.VocabOnly, false);

			var ollamaRequest = AbstractionMapper.ToOllamaSharpChatRequest([], options, stream: true, JsonSerializerOptions.Default);

			ollamaRequest.Options.F16kv.Should().Be(true);
			ollamaRequest.Options.FrequencyPenalty.Should().Be(0.11f);
			ollamaRequest.Options.LogitsAll.Should().Be(false);
			ollamaRequest.Options.LowVram.Should().Be(true);
			ollamaRequest.Options.MainGpu.Should().Be(1);
			ollamaRequest.Options.MinP.Should().Be(0.22f);
			ollamaRequest.Options.MiroStat.Should().Be(2);
			ollamaRequest.Options.MiroStatEta.Should().Be(0.33f);
			ollamaRequest.Options.MiroStatTau.Should().Be(0.44f);
			ollamaRequest.Options.Numa.Should().Be(false);
			ollamaRequest.Options.NumBatch.Should().Be(3);
			ollamaRequest.Options.NumCtx.Should().Be(4);
			ollamaRequest.Options.NumGpu.Should().Be(5);
			ollamaRequest.Options.NumGqa.Should().Be(6);
			ollamaRequest.Options.NumKeep.Should().Be(7);
			ollamaRequest.Options.NumPredict.Should().Be(8);
			ollamaRequest.Options.NumThread.Should().Be(9);
			ollamaRequest.Options.PenalizeNewline.Should().Be(true);
			ollamaRequest.Options.PresencePenalty.Should().Be(0.55f);
			ollamaRequest.Options.RepeatLastN.Should().Be(10);
			ollamaRequest.Options.RepeatPenalty.Should().Be(0.66f);
			ollamaRequest.Options.Seed.Should().Be(11);
			ollamaRequest.Options.Stop.Should().BeEquivalentTo("stop", "quit", "exit");
			ollamaRequest.Options.Temperature.Should().Be(0.77f);
			ollamaRequest.Options.TfsZ.Should().Be(0.88f);
			ollamaRequest.Options.TopK.Should().Be(12);
			ollamaRequest.Options.TopP.Should().Be(0.99f);
			ollamaRequest.Options.TypicalP.Should().Be(1.01f);
			ollamaRequest.Options.UseMlock.Should().Be(false);
			ollamaRequest.Options.UseMmap.Should().Be(true);
			ollamaRequest.Options.VocabOnly.Should().Be(false);
		}
	}

	public class ToChatCompletionMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Known_Properties()
		{
			var ollamaCreatedStamp = "2023-08-04T08:52:19.385406-07:00";

			var stream = new ChatDoneResponseStream
			{
				CreatedAtString = ollamaCreatedStamp,
				Done = true,
				DoneReason = "stop",
				EvalCount = 111,
				EvalDuration = 2222222222,
				LoadDuration = 3333333333,
				Message = new Message { Role = OllamaSharp.Models.Chat.ChatRole.Assistant, Content = "Hi." },
				Model = "llama3.1:8b",
				PromptEvalCount = 411,
				PromptEvalDuration = 5555555555,
				TotalDuration = 6666666666
			};

			var chatCompletion = AbstractionMapper.ToChatCompletion(stream, usedModel: null);

			chatCompletion.AdditionalProperties.Should().NotBeNull();
			chatCompletion.AdditionalProperties["eval_duration"].Should().Be(TimeSpan.FromSeconds(2.222222222));
			chatCompletion.AdditionalProperties["load_duration"].Should().Be(TimeSpan.FromSeconds(3.333333333));
			chatCompletion.AdditionalProperties["total_duration"].Should().Be(TimeSpan.FromSeconds(6.666666666));
			chatCompletion.AdditionalProperties["prompt_eval_duration"].Should().Be(TimeSpan.FromSeconds(5.555555555));
			chatCompletion.Choices.Should().HaveCount(1);
			chatCompletion.Choices.Single().Text.Should().Be("Hi.");
			chatCompletion.CompletionId.Should().Be(ollamaCreatedStamp);
			chatCompletion.CreatedAt.Should().Be(new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7)));
			chatCompletion.FinishReason.Should().Be(ChatFinishReason.Stop);
			chatCompletion.Message.AuthorName.Should().BeNull();
			chatCompletion.Message.RawRepresentation.Should().Be(stream.Message);
			chatCompletion.Message.Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
			chatCompletion.Message.Text.Should().Be("Hi.");
			chatCompletion.Message.Contents.Should().BeEquivalentTo([new TextContent("Hi.")]);
			chatCompletion.ModelId.Should().Be("llama3.1:8b");
			chatCompletion.RawRepresentation.Should().Be(stream);
			chatCompletion.Usage.Should().NotBeNull();
			chatCompletion.Usage.InputTokenCount.Should().Be(411);
			chatCompletion.Usage.OutputTokenCount.Should().Be(111);
			chatCompletion.Usage.TotalTokenCount.Should().Be(111 + 411);
		}
	}

	public class ToStreamingChatCompletionUpdateMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Known_Properties()
		{
			var ollamaCreated = new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7));
			var ollamaCreatedStamp = "2023-08-04T08:52:19.385406-07:00";

			var stream = new ChatResponseStream
			{
				CreatedAt = ollamaCreated,
				CreatedAtString = ollamaCreatedStamp,
				Done = true,
				Message = new Message { Role = OllamaSharp.Models.Chat.ChatRole.Assistant, Content = "Hi." },
				Model = "llama3.1:8b"
			};

			var streamingChatCompletion = AbstractionMapper.ToStreamingChatCompletionUpdate(stream);

			streamingChatCompletion.AdditionalProperties.Should().BeNull();
			streamingChatCompletion.AuthorName.Should().BeNull();
			streamingChatCompletion.ChoiceIndex.Should().Be(0);
			streamingChatCompletion.CompletionId.Should().Be(ollamaCreatedStamp);
			streamingChatCompletion.Contents.Should().BeEquivalentTo([new TextContent("Hi.")]);
			streamingChatCompletion.CreatedAt.Should().Be(new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7)));
			streamingChatCompletion.FinishReason.Should().Be(ChatFinishReason.Stop);
			streamingChatCompletion.RawRepresentation.Should().Be(stream);
			streamingChatCompletion.Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
			streamingChatCompletion.Text.Should().Be("Hi.");
		}
	}

	public class ToChatMessageMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_ToolCalls()
		{
			var message = new Message();
			message.Role = OllamaSharp.Models.Chat.ChatRole.Assistant;
			message.Content = "It seems the sun will be out all day.";
			message.ToolCalls =
			[
				new Message.ToolCall
				{
					Function = new Message.Function
					{
						Arguments = new Dictionary<string, object?>
						{
							["city"] = "Honululu",
							["unit"] = "celsius"
						},
						Name = "get_weather"
					}
				}
			];

			var chatMessage = AbstractionMapper.ToChatMessage(message);

			chatMessage.AdditionalProperties.Should().BeNull();
			chatMessage.AuthorName.Should().BeNull();
			chatMessage.Contents.Should().HaveCount(2);
			chatMessage.Contents.First().Should().BeOfType<TextContent>().Which.Text.Should().Be("It seems the sun will be out all day.");
			var toolCall = chatMessage.Contents.Last() as FunctionCallContent;
			toolCall.AdditionalProperties.Should().BeNull();
			toolCall.Arguments.Should().HaveCount(2);
			toolCall.Arguments["city"].Should().Be("Honululu");
			toolCall.Arguments["unit"].Should().Be("celsius");
			toolCall.CallId.Should().NotBeEmpty().And.HaveLength(8); // random guid
			toolCall.Exception.Should().BeNull();
			toolCall.Name.Should().Be("get_weather");
			toolCall.RawRepresentation.Should().Be(message.ToolCalls.Single());
			chatMessage.RawRepresentation.Should().Be(message);
			chatMessage.Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
			chatMessage.Text.Should().Be("It seems the sun will be out all day.");
		}
	}

	public class ToOllamaEmbedRequestMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Request()
		{
			var values = new string[] { "Teenage ", " Dirtbag." };

			var options = new EmbeddingGenerationOptions
			{
				Dimensions = 8,
				ModelId = "nomic_embed"
			};

			var request = AbstractionMapper.ToOllamaEmbedRequest(values, options);

			request.Input.Should().BeEquivalentTo("Teenage ", " Dirtbag.");
			request.KeepAlive.Should().BeNull();
			request.Model.Should().Be("nomic_embed");
			request.Options.Should().BeNull();
			request.Truncate.Should().BeNull();
		}

		[Test]
		public void Maps_KeepAlive_And_Truncate_From_AdditionalProperties()
		{
			var options = new EmbeddingGenerationOptions();
			options.AdditionalProperties = [];
			options.AdditionalProperties["keep_alive"] = 123456789;
			options.AdditionalProperties["truncate"] = true;

			var request = AbstractionMapper.ToOllamaEmbedRequest([], options);

			request.KeepAlive.Should().Be(123456789);
			request.Truncate.Should().BeTrue();
		}
	}

	public class ToGeneratedEmbeddingsMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Response()
		{
			var request = new EmbedRequest();
			var response = new EmbedResponse
			{
				Embeddings =
				[
					[0.101f, 0.102f, 0.103f],
					[0.201f, 0.202f, 0.203f]
				],
				LoadDuration = 1_100_000,
				PromptEvalCount = 18,
				TotalDuration = 3_200_000
			};

			var mappedResponse = AbstractionMapper.ToGeneratedEmbeddings(request, response, usedModel: "model");

			mappedResponse.AdditionalProperties.Should().NotBeNull();
			mappedResponse.Count.Should().Be(2);
			mappedResponse[0].ModelId.Should().Be("model");
			mappedResponse[0].Vector.ToArray().Should().BeEquivalentTo([0.101f, 0.102f, 0.103f]);
			mappedResponse[1].ModelId.Should().Be("model");
			mappedResponse[1].Vector.ToArray().Should().BeEquivalentTo([0.201f, 0.202f, 0.203f]);
			mappedResponse.Usage.InputTokenCount.Should().Be(18);
			mappedResponse.Usage.OutputTokenCount.Should().BeNull();
			mappedResponse.Usage.TotalTokenCount.Should().Be(18);
		}
	}
}