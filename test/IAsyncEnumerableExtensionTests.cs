using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Shouldly;

namespace Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

/// <summary>
/// Contains tests for the <see cref="IAsyncEnumerableExtension.StreamToEndAsync"/> extension method.
/// </summary>
public class IAsyncEnumerableExtensionTests
{
	/// <summary>
	/// Tests for the <c>StreamToEndAsync</c> method.
	/// </summary>
	public class StreamToEndAsyncMethod : IAsyncEnumerableExtensionTests
	{
		/// <summary>
		/// Verifies that a stream of chat responses is concatenated into a single response value.
		/// </summary>
		[Test]
		public async Task Appends_Stream_To_One_Single_Response_Value()
		{
			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "Hi hu") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "man, how") },
				new ChatDoneResponseStream { Message = CreateMessage(ChatRole.Assistant, " are you?"), Done = true });

			var answer = await ollama.ChatAsync(new ChatRequest()).StreamToEndAsync();

			answer.Message.Content.ShouldBe("Hi human, how are you?");
		}

		/// <summary>
		/// Ensures that the optional callback is invoked for each streamed item.
		/// </summary>
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

			concatinatedItems.ShouldBe("ABC");
		}

		/// <summary>
		/// Verifies that an <see cref="InvalidOperationException"/> is thrown when the stream does not end with a
		/// response marked as done.
		/// </summary>
		[Test]
		public async Task Throws_If_No_Done_Response_Was_Send()
		{
			var ollama = new TestOllamaApiClient();

			ollama.SetExpectedChatResponses(
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, "This message") },
				new ChatResponseStream { Message = CreateMessage(ChatRole.Assistant, " is not compl") }); // missing last message with Done=true

			Func<Task> act = async () => await ollama.ChatAsync(new ChatRequest()).StreamToEndAsync();

			await act.ShouldThrowAsync<InvalidOperationException>();

			ollama.SetExpectedGenerateResponses(
				new GenerateResponseStream { Response = "This message" },
				new GenerateResponseStream { Response = " is not compl" }); // missing last message with Done=true

			act = async () => await ollama.GenerateAsync(new GenerateRequest()).StreamToEndAsync();

			await act.ShouldThrowAsync<InvalidOperationException>();
		}

		private static Message CreateMessage(ChatRole role, string content)
			=> new() { Role = role, Content = content };
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.