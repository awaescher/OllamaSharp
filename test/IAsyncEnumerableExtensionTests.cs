using FluentAssertions;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class IAsyncEnumerableExtensionTests
{
	public class StreamToEndMethod : IAsyncEnumerableExtensionTests
	{
		[Test]
		public async Task Appends_Stream_To_One_Single_Response_Value()
		{
			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "Hi hu") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "man, how") },
				new ChatDoneResponseStream { Message = CreateMessage(ChatRole.Assistant, " are you?"), Done = true });

			var answer = await ollama.Chat(new ChatRequest()).StreamToEnd();

			answer.Message.Content.Should().Be("Hi human, how are you?");
		}

		[Test]
		public async Task Calls_The_Optional_Callback_For_Each_Item()
		{
			var concatinatedItems = "";

			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "A") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "B") },
				new ChatDoneResponseStream { Message = CreateMessage(ChatRole.Assistant, "C"), Done = true });

			await ollama.Chat(new ChatRequest()).StreamToEnd(r => concatinatedItems += r.Message.Content);

			concatinatedItems.Should().Be("ABC");
		}

		/// <summary>
		/// This test documents the expected behavior that a stream of chat responses needs to end with a
		/// message that sets Done to true.
		/// </summary>
		[Test]
		public async Task Throws_If_No_Done_Response_Was_Send()
		{
			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "This message") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, " is not compl") }); // missing last message with Done=true

			Func<Task> act = async () => await ollama.Chat(new ChatRequest()).StreamToEnd();

			await act.Should().ThrowAsync<InvalidOperationException>();

			ollama.SetExpectedGenerateResponses(
				new GenerateResponseStream { Response = "This message" },
				new GenerateResponseStream { Response = " is not compl" }); // missing last message with Done=true

			act = async () => await ollama.Generate(new GenerateRequest()).StreamToEnd();

			await act.Should().ThrowAsync<InvalidOperationException>();
		}

		private static Message CreateMessage(ChatRole role, string content)
			=> new() { Role = role, Content = content };
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
