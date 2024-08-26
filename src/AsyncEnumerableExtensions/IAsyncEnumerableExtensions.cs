using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace OllamaSharp;

/// <summary>
/// Extension methods to stream IAsyncEnumerable to its end and return one single result value
/// </summary>
public static class IAsyncEnumerableExtensions
{
	/// <summary>
	/// Streams a given IAsyncEnumerable to its end and appends its items to a single response string
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single response stream appened from every IAsyncEnumerable item</returns>
	public static Task<string> StreamToEnd(this IAsyncEnumerable<string> stream, Action<string>? itemCallback = null)
		=> stream.StreamToEnd(new StringAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single GenerateDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single GenerateDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	public static Task<GenerateDoneResponseStream?> StreamToEnd(this IAsyncEnumerable<GenerateResponseStream?> stream, Action<GenerateResponseStream?>? itemCallback = null)
		=> stream.StreamToEnd(new GenerateResponseStreamAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single ChatDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single ChatDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	public static Task<ChatDoneResponseStream?> StreamToEnd(this IAsyncEnumerable<ChatResponseStream?> stream, Action<ChatResponseStream?>? itemCallback = null)
		=> stream.StreamToEnd(new ChatResponseStreamAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single ChatDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="appender">The appender instance used to build up one single response value</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single ChatDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	public static async Task<Tout> StreamToEnd<Tin, Tout>(this IAsyncEnumerable<Tin> stream, IAppender<Tin, Tout> appender, Action<Tin>? itemCallback = null)
	{
		await foreach (var item in stream)
		{
			appender.Append(item);
			itemCallback?.Invoke(item);
		}

		return appender.Complete();
	}
}
