using OllamaSharp.Models.Chat;

namespace OllamaSharp;

/// <summary>
/// Defines an interface for invoking tools asynchronously.
/// </summary>
public interface IToolInvoker
{
	/// <summary>
	/// Invokes a collection of tools by AI model tool calls asynchronously.
	/// </summary>
	/// <param name="toolCalls">The collection of tool calls the AI model chose to be invoked.</param>
	/// <param name="tools">The collection of tools to be used for invocation.</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>An asynchronous stream of results from the tool invocations.</returns>
	IAsyncEnumerable<string> InvokeAsync(IEnumerable<Message.ToolCall> toolCalls, IEnumerable<object> tools, CancellationToken cancellationToken);
}
