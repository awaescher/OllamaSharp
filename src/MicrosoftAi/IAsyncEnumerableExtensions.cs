using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OllamaSharp;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods to stream IAsyncEnumerable to its end and return one single result value
/// </summary>
public static partial class IAsyncEnumerableExtensions
{
	/// <summary>
	/// Streams a given <see cref="IAsyncEnumerable{StreamingChatCompletionUpdate}" /> of response chunks to its end and builds one single <see cref="StreamingChatCompletionUpdate"/> out of them.
	/// </summary>
	/// <param name="stream">The <see cref="IAsyncEnumerable{StreamingChatCompletionUpdate}" /> to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single <see cref="StreamingChatCompletionUpdate"/> built up from every single IAsyncEnumerable item</returns>
	public static Task<StreamingChatCompletionUpdate?> StreamToEndAsync(this IAsyncEnumerable<StreamingChatCompletionUpdate?> stream, Action<StreamingChatCompletionUpdate?>? itemCallback = null)
		=> stream.StreamToEndAsync(new MicrosoftAi.StreamingChatCompletionUpdateAppender(), itemCallback);
}
