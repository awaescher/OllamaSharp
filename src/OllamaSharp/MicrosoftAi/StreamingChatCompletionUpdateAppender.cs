using Microsoft.Extensions.AI;
using OllamaSharp.AsyncEnumerableExtensions;
namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Appender to stream <see cref="IAsyncEnumerable{StreamingChatCompletionUpdate}" />
/// to build up one consolidated <see cref="ChatResponseUpdate"/> object
/// </summary>
internal class StreamingChatCompletionUpdateAppender : IAppender<ChatResponseUpdate?, ChatResponseUpdate?>
{
	private readonly ChatResponseUpdateBuilder _messageBuilder = new();

	/// <summary>
	/// Appends a given <see cref="StreamingChatCompletionUpdate"/> item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(ChatResponseUpdate? item) => _messageBuilder.Append(item);

	/// <summary>
	/// Builds up one final, single <see cref="StreamingChatCompletionUpdate"/> object from the previously streamed items
	/// </summary>
	/// <returns>The completed, consolidated <see cref="StreamingChatCompletionUpdate"/> object</returns>
	public ChatResponseUpdate? Complete() => _messageBuilder.Complete();
}