using System.Text.Json;
using Microsoft.Extensions.AI;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Constants;
using OllamaSharp.MicrosoftAi;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Shouldly;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

public class AbstractionMapperTests
{
	public class ToOllamaSharpChatRequestMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Partial_Options_Class()
		{
			var messages = new List<ChatMessage>
			{
				new(Microsoft.Extensions.AI.ChatRole.Assistant, "A"),
				new(Microsoft.Extensions.AI.ChatRole.User, "B"),
			};

			var options = new ChatOptions { Temperature = 0.5f, /* other properties are left out */ };

			var request = AbstractionMapper.ToOllamaSharpChatRequest(messages, options, stream: true, JsonSerializerOptions.Default);

			request.Options.F16kv.ShouldBeNull();
			request.Options.FrequencyPenalty.ShouldBeNull();
			request.Options.LogitsAll.ShouldBeNull();
			request.Options.LowVram.ShouldBeNull();
			request.Options.MainGpu.ShouldBeNull();
			request.Options.MinP.ShouldBeNull();
			request.Options.MiroStat.ShouldBeNull();
			request.Options.MiroStatEta.ShouldBeNull();
			request.Options.MiroStatTau.ShouldBeNull();
			request.Options.Numa.ShouldBeNull();
			request.Options.NumBatch.ShouldBeNull();
			request.Options.NumCtx.ShouldBeNull();
			request.Options.NumGpu.ShouldBeNull();
			request.Options.NumGqa.ShouldBeNull();
			request.Options.NumKeep.ShouldBeNull();
			request.Options.NumPredict.ShouldBeNull();
			request.Options.NumThread.ShouldBeNull();
			request.Options.PenalizeNewline.ShouldBeNull();
			request.Options.PresencePenalty.ShouldBeNull();
			request.Options.RepeatLastN.ShouldBeNull();
			request.Options.RepeatPenalty.ShouldBeNull();
			request.Options.Seed.ShouldBeNull();
			request.Options.Stop.ShouldBeNull();
			request.Options.Temperature.ShouldBe(0.5f); // the only specified value
			request.Options.TfsZ.ShouldBeNull();
			request.Options.TopK.ShouldBeNull();
			request.Options.TopP.ShouldBeNull();
			request.Options.TypicalP.ShouldBeNull();
			request.Options.UseMlock.ShouldBeNull();
			request.Options.UseMmap.ShouldBeNull();
			request.Options.VocabOnly.ShouldBeNull();
		}

		[Test]
		public void Maps_Messages()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new(Microsoft.Extensions.AI.ChatRole.Assistant, "Hi there.")
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [new TextContent("Hi there.")],
					RawRepresentation = null,
				},
				new(Microsoft.Extensions.AI.ChatRole.User, "What is 3 + 4?")
				{
					AdditionalProperties = [],
					AuthorName = "a2",
					Contents = [new TextContent("What is 3 + 4?")],
					RawRepresentation = null,
				},
				new(Microsoft.Extensions.AI.ChatRole.Assistant, "3 + 4 is 7")
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [new TextContent("3 + 4 is 7")],
					RawRepresentation = null,
				},
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);

			chatRequest.Messages.Count().ShouldBe(3);

			var message = chatRequest.Messages.ElementAt(0);
			message.Content.ShouldBe("Hi there.");
			message.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Assistant);

			message = chatRequest.Messages.ElementAt(1);
			message.Content.ShouldBe("What is 3 + 4?");
			message.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.User);

			message = chatRequest.Messages.ElementAt(2);
			message.Content.ShouldBe("3 + 4 is 7");
			message.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Assistant);
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
						new DataContent(TRANSPARENT_PIXEL_WITH_BASE64_META)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				},
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a2",
					Contents = [
						new TextContent("Interesting idea, here we go:"),
						new DataContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new DataContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new DataContent(TRANSPARENT_PIXEL_WITH_BASE64_META),
						new DataContent(TRANSPARENT_PIXEL_WITH_BASE64_META)],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.Assistant
				},
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);

			chatRequest.Messages.Count().ShouldBe(2);

			var message = chatRequest.Messages.ElementAt(0);
			message.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.User);
			message.Images.Single().ShouldBe(TRANSPARENT_PIXEL); // <- WITHOUT BASE64_META

			message = chatRequest.Messages.ElementAt(1);
			message.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Assistant);
			message.Images.Count().ShouldBe(4);
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
						new DataContent(bytes, "image/png")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				}
			};

			var request = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);
			request.Messages.Single().Images.Single().ShouldBe("QUJD");
		}

		/// <summary>
		/// Ollama only supports images provided as base64 string, that means with the image content
		/// Links to images are not supported
		/// </summary>
		[Test]
		public void Ignores_UriContent()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new()
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					Contents = [
						new TextContent("Make me an image like this, but with beer."),
						new UriContent("https://unsplash.com/sunset.png", "image/png")],
					RawRepresentation = null,
					Role = Microsoft.Extensions.AI.ChatRole.User
				}
			};

			Action act = () =>
			{
				var request = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, null, stream: true, JsonSerializerOptions.Default);
				request.Messages.ShouldNotBeEmpty(); // access .Messages to invoke the evaluation of IEnumerable<Message>
			};

			act.ShouldNotThrow();
		}

		[Test]
		public void Maps_Messages_With_Tools()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new(Microsoft.Extensions.AI.ChatRole.User, "What's the weather in Honululu?")
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
				}
			};

			var options = new ChatOptions
			{
				Tools = [AIFunctionFactory.Create((
					[System.ComponentModel.Description("The city to get the weather for")] string city,
					[System.ComponentModel.Description("The unit to calculate the current temperature to")] string unit = "celsius") => "sunny",
					"get_weather", "Gets the current weather for a current location")],
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);

			var tool = (Tool)chatRequest.Tools.Single();
			tool.Function.Description.ShouldBe("Gets the current weather for a current location");
			tool.Function.Name.ShouldBe("get_weather");
			tool.Function.Parameters.Properties.Count.ShouldBe(2);
			tool.Function.Parameters.Properties["city"].Description.ShouldBe("The city to get the weather for");
			tool.Function.Parameters.Properties["city"].Enum.ShouldBeNull();
			tool.Function.Parameters.Properties["city"].Type.ShouldBe("string");
			tool.Function.Parameters.Properties["unit"].Description.ShouldBe("The unit to calculate the current temperature to");
			tool.Function.Parameters.Properties["unit"].Enum.ShouldBeNull();
			tool.Function.Parameters.Properties["unit"].Type.ShouldBe("string");
			tool.Function.Parameters.Required.ShouldBe(["city"], ignoreOrder: true);
			tool.Function.Parameters.Type.ShouldBe("object");
			tool.Type.ShouldBe("function");
		}

		[Test]
		public void Maps_KeepAliveAll_From_AdditionalProperties()
		{
			var options = new ChatOptions
			{
				AdditionalProperties = []
			};
			options.AdditionalProperties["keep_alive"] = "60m";

			var request = AbstractionMapper.ToOllamaSharpChatRequest([], options, false, JsonSerializerOptions.Default);

			request.KeepAlive.ShouldBe("60m");
		}

		[Test]
		public void Maps_All_Options_With_AdditionalProperties()
		{
			// Arrange
			List<ChatMessage> chatMessages = [];

			var options = new ChatOptions
			{
				AdditionalProperties = new AdditionalPropertiesDictionary()
				{
					// Boolean options
					[OllamaOption.F16kv.Name] = true,
					[OllamaOption.LogitsAll.Name] = true,
					[OllamaOption.LowVram.Name] = true,
					[OllamaOption.Numa.Name] = true,
					[OllamaOption.PenalizeNewline.Name] = true,
					[OllamaOption.UseMlock.Name] = true,
					[OllamaOption.UseMmap.Name] = true,
					[OllamaOption.VocabOnly.Name] = true,

					// Float options
					[OllamaOption.FrequencyPenalty.Name] = 0.5f,
					[OllamaOption.MinP.Name] = 0.1f,
					[OllamaOption.MiroStatEta.Name] = 0.1f,
					[OllamaOption.MiroStatTau.Name] = 0.2f,
					[OllamaOption.PresencePenalty.Name] = 0.3f,
					[OllamaOption.RepeatPenalty.Name] = 0.4f,
					[OllamaOption.Temperature.Name] = 0.7f,
					[OllamaOption.TfsZ.Name] = 0.8f,
					[OllamaOption.TopP.Name] = 0.9f,
					[OllamaOption.TypicalP.Name] = 0.95f,

					// Integer options
					[OllamaOption.MainGpu.Name] = 0,
					[OllamaOption.MiroStat.Name] = 1,
					[OllamaOption.NumBatch.Name] = 512,
					[OllamaOption.NumCtx.Name] = 4096,
					[OllamaOption.NumGpu.Name] = 1,
					[OllamaOption.NumGqa.Name] = 8,
					[OllamaOption.NumKeep.Name] = 64,
					[OllamaOption.NumPredict.Name] = 1024,
					[OllamaOption.MaxOutputTokens.Name] = 2048,
					[OllamaOption.NumThread.Name] = 8,
					[OllamaOption.RepeatLastN.Name] = 64,
					[OllamaOption.Seed.Name] = 42,
					[OllamaOption.TopK.Name] = 40,

					// String array options
					[OllamaOption.Stop.Name] = new[] { "stop1", "stop2" }
				}
			};

			// Act
			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);

			// Assert
			chatRequest.Options.ShouldNotBeNull();

			// Boolean assertions
			chatRequest.Options!.F16kv.ShouldBe(true);
			chatRequest.Options!.LogitsAll.ShouldBe(true);
			chatRequest.Options!.LowVram.ShouldBe(true);
			chatRequest.Options!.Numa.ShouldBe(true);
			chatRequest.Options!.PenalizeNewline.ShouldBe(true);
			chatRequest.Options!.UseMlock.ShouldBe(true);
			chatRequest.Options!.UseMmap.ShouldBe(true);
			chatRequest.Options!.VocabOnly.ShouldBe(true);

			// Float assertions
			chatRequest.Options!.FrequencyPenalty.ShouldBe(0.5f);
			chatRequest.Options!.MinP.ShouldBe(0.1f);
			chatRequest.Options!.MiroStatEta.ShouldBe(0.1f);
			chatRequest.Options!.MiroStatTau.ShouldBe(0.2f);
			chatRequest.Options!.PresencePenalty.ShouldBe(0.3f);
			chatRequest.Options!.RepeatPenalty.ShouldBe(0.4f);
			chatRequest.Options!.Temperature.ShouldBe(0.7f);
			chatRequest.Options!.TfsZ.ShouldBe(0.8f);
			chatRequest.Options!.TopP.ShouldBe(0.9f);
			chatRequest.Options!.TypicalP.ShouldBe(0.95f);

			// Integer assertions
			chatRequest.Options!.MainGpu.ShouldBe(0);
			chatRequest.Options!.MiroStat.ShouldBe(1);
			chatRequest.Options!.NumBatch.ShouldBe(512);
			chatRequest.Options!.NumCtx.ShouldBe(4096);
			chatRequest.Options!.NumGpu.ShouldBe(1);
			chatRequest.Options!.NumGqa.ShouldBe(8);
			chatRequest.Options!.NumKeep.ShouldBe(64);
			chatRequest.Options!.NumPredict.ShouldBe(2048);
			chatRequest.Options!.NumThread.ShouldBe(8);
			chatRequest.Options!.RepeatLastN.ShouldBe(64);
			chatRequest.Options!.Seed.ShouldBe(42);
			chatRequest.Options!.TopK.ShouldBe(40);

			// String array assertions
			chatRequest.Options!.Stop.ShouldNotBeNull();
			chatRequest.Options!.Stop.ShouldBe(["stop1", "stop2"], ignoreOrder: true);
		}

		[TestCaseSource(nameof(StopSequencesTestData))]
		public void Maps_Messages_With_IEnumerable_StopSequences(object? enumerable)
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new(Microsoft.Extensions.AI.ChatRole.User, "What's the weather in Honululu?")
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
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
				stopSequences.ShouldBeNull();
				return;
			}
			stopSequences.Length.ShouldBe(typedEnumerable?.Count() ?? 0);
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
				yield return new TestCaseData((object?)new Stack<string>(["stop1"]));
				yield return new TestCaseData((object?)new Queue<string>(["stop1"]));
			}
		}

		[Test]
		public void Maps_Messages_With_ToolResponse()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>
			{
				new(Microsoft.Extensions.AI.ChatRole.Tool, "The weather in Honolulu is 25°C.")
				{
					AdditionalProperties = [],
					AuthorName = "a1",
					RawRepresentation = null,
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, new(), stream: true, JsonSerializerOptions.Default);

			var tool = chatRequest.Messages.Single();
			tool.Content.ShouldContain("The weather in Honolulu is 25°C.");
			tool.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Tool);
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
							result: new { Temperature = 40 }),

						new FunctionResultContent(
							callId: "456",
							result: new { Summary = "This is a tool result test" }
						),
					]
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(aiChatMessages, new(), stream: true, JsonSerializerOptions.Default);
			var chatMessages = chatRequest.Messages?.ToList();

			chatMessages.Count.ShouldBe(3);

			var user = chatMessages[0];
			var tool1 = chatMessages[1];
			var tool2 = chatMessages[2];
			tool1.Content.ShouldContain("\"Temperature\":40");
			tool1.Content.ShouldContain("\"CallId\":\"123\"");
			tool1.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Tool);
			tool2.Content.ShouldContain("\"Summary\":\"This is a tool result test\"");
			tool2.Content.ShouldContain("\"CallId\":\"456\"");
			tool2.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Tool);
			user.Content.ShouldContain("I have found those 2 results");
			user.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.User);
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
							result: new { Temperature = 40 }),

						new FunctionResultContent(
							callId: "456",
							result: new { Summary = "This is a tool result test" }
						),
					]
				}
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(aiChatMessages, new(), stream: true, JsonSerializerOptions.Default);
			var chatMessages = chatRequest.Messages?.ToList();

			chatMessages.Count.ShouldBe(2);

			var tool1 = chatMessages[0];
			var tool2 = chatMessages[1];
			tool1.Content.ShouldContain("\"Temperature\":40");
			tool1.Content.ShouldContain("\"CallId\":\"123\"");
			tool1.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Tool);
			tool2.Content.ShouldContain("\"Summary\":\"This is a tool result test\"");
			tool2.Content.ShouldContain("\"CallId\":\"456\"");
			tool2.Role.ShouldBe(OllamaSharp.Models.Chat.ChatRole.Tool);
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

			chatRequest.Format.ShouldBe("json");
			chatRequest.Model.ShouldBe("llama3.1:405b");
			chatRequest.Options.FrequencyPenalty.ShouldBe(0.5f);
			chatRequest.Options.PresencePenalty.ShouldBe(0.3f);
			chatRequest.Options.Stop.ShouldBe(["stop1", "stop2", "stop3"], ignoreOrder: true);
			chatRequest.Options.Temperature.ShouldBe(0.1f);
			chatRequest.Options.TopP.ShouldBe(10.1f);
			chatRequest.Options.Seed.ShouldBe(11);
			chatRequest.Stream.ShouldBeTrue();
			chatRequest.Template.ShouldBeNull();
			chatRequest.Options.NumPredict.ShouldBe(1000);

			// not defined in ChatOptions
			chatRequest.CustomHeaders.ShouldBeEmpty();
			chatRequest.KeepAlive.ShouldBeNull();
			chatRequest.Options.F16kv.ShouldBeNull();
			chatRequest.Options.LogitsAll.ShouldBeNull();
			chatRequest.Options.LowVram.ShouldBeNull();
			chatRequest.Options.MainGpu.ShouldBeNull();
			chatRequest.Options.MinP.ShouldBeNull();
			chatRequest.Options.MiroStat.ShouldBeNull();
			chatRequest.Options.MiroStatEta.ShouldBeNull();
			chatRequest.Options.MiroStatTau.ShouldBeNull();
			chatRequest.Options.Numa.ShouldBeNull();
			chatRequest.Options.NumBatch.ShouldBeNull();
			chatRequest.Options.NumCtx.ShouldBeNull();
			chatRequest.Options.NumGpu.ShouldBeNull();
			chatRequest.Options.NumGqa.ShouldBeNull();
			chatRequest.Options.NumKeep.ShouldBeNull();
			chatRequest.Options.NumThread.ShouldBeNull();
			chatRequest.Options.PenalizeNewline.ShouldBeNull();
			chatRequest.Options.RepeatLastN.ShouldBeNull();
			chatRequest.Options.RepeatPenalty.ShouldBeNull();
			chatRequest.Options.TfsZ.ShouldBeNull();
			chatRequest.Options.TopK.ShouldBeNull();
			chatRequest.Options.TypicalP.ShouldBeNull();
			chatRequest.Options.UseMlock.ShouldBeNull();
			chatRequest.Options.UseMmap.ShouldBeNull();
			chatRequest.Options.VocabOnly.ShouldBeNull();
		}

		[Test]
		public void Maps_JsonWithoutSchema()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>();

			var options = new Microsoft.Extensions.AI.ChatOptions
			{
				ResponseFormat = ChatResponseFormat.Json
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);
			chatRequest.Format.ShouldBe("json");
		}

		[Test]
		public void Maps_JsonWithSchema()
		{
			var chatMessages = new List<Microsoft.Extensions.AI.ChatMessage>();
			var schemaElement = AIJsonUtilities.CreateJsonSchema(type: typeof(Sword));

			var options = new Microsoft.Extensions.AI.ChatOptions
			{
				ResponseFormat = ChatResponseFormat.ForJsonSchema(schemaElement)
			};

			var chatRequest = AbstractionMapper.ToOllamaSharpChatRequest(chatMessages, options, stream: true, JsonSerializerOptions.Default);
			chatRequest.Format.ShouldBe(schemaElement);
		}

		private class Sword
		{
			public required string Name { get; set; }
			public required int Damage { get; set; }
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
				.AddOllamaOption(OllamaOption.VocabOnly, false)
				.AddOllamaOption(OllamaOption.Think, false);

			var ollamaRequest = AbstractionMapper.ToOllamaSharpChatRequest([], options, stream: true, JsonSerializerOptions.Default);

			ollamaRequest.Options.F16kv.ShouldBe(true);
			ollamaRequest.Options.FrequencyPenalty.ShouldBe(0.11f);
			ollamaRequest.Options.LogitsAll.ShouldBe(false);
			ollamaRequest.Options.LowVram.ShouldBe(true);
			ollamaRequest.Options.MainGpu.ShouldBe(1);
			ollamaRequest.Options.MinP.ShouldBe(0.22f);
			ollamaRequest.Options.MiroStat.ShouldBe(2);
			ollamaRequest.Options.MiroStatEta.ShouldBe(0.33f);
			ollamaRequest.Options.MiroStatTau.ShouldBe(0.44f);
			ollamaRequest.Options.Numa.ShouldBe(false);
			ollamaRequest.Options.NumBatch.ShouldBe(3);
			ollamaRequest.Options.NumCtx.ShouldBe(4);
			ollamaRequest.Options.NumGpu.ShouldBe(5);
			ollamaRequest.Options.NumGqa.ShouldBe(6);
			ollamaRequest.Options.NumKeep.ShouldBe(7);
			ollamaRequest.Options.NumPredict.ShouldBe(8);
			ollamaRequest.Options.NumThread.ShouldBe(9);
			ollamaRequest.Options.PenalizeNewline.ShouldBe(true);
			ollamaRequest.Options.PresencePenalty.ShouldBe(0.55f);
			ollamaRequest.Options.RepeatLastN.ShouldBe(10);
			ollamaRequest.Options.RepeatPenalty.ShouldBe(0.66f);
			ollamaRequest.Options.Seed.ShouldBe(11);
			ollamaRequest.Options.Stop.ShouldBe(["stop", "quit", "exit"], ignoreOrder: true);
			ollamaRequest.Options.Temperature.ShouldBe(0.77f);
			ollamaRequest.Options.TfsZ.ShouldBe(0.88f);
			ollamaRequest.Options.TopK.ShouldBe(12);
			ollamaRequest.Options.TopP.ShouldBe(0.99f);
			ollamaRequest.Options.TypicalP.ShouldBe(1.01f);
			ollamaRequest.Options.UseMlock.ShouldBe(false);
			ollamaRequest.Options.UseMmap.ShouldBe(true);
			ollamaRequest.Options.VocabOnly.ShouldBe(false);
			ollamaRequest.Think.ShouldBe(false);
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

			var response = AbstractionMapper.ToChatResponse(stream, usedModel: null);

			response.AdditionalProperties.ShouldNotBeNull();
			response.AdditionalProperties[Application.EvalDuration].ShouldBe(TimeSpan.FromSeconds(2.222222222));
			response.AdditionalProperties[Application.LoadDuration].ShouldBe(TimeSpan.FromSeconds(3.333333333));
			response.AdditionalProperties[Application.TotalDuration].ShouldBe(TimeSpan.FromSeconds(6.666666666));
			response.AdditionalProperties[Application.PromptEvalDuration].ShouldBe(TimeSpan.FromSeconds(5.555555555));
			response.CreatedAt.ShouldBe(new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7)));
			response.FinishReason.ShouldBe(ChatFinishReason.Stop);
			response.Messages[0].AuthorName.ShouldBeNull();
			response.Messages[0].RawRepresentation.ShouldBe(stream.Message);
			response.Messages[0].Role.ShouldBe(Microsoft.Extensions.AI.ChatRole.Assistant);
			response.Messages[0].Text.ShouldBe("Hi.");
			response.Messages[0].Contents.Count.ShouldBe(1);
			((TextContent)response.Messages[0].Contents[0]).Text.ShouldBe("Hi.");
			response.ModelId.ShouldBe("llama3.1:8b");
			response.RawRepresentation.ShouldBe(stream);
			response.ResponseId.ShouldBe(ollamaCreatedStamp);
			response.Usage.ShouldNotBeNull();
			response.Usage.InputTokenCount.ShouldBe(411);
			response.Usage.OutputTokenCount.ShouldBe(111);
			response.Usage.TotalTokenCount.ShouldBe(111 + 411);
		}
	}

	public class ToChatResponseUpdateMethod : AbstractionMapperTests
	{
		[Test]
		public void Maps_Known_Properties()
		{
			var ollamaCreated = new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7));

			var stream = new ChatResponseStream
			{
				CreatedAt = ollamaCreated,
				Done = true,
				Message = new Message { Role = OllamaSharp.Models.Chat.ChatRole.Assistant, Content = "Hi." },
				Model = "llama3.1:8b"
			};

			var streamingChatCompletion = AbstractionMapper.ToChatResponseUpdate(stream, "12345");

			streamingChatCompletion.AdditionalProperties.ShouldBeNull();
			streamingChatCompletion.AuthorName.ShouldBeNull();
			streamingChatCompletion.Contents.Count.ShouldBe(1);
			((TextContent)streamingChatCompletion.Contents[0]).Text.ShouldBe("Hi.");
			streamingChatCompletion.CreatedAt.ShouldBe(new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7)));
			streamingChatCompletion.FinishReason.ShouldBe(ChatFinishReason.Stop);
			streamingChatCompletion.RawRepresentation.ShouldBe(stream);
			streamingChatCompletion.ResponseId.ShouldBe("12345");
			streamingChatCompletion.Role.ShouldBe(Microsoft.Extensions.AI.ChatRole.Assistant);
			streamingChatCompletion.Text.ShouldBe("Hi.");
		}

		[Test]
		public void Maps_Thinking_Tokens()
		{
			var ollamaCreated = new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7));

			var stream = new ChatResponseStream
			{
				CreatedAt = ollamaCreated,
				Done = true,
				Message = new Message { Role = OllamaSharp.Models.Chat.ChatRole.Assistant, Content = "", Thinking = "Beer." },
				Model = "llama3.1:8b"
			};

			var streamingChatCompletion = AbstractionMapper.ToChatResponseUpdate(stream, "12345");

			streamingChatCompletion.AdditionalProperties.ShouldBeNull();
			streamingChatCompletion.AuthorName.ShouldBeNull();
			streamingChatCompletion.Contents.Count.ShouldBe(1);
			((TextReasoningContent)streamingChatCompletion.Contents[0]).Text.ShouldBe("Beer.");
			streamingChatCompletion.CreatedAt.ShouldBe(new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7)));
			streamingChatCompletion.FinishReason.ShouldBe(ChatFinishReason.Stop);
			streamingChatCompletion.RawRepresentation.ShouldBe(stream);
			streamingChatCompletion.ResponseId.ShouldBe("12345");
			streamingChatCompletion.Role.ShouldBe(Microsoft.Extensions.AI.ChatRole.Assistant);
			streamingChatCompletion.Text.ShouldBeEmpty();
		}

		[Test]
		public void Maps_ToolCalls()
		{
			var ollamaCreated = new DateTimeOffset(2023, 08, 04, 08, 52, 19, 385, 406, TimeSpan.FromHours(-7));

			var message = new Message
			{
				Role = OllamaSharp.Models.Chat.ChatRole.Assistant,
				Content = "It seems the sun will be out all day.",
				ToolCalls =
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
				]
			};

			var stream = new ChatResponseStream
			{
				CreatedAt = ollamaCreated,
				Done = true,
				Message = message,
				Model = "llama3.1:8b"
			};

			var chatMessage = AbstractionMapper.ToChatResponseUpdate(stream, "12345");

			chatMessage.AdditionalProperties.ShouldBeNull();
			chatMessage.AuthorName.ShouldBeNull();
			chatMessage.Contents.Count.ShouldBe(2);
			chatMessage.Contents.First().ShouldBeOfType<TextContent>();
			((TextContent)chatMessage.Contents.First()).Text.ShouldBe("It seems the sun will be out all day.");
			var toolCall = chatMessage.Contents.Last() as FunctionCallContent;
			toolCall.AdditionalProperties.ShouldBeNull();
			toolCall.Arguments.Count.ShouldBe(2);
			toolCall.Arguments["city"].ShouldBe("Honululu");
			toolCall.Arguments["unit"].ShouldBe("celsius");
			toolCall.CallId.Length.ShouldBe(8); // random guid
			toolCall.Exception.ShouldBeNull();
			toolCall.Name.ShouldBe("get_weather");
			toolCall.RawRepresentation.ShouldBe(message.ToolCalls.Single());
			chatMessage.RawRepresentation.ShouldBe(stream);
			chatMessage.Role.ShouldBe(Microsoft.Extensions.AI.ChatRole.Assistant);
			chatMessage.Text.ShouldBe("It seems the sun will be out all day.");
		}
	}
}

public class ToChatMessageMethod : AbstractionMapperTests
{
	[Test]
	public void Maps_ToolCalls()
	{
		var message = new Message
		{
			Role = OllamaSharp.Models.Chat.ChatRole.Assistant,
			Content = "It seems the sun will be out all day.",
			ToolCalls =
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
			]
		};

		var chatMessage = AbstractionMapper.ToChatMessage(message);

		chatMessage.AdditionalProperties.ShouldBeNull();
		chatMessage.AuthorName.ShouldBeNull();
		chatMessage.Contents.Count.ShouldBe(2);
		chatMessage.Contents.First().ShouldBeOfType<TextContent>();
		((TextContent)chatMessage.Contents.First()).Text.ShouldBe("It seems the sun will be out all day.");
		var toolCall = chatMessage.Contents.Last() as FunctionCallContent;
		toolCall.AdditionalProperties.ShouldBeNull();
		toolCall.Arguments.Count.ShouldBe(2);
		toolCall.Arguments["city"].ShouldBe("Honululu");
		toolCall.Arguments["unit"].ShouldBe("celsius");
		toolCall.CallId.Length.ShouldBe(8); // random guid
		toolCall.Exception.ShouldBeNull();
		toolCall.Name.ShouldBe("get_weather");
		toolCall.RawRepresentation.ShouldBe(message.ToolCalls.Single());
		chatMessage.RawRepresentation.ShouldBe(message);
		chatMessage.Role.ShouldBe(Microsoft.Extensions.AI.ChatRole.Assistant);
		chatMessage.Text.ShouldBe("It seems the sun will be out all day.");
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

		request.Input.ShouldBe(["Teenage ", " Dirtbag."], ignoreOrder: true);
		request.KeepAlive.ShouldBeNull();
		request.Model.ShouldBe("nomic_embed");
		request.Options.ShouldBeNull();
		request.Truncate.ShouldBeNull();
	}

	[Test]
	public void Maps_KeepAlive_And_Truncate_From_AdditionalProperties()
	{
		var options = new EmbeddingGenerationOptions
		{
			AdditionalProperties = []
		};
		options.AdditionalProperties["keep_alive"] = "60m";
		options.AdditionalProperties["truncate"] = true;

		var request = AbstractionMapper.ToOllamaEmbedRequest([], options);

		request.KeepAlive.ShouldBe("60m");
		request.Truncate.ShouldBe(true);
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

		mappedResponse.AdditionalProperties.ShouldNotBeNull();
		mappedResponse.Count.ShouldBe(2);
		mappedResponse[0].ModelId.ShouldBe("model");
		mappedResponse[0].Vector.ToArray().ShouldBe([0.101f, 0.102f, 0.103f], ignoreOrder: true);
		mappedResponse[1].ModelId.ShouldBe("model");
		mappedResponse[1].Vector.ToArray().ShouldBe([0.201f, 0.202f, 0.203f], ignoreOrder: true);
		mappedResponse.Usage.InputTokenCount.ShouldBe(18);
		mappedResponse.Usage.OutputTokenCount.ShouldBeNull();
		mappedResponse.Usage.TotalTokenCount.ShouldBe(18);
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
