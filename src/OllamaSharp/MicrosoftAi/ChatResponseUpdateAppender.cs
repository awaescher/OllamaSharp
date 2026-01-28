using Microsoft.Extensions.AI;
using OllamaSharp.AsyncEnumerableExtensions;
namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Appender to stream <see cref="IAsyncEnumerable{ChatResponseUpdate}" />
/// to build up one consolidated <see cref="ChatResponseUpdate"/> object
/// </summary>
internal class ChatResponseUpdateAppender : IAppender<ChatResponseUpdate?, ChatResponseUpdate?>
{
	private readonly ChatResponseUpdateBuilder _messageBuilder = new();

	/// <summary>
	/// Appends a given <see cref="ChatResponseUpdate"/> item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(ChatResponseUpdate? item) => _messageBuilder.Append(item);

	/// <summary>
	/// Builds up one final, single <see cref="ChatResponseUpdate"/> object from the previously streamed items
	/// </summary>
	/// <returns>The completed, consolidated <see cref="ChatResponseUpdate"/> object</returns>
	public ChatResponseUpdate? Complete() => _messageBuilder.Complete();
}