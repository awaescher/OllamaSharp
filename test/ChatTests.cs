using FluentAssertions;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace Tests;

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
			var answer = await chat.Send("henlo", CancellationToken.None).StreamToEnd();

			answer.Should().Be("Hi human, how are you?");

			chat.Messages.Last().Role.Should().Be(ChatRole.Assistant);
			chat.Messages.Last().Content.Should().Be("Hi human, how are you?");
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
									Arguments = new Dictionary<string, string>()
									{
										["format"] = "celsius",
										["location"] = "Los Angeles, CA"
									}
								}

							}
						]
					}
				});

			var chat = new Chat(_ollama);
			await chat.Send("How is the weather in LA?", CancellationToken.None).StreamToEnd();

			chat.Messages.Last().Role.Should().Be(ChatRole.Assistant);
			chat.Messages.Last().ToolCalls.Should().HaveCount(1);
			chat.Messages.Last().ToolCalls!.ElementAt(0).Function!.Name.Should().Be("get_current_weather");
		}

		[Test]
		public async Task Sends_System_Prompt_Message()
		{
			var chat = new Chat(_ollama, "Speak like a pirate.");
			await chat.Send("henlo", CancellationToken.None).StreamToEnd();

			chat.Messages.First().Role.Should().Be(ChatRole.System);
			chat.Messages.First().Content.Should().Be("Speak like a pirate.");
		}

		[Test]
		public async Task Sends_Messages_As_User()
		{
			var chat = new Chat(_ollama);
			await chat.Send("henlo", CancellationToken.None).StreamToEnd();

			chat.Messages.First().Role.Should().Be(ChatRole.User);
			chat.Messages.First().Content.Should().Be("henlo");
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
			await chat.SendAs(ChatRole.Tool, "Henlo assistant.", CancellationToken.None).StreamToEnd();

			var history = chat.Messages.ToArray();
			history.Length.Should().Be(2);
			history[0].Role.Should().Be(ChatRole.Tool);
			history[0].Content.Should().Be("Henlo assistant.");
			history[1].Role.Should().Be(ChatRole.Assistant);
			history[1].Content.Should().Be("Hi tool.");
		}
	}

	public class SetMessagesMethod : ChatTests
	{
		[Test]
		public void Replaces_Chat_History()
		{
			var chat = new Chat(_ollama);

			chat.SetMessages([new Message { Content = "A", Role = ChatRole.System }]);
			chat.Messages.Single().Content.Should().Be("A");

			chat.SetMessages([new Message { Content = "B", Role = ChatRole.System }]);
			chat.Messages.Single().Content.Should().Be("B");
		}
	}

	protected static Message CreateMessage(ChatRole role, string content)
			=> new() { Role = role, Content = content };
}