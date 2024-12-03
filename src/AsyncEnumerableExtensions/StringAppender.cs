using System.Text;

namespace OllamaSharp.AsyncEnumerableExtensions;

/// <summary>
/// Appender to stream <see cref="System.Collections.Generic.IAsyncEnumerable{String}"/> to build up one single result string
/// </summary>
internal class StringAppender : IAppender<string, string>
{
	private readonly StringBuilder _builder = new();

	/// <summary>
	/// Appends a given string value to the return value
	/// </summary>
	/// <param name="item">The string value to append</param>
	public void Append(string item) => _builder.Append(item);

	/// <summary>
	/// Returns the whole string value
	/// </summary>
	public string Complete() => _builder.ToString();
}
