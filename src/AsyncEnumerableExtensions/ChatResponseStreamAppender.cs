using System;
using OllamaSharp.Models.Chat;

namespace OllamaSharp.AsyncEnumerableExtensions;

/// <summary>
/// Appender to stream <see cref="System.Collections.Generic.IAsyncEnumerable{ChatResponseStream}"/> to
/// build up one single <see cref="ChatDoneResponseStream"/> object
/// </summary>
internal class ChatResponseStreamAppender : IAppender<ChatResponseStream?, ChatDoneResponseStream?>
{
	private readonly MessageBuilder _messageBuilder = new();
	private ChatDoneResponseStream? _lastItem;

	/// <summary>
	/// Appends a given <see cref="ChatResponseStream"/> item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(ChatResponseStream? item)
	{
		_messageBuilder.Append(item);

		if (item?.Done ?? false)
			_lastItem = (ChatDoneResponseStream)item;
	}

	/// <summary>
	/// Builds up one single <see cref="ChatDoneResponseStream"/> object from the
	/// previously streamed <see cref="ChatResponseStream"/> items
	/// </summary>
	/// <returns>The completed consolidated <see cref="ChatDoneResponseStream"/> object</returns>
	public ChatDoneResponseStream? Complete()
	{
		if (_lastItem is null)
			throw new InvalidOperationException("IAsyncEnumerable<ChatResponseStream> did not yield an item with Done=true. The stream might be corrupted or incomplete.");

		_lastItem.Message = _messageBuilder.ToMessage();

		return _lastItem;
	}
}