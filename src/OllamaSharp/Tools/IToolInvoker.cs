using OllamaSharp.Models.Chat;

namespace OllamaSharp.Tools;

/// <summary>
/// Defines an interface for invoking tools asynchronously.
/// </summary>
public interface IToolInvoker
{
	/// <summary>
	/// Invokes a collection of tools based on the AI model's tool call asynchronously.
	/// </summary>
	/// <param name="toolCall">The tool call selected by the AI model to be invoked.</param>
	/// <param name="tools">The collection of tools to be used for invocation.</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>An asynchronous stream of results from the tool invocations.</returns>
	Task<ToolResult> InvokeAsync(Message.ToolCall toolCall, IEnumerable<object> tools, CancellationToken cancellationToken);
}