using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.AsyncEnumerableExtensions;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OllamaSharp;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods to stream IAsyncEnumerable to its end and return one single result value
/// </summary>
public static partial class IAsyncEnumerableExtensions
{
	/// <summary>
	/// Streams a given IAsyncEnumerable to its end and appends its items to a single response string
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single response stream append from every IAsyncEnumerable item</returns>
	public static Task<string> StreamToEndAsync(this IAsyncEnumerable<string> stream, Action<string>? itemCallback = null)
		=> stream.StreamToEndAsync(new StringAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single GenerateDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single GenerateDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	public static Task<GenerateDoneResponseStream?> StreamToEndAsync(this IAsyncEnumerable<GenerateResponseStream?> stream, Action<GenerateResponseStream?>? itemCallback = null)
		=> stream.StreamToEndAsync(new GenerateResponseStreamAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single ChatDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single ChatDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	public static Task<ChatDoneResponseStream?> StreamToEndAsync(this IAsyncEnumerable<ChatResponseStream?> stream, Action<ChatResponseStream?>? itemCallback = null)
		=> stream.StreamToEndAsync(new ChatResponseStreamAppender(), itemCallback);

	/// <summary>
	/// Streams a given IAsyncEnumerable of response chunks to its end and builds one single ChatDoneResponseStream out of them.
	/// </summary>
	/// <param name="stream">The IAsyncEnumerable to stream</param>
	/// <param name="appender">The appender instance used to build up one single response value</param>
	/// <param name="itemCallback">An optional callback to additionally process every single item from the IAsyncEnumerable</param>
	/// <returns>A single ChatDoneResponseStream built up from every single IAsyncEnumerable item</returns>
	internal static async Task<Tout> StreamToEndAsync<Tin, Tout>(this IAsyncEnumerable<Tin> stream, IAppender<Tin, Tout> appender, Action<Tin>? itemCallback = null)
	{
		await foreach (var item in stream.ConfigureAwait(false))
		{
			appender.Append(item);
			itemCallback?.Invoke(item);
		}

		return appender.Complete();
	}
}
