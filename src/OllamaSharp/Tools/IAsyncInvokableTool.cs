namespace OllamaSharp.Tools;

/// <summary>
/// Represents an asynchronous tool that can be invoked with a set of arguments.
/// </summary>
public interface IAsyncInvokableTool
{
	/// <summary>
	/// Invokes the method asynchronously with the specified arguments.
	/// </summary>
	/// <param name="args">The arguments to pass to the method.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the result of the method invocation.</returns>
	Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args);
}