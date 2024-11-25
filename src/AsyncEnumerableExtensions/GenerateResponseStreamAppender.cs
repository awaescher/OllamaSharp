using System;
using System.Text;
using OllamaSharp.Models;

namespace OllamaSharp.AsyncEnumerableExtensions;

/// <summary>
/// Appender to stream <see cref="System.Collections.Generic.IAsyncEnumerable{GenerateDoneResponseStream}"/>
/// to build up one single <see cref="GenerateDoneResponseStream"/> object
/// </summary>
internal class GenerateResponseStreamAppender : IAppender<GenerateResponseStream?, GenerateDoneResponseStream?>
{
	private readonly StringBuilder _builder = new();
	private GenerateDoneResponseStream? _lastItem;

	/// <summary>
	/// Appends a given <see cref="GenerateResponseStream"/> item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(GenerateResponseStream? item)
	{
		_builder.Append(item?.Response ?? string.Empty);

		if (item?.Done ?? false)
			_lastItem = (GenerateDoneResponseStream)item;
	}

	/// <summary>
	/// Builds up one single <see cref="GenerateDoneResponseStream"/> object
	/// from the previously streamed <see cref="GenerateResponseStream"/> items
	/// </summary>
	/// <returns>The completed, consolidated <see cref="GenerateDoneResponseStream"/> object</returns>
	public GenerateDoneResponseStream? Complete()
	{
		if (_lastItem is null)
			throw new InvalidOperationException("IAsyncEnumerable<GenerateResponseStream> did not yield an item with Done=true. The stream might be corrupted or incomplete.");

		_lastItem.Response = _builder.ToString();

		return _lastItem;
	}
}