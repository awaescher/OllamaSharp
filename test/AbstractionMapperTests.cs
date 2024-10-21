using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models.Chat;

namespace Tests;

public partial class AbstractionMapperTests
{
	public partial class ToOllamaSharpChatRequestMethod : AbstractionMapperTests
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

			var request = AbstractionMapper.ToOllamaSharpChatRequest(Mock.Of<IOllamaApiClient>(), messages, options, stream: true);

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

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(Mock.Of<IOllamaApiClient>(), chatMessages, null, stream: true);

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

		[Test]
		public void Maps_Messages_With_Images()
		{
			const string TRANSPARENT_PIXEL = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/wcAAgEBAYkFNgAAAAAASUVORK5CYII=";

			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [
						new TextContent("Make me an image like this, but with beer."),
						new ImageContent(TRANSPARENT_PIXEL)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				},
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a2",
					Contents = [
						new TextContent("Interesting idea, here we go:"),
						new ImageContent(TRANSPARENT_PIXEL),
						new ImageContent(TRANSPARENT_PIXEL),
						new ImageContent(TRANSPARENT_PIXEL),
						new ImageContent(TRANSPARENT_PIXEL)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Assistant
				},
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(Mock.Of<IOllamaApiClient>(), chatMessages, null, stream: true);

			chatRequest.Messages.Should().HaveCount(2);

			var message = chatRequest.Messages.ElementAt(0);
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.User);
			message.Images.Single().Should().Be(TRANSPARENT_PIXEL);

			message = chatRequest.Messages.ElementAt(1);
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);
			message.Images.Should().HaveCount(4);
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

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(Mock.Of<IOllamaApiClient>(), chatMessages, options, stream: true);

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
			tool.Function.Parameters.Required.Should().BeEquivalentTo(["city"]);
			tool.Function.Parameters.Type.Should().Be("object");
			tool.Type.Should().Be("function");
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
				StopSequences = ["stop1", "stop2", "stop3"],
				Temperature = 0.1f,
				TopP = 10.1f
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(Mock.Of<IOllamaApiClient>(), chatMessages, options, stream: true);

			chatRequest.Format.Should().Be("json");
			chatRequest.Model.Should().Be("llama3.1:405b");
			chatRequest.Options.FrequencyPenalty.Should().Be(0.5f);
			chatRequest.Options.PresencePenalty.Should().Be(0.3f);
			chatRequest.Options.Stop.Should().BeEquivalentTo("stop1", "stop2", "stop3");
			chatRequest.Options.Temperature.Should().Be(0.1f);
			chatRequest.Options.TopP.Should().Be(10.1f);
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

			var chatCompletion = AbstractionMapper.ToChatCompletion(stream);

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
			toolCall.ModelId.Should().BeNull();
			toolCall.Name.Should().Be("get_weather");
			toolCall.RawRepresentation.Should().Be(message.ToolCalls.Single());
			chatMessage.RawRepresentation.Should().Be(message);
			chatMessage.Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
			chatMessage.Text.Should().Be("It seems the sun will be out all day.");
		}
	}
}
