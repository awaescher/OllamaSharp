using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OllamaSharp;
using OllamaSharp.Abstraction;
using OllamaSharp.Models.Chat;

namespace Tests;

public class AbstractionMapperTests
{
	public class ToChatCompletionMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_All_Properties()
		{
			var ollamaCreatedStamp = "2023-08-04T08:52:19.385406-07:00";

			var stream = new ChatDoneResponseStream
			{
				CreatedAt = ollamaCreatedStamp,
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

			var chatCompletion = AbstractionMapper.ToChatCompletion(new ChatRequest(), stream);

			chatCompletion.AdditionalProperties.Should().NotBeNull();
			chatCompletion.AdditionalProperties["eval_duration"].Should().Be(TimeSpan.FromSeconds(2.222222222));
			chatCompletion.AdditionalProperties["load_duration"].Should().Be(TimeSpan.FromSeconds(3.333333333));
			chatCompletion.AdditionalProperties["total_duration"].Should().Be(TimeSpan.FromSeconds(6.666666666));
			chatCompletion.AdditionalProperties["prompt_eval_duration"].Should().Be(TimeSpan.FromSeconds(5.555555555));

			chatCompletion.Choices.Should().HaveCount(1);
			chatCompletion.Choices.Single().Text.Should().Be("Hi.");

			chatCompletion.CompletionId.Should().Be(ollamaCreatedStamp);

			chatCompletion.CreatedAt.Should().Be(new DateTimeOffset(2023, 08, 04, 8, 52, 19, 385, 406, TimeSpan.FromHours(-7)));

			chatCompletion.FinishReason.Should().Be(ChatFinishReason.Stop);

			chatCompletion.Message.AuthorName.Should().BeNull();
			chatCompletion.Message.RawRepresentation.Should().BeNull();
			chatCompletion.Message.Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
			chatCompletion.Message.Text.Should().Be("Hi.");

			chatCompletion.Message.Contents.Should().BeEquivalentTo([new TextContent("Hi.")]);

			chatCompletion.ModelId.Should().Be("llama3.1:8b");

			chatCompletion.RawRepresentation.Should().Be(stream);

			chatCompletion.Usage.Should().NotBeNull();
			chatCompletion.Usage.InputTokenCount.Should().Be(411);
			chatCompletion.Usage.OutputTokenCount.Should().Be(111);
			chatCompletion.Usage.TotalTokenCount.Should().Be(111 + 411);

			// TODO extra tests for images and toolcalls
		}
	}

	public class ToChatRequestMethod : AbstractionMapperTests
	{
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
			//message.Images
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);
			//message.ToolCalls

			message = chatRequest.Messages.ElementAt(1);
			message.Content.Should().Be("What is 3 + 4?");
			//message.Images
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.User);
			//message.ToolCalls

			message = chatRequest.Messages.ElementAt(2);
			message.Content.Should().Be("3 + 4 is 7");
			//message.Images
			message.Role.Should().Be(OllamaSharp.Models.Chat.ChatRole.Assistant);
			//message.ToolCalls
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

		[Test]
		public void Maps_Tools()
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

			chatRequest.Tools.Should().BeEmpty();
		}
	}

	public class ToStreamingChatCompletionUpdateMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_All_Properties()
		{
			var date = new DateTime(2025, 12, 31, 22, 55, 56, 0, 0, DateTimeKind.Utc);

			var stream = new ChatResponseStream
			{
				CreatedAt = "",
				Model = "",
				Done = false,
				Message = new Message()
			};

			var chatRequest = AbstractionMapper.ToStreamingChatCompletionUpdate(stream);

			//chatRequest.CustomHeaders.Should().Be();
			//chatRequest.Format.Should().Be();
			//chatRequest.KeepAlive.Should().Be();
			//chatRequest.Messages.Should().Be();
			//chatRequest.Model.Should().Be();
			//chatRequest.Options.Should().Be();
			//chatRequest.Stream.Should().Be();
			//chatRequest.Template.Should().Be();
			//chatRequest.Tools.Should().Be();
		}
	}

	public class ToOllamaSharpToolMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_All_Properties()
		{
			var functionMeta = new AIFunctionMetadata("get_weather")
			{
				Description = "Gets the current weather for a certain location",
				AdditionalProperties = new Dictionary<string, object?>(),
				JsonSerializerOptions = null,
				Name = "",
				Parameters = null,
				ReturnParameter = null
			};

			var stream = new ChatResponseStream
			{
				CreatedAt = "",
				Model = "",
				Done = false,
				Message = new Message()
			};

			var chatRequest = AbstractionMapper.ToStreamingChatCompletionUpdate(stream);

			//chatRequest.CustomHeaders.Should().Be();
			//chatRequest.Format.Should().Be();
			//chatRequest.KeepAlive.Should().Be();
			//chatRequest.Messages.Should().Be();
			//chatRequest.Model.Should().Be();
			//chatRequest.Options.Should().Be();
			//chatRequest.Stream.Should().Be();
			//chatRequest.Template.Should().Be();
			//chatRequest.Tools.Should().Be();
		}
	}
}
