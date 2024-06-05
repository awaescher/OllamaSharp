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
			ChatResponseStream? answerFromAssistant = null!;

			_ollama.DefineChatResponse("assistant", "hi!");

			var chat = new Chat(_ollama, answer => answerFromAssistant = answer);
			await chat.Send("henlo", CancellationToken.None);

			answerFromAssistant.Should().NotBeNull();
			answerFromAssistant.Message.Role.Should().Be(ChatRole.Assistant);
			answerFromAssistant.Message.Content.Should().Be("hi!");
		}

		[Test]
		public async Task Sends_Messages_As_User()
		{
			var chat = new Chat(_ollama, _ => { });
			var history = (await chat.Send("henlo", CancellationToken.None)).ToArray();

			history[0].Role.Should().Be(ChatRole.User);
			history[0].Content.Should().Be("henlo");
		}

		[Test]
		public async Task Returns_User_And_Assistant_Message_History()
		{
			_ollama.DefineChatResponse(ChatRole.Assistant, "hi!");

			var chat = new Chat(_ollama, _ => { });
			var history = (await chat.Send("henlo", CancellationToken.None)).ToArray();

			history.Length.Should().Be(2);
			history[0].Role.Should().Be(ChatRole.User);
			history[0].Content.Should().Be("henlo");
			history[1].Role.Should().Be(ChatRole.Assistant);
			history[1].Content.Should().Be("hi!");
		}
	}

	public class SendAsMethod : ChatTests
	{
		[Test]
		public async Task Sends_Messages_As_Defined_Role()
		{
			_ollama.DefineChatResponse(ChatRole.Assistant, "hi system!");

			var chat = new Chat(_ollama, _ => { });
			var history = (await chat.SendAs(ChatRole.System, "henlo hooman", CancellationToken.None)).ToArray();

			history.Length.Should().Be(2);
			history[0].Role.Should().Be(ChatRole.System);
			history[0].Content.Should().Be("henlo hooman");
			history[1].Role.Should().Be(ChatRole.Assistant);
			history[1].Content.Should().Be("hi system!");
		}
	}
}