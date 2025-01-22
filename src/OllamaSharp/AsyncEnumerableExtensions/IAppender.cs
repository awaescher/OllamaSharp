namespace OllamaSharp.AsyncEnumerableExtensions;

/// <summary>
/// Interface to append items while streaming an IAsyncEnumerable to the end
/// </summary>
/// <typeparam name="Tin">The type of the items of the IAsyncEnumerable</typeparam>
/// <typeparam name="Tout">The return type after the IAsyncEnumerable was streamed to the end</typeparam>
internal interface IAppender<in Tin, out Tout>
{
	/// <summary>
	/// Appends an item to build up the return value
	/// </summary>
	/// <param name="item">The item to append</param>
	void Append(Tin item);

	/// <summary>
	/// Completes and returns the return value built up from the appended items
	/// </summary>
	Tout Complete();
}
