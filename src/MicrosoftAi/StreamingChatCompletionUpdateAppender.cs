using Microsoft.Extensions.AI;
using OllamaSharp.AsyncEnumerableExtensions;
namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Appender to stream IAsyncEnumerable(StreamingChatCompletionUpdate) to build up one single StreamingChatCompletionUpdate object
/// </summary>
public class StreamingChatCompletionUpdateAppender : IAppender<StreamingChatCompletionUpdate?, StreamingChatCompletionUpdate?>
{
	private readonly StreamingChatCompletionUpdateBuilder _messageBuilder = new();

	/// <summary>
	/// Appends a given StreamingChatCompletionUpdate item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(StreamingChatCompletionUpdate? item) => _messageBuilder.Append(item);

	/// <summary>
	/// Builds up one single StreamingChatCompletionUpdate object from the previously streamed items
	/// </summary>
	public StreamingChatCompletionUpdate? Complete() => _messageBuilder.Complete();
}