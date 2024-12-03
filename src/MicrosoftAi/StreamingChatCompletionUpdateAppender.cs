using Microsoft.Extensions.AI;
using OllamaSharp.AsyncEnumerableExtensions;
namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Appender to stream <see cref="System.Collections.Generic.IAsyncEnumerable{StreamingChatCompletionUpdate}" />
/// to build up one consolidated <see cref="StreamingChatCompletionUpdate"/> object
/// </summary>
internal class StreamingChatCompletionUpdateAppender : IAppender<StreamingChatCompletionUpdate?, StreamingChatCompletionUpdate?>
{
	private readonly StreamingChatCompletionUpdateBuilder _messageBuilder = new();

	/// <summary>
	/// Appends a given <see cref="StreamingChatCompletionUpdate"/> item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(StreamingChatCompletionUpdate? item) => _messageBuilder.Append(item);

	/// <summary>
	/// Builds up one final, single <see cref="StreamingChatCompletionUpdate"/> object from the previously streamed items
	/// </summary>
	/// <returns>The completed, consolidated <see cref="StreamingChatCompletionUpdate"/> object</returns>
	public StreamingChatCompletionUpdate? Complete() => _messageBuilder.Complete();
}