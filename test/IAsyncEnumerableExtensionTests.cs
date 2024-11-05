using FluentAssertions;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

public class IAsyncEnumerableExtensionTests
{
	public class StreamToEndAsyncMethod : IAsyncEnumerableExtensionTests
	{
		[Test]
		public async Task Appends_Stream_To_One_Single_Response_Value()
		{
			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "Hi hu") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "man, how") },
				new ChatDoneResponseStream { Message = CreateMessage(ChatRole.Assistant, " are you?"), Done = true });

			var answer = await ollama.ChatAsync(new ChatRequest()).StreamToEndAsync();

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

			await ollama.ChatAsync(new ChatRequest()).StreamToEndAsync(r => concatinatedItems += r.Message.Content);

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

			Func<Task> act = async () => await ollama.ChatAsync(new ChatRequest()).StreamToEndAsync();

			await act.Should().ThrowAsync<InvalidOperationException>();

			ollama.SetExpectedGenerateResponses(
				new GenerateResponseStream { Response = "This message" },
				new GenerateResponseStream { Response = " is not compl" }); // missing last message with Done=true

			act = async () => await ollama.GenerateAsync(new GenerateRequest()).StreamToEndAsync();

			await act.Should().ThrowAsync<InvalidOperationException>();
		}

		private static Message CreateMessage(ChatRole role, string content)
			=> new() { Role = role, Content = content };
	}
}