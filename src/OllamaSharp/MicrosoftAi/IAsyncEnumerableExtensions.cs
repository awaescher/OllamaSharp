using Microsoft.Extensions.AI;

namespace OllamaSharp;

/// <summary>
/// Extension methods to stream IAsyncEnumerable to its end and return one single result value
/// </summary>
public static partial class IAsyncEnumerableExtensions
{
	/// <summary>
	/// Streams a given <see cref="IAsyncEnumerable{StreamingChatCompletionUpdate}"/> of response chunks to its end and builds one single <see cref="MicrosoftAi.ChatResponseUpdateAppender"/> out of them.
	/// </summary>
	/// <param name="stream">The <see cref="IAsyncEnumerable{StreamingChatCompletionUpdate}"/> to stream.</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable.</param>
	/// <returns>A single <see cref="MicrosoftAi.ChatResponseUpdateAppender"/> built up from every single IAsyncEnumerable item.</returns>
	public static Task<ChatResponseUpdate?> StreamToEndAsync(this IAsyncEnumerable<ChatResponseUpdate?> stream, Action<ChatResponseUpdate?>? itemCallback = null)
		=> stream.StreamToEndAsync(new MicrosoftAi.ChatResponseUpdateAppender(), itemCallback);
}