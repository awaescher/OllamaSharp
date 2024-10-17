using System;
using System.Text;
using OllamaSharp.Models;

namespace OllamaSharp;

/// <summary>
/// Appender to stream IAsyncEnumerable(GenerateResponseStream) to build up one single GenerateDoneResponseStream object
/// </summary>
public class GenerateResponseStreamAppender : IAppender<GenerateResponseStream?, GenerateDoneResponseStream?>
{
	private readonly StringBuilder _builder = new();
	private GenerateDoneResponseStream? _lastItem;

	/// <summary>
	/// Appends a given GenerateResponseStream item to build a single return object
	/// </summary>
	/// <param name="item">The item to append</param>
	public void Append(GenerateResponseStream? item)
	{
		_builder.Append(item?.Response ?? string.Empty);

		if (item?.Done ?? false)
			_lastItem = (GenerateDoneResponseStream)item;
	}

	/// <summary>
	/// Builds up one single GenerateDoneResponseStream object from the previously streamed GenerateResponseStream items
	/// </summary>
	public GenerateDoneResponseStream? Complete()
	{
		if (_lastItem is null)
			throw new InvalidOperationException("IAsyncEnumerable<GenerateResponseStream> did not yield an item with Done=true. The stream might be corrupted or incomplete.");

		_lastItem.Response = _builder.ToString();

		return _lastItem;
	}
}