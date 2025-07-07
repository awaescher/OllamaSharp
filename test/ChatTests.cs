using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Shouldly;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

public class ChatTests
{
	private readonly TestOllamaApiClient _ollama = new();

	public class SendMethod : ChatTests
	{
		[Test]
		public async Task Sends_Assistant_Answer_To_Streamer()
		{
			_ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "Hi hu") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "man, how") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, " are you?") });

			var chat = new Chat(_ollama);
			var answer = await chat.SendAsync("henlo", CancellationToken.None).StreamToEndAsync();

			answer.ShouldBe("Hi human, how are you?");

			chat.Messages.Last().Role.ShouldBe(ChatRole.Assistant);
			chat.Messages.Last().Content.ShouldBe("Hi human, how are you?");
		}

		[Test]
		public async Task Sends_Assistant_ToolsCall_To_Streamer()
		{
			_ollama.SetExpectedChatResponses(
				new ChatResponseStream
				{
					Message = new Message
					{
						Role = ChatRole.Assistant,
						Content = "",
						ToolCalls = [
							new Message.ToolCall
							{
								Function = new Message.Function
								{
									Name = "get_current_weather",
									Arguments = new Dictionary<string, object?>()
									{
										["format"] = "celsius",
										["location"] = "Los Angeles, CA",
										["number"] = 30,
									}
								}
							}
						]
					}
				});

			var chat = new Chat(_ollama) { ToolInvoker = null }; // we have no tool implementation in this test
			await chat.SendAsync("How is the weather in LA?", CancellationToken.None).StreamToEndAsync();

			chat.Messages.Last().Role.ShouldBe(ChatRole.Assistant);
			chat.Messages.Last().ToolCalls.Count().ShouldBe(1);
			chat.Messages.Last().ToolCalls.ElementAt(0).Function.Name.ShouldBe("get_current_weather");
		}

		[Test]
		public async Task Sends_System_Prompt_Message()
		{
			var chat = new Chat(_ollama, "Speak like a pirate.");
			await chat.SendAsync("henlo", CancellationToken.None).StreamToEndAsync();

			chat.Messages.First().Role.ShouldBe(ChatRole.System);
			chat.Messages.First().Content.ShouldBe("Speak like a pirate.");
		}

		[Test]
		public async Task Sends_Messages_As_User()
		{
			var chat = new Chat(_ollama);
			await chat.SendAsync("henlo", CancellationToken.None).StreamToEndAsync();

			chat.Messages.First().Role.ShouldBe(ChatRole.User);
			chat.Messages.First().Content.ShouldBe("henlo");
		}

		[Test]
		public async Task Sends_Image_Bytes_As_Base64()
		{
			var bytes1 = System.Text.Encoding.ASCII.GetBytes("ABC");
			var bytes2 = System.Text.Encoding.ASCII.GetBytes("ABD");

			var chat = new Chat(_ollama);
			await chat.SendAsync("", [bytes1, bytes2], CancellationToken.None).StreamToEndAsync();

			chat.Messages.Single(m => m.Role == ChatRole.User).Images.ShouldBe(["QUJD", "QUJE"], ignoreOrder: true);
		}
	}

	public class SendAsMethod : ChatTests
	{
		[Test]
		public async Task Sends_Messages_As_Defined_Role()
		{
			_ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "Hi") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, " tool.") });

			var chat = new Chat(_ollama);
			await chat.SendAsAsync(ChatRole.Tool, "Henlo assistant.", CancellationToken.None).StreamToEndAsync();

			var history = chat.Messages.ToArray();
			history.Length.ShouldBe(2);
			history[0].Role.ShouldBe(ChatRole.Tool);
			history[0].Content.ShouldBe("Henlo assistant.");
			history[1].Role.ShouldBe(ChatRole.Assistant);
			history[1].Content.ShouldBe("Hi tool.");
		}

		[Test]
		public async Task Sends_Image_Bytes_As_Base64()
		{
			var bytes1 = System.Text.Encoding.ASCII.GetBytes("ABC");
			var bytes2 = System.Text.Encoding.ASCII.GetBytes("ABD");

			var chat = new Chat(_ollama);
			await chat.SendAsAsync(ChatRole.User, "", [bytes1, bytes2], CancellationToken.None).StreamToEndAsync();

			chat.Messages.Single(m => m.Role == ChatRole.User).Images.ShouldBe(["QUJD", "QUJE"], ignoreOrder: true);
		}
	}

	public class MessagesPropertyMethod : ChatTests
	{
		[Test]
		public void Replaces_Chat_History()
		{
			var chat = new Chat(_ollama)
			{
				Messages = [new Message { Content = "A", Role = ChatRole.System }]
			};
			chat.Messages.Single().Content.ShouldBe("A");

			chat.Messages = [new Message { Content = "B", Role = ChatRole.System }];
			chat.Messages.Single().Content.ShouldBe("B");
		}
	}

	protected static Message CreateMessage(ChatRole role, string content)
		=> new() { Role = role, Content = content };
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
