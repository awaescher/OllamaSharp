namespace OllamaSharp.Tools;

/// <summary>
/// Represents a synchronous tool that can be invoked with a set of arguments.
/// </summary>
public interface IInvokableTool
{
	/// <summary>
	/// Invokes the method synchronously with the specified arguments.
	/// </summary>
	/// <param name="args">The arguments to pass to the method.</param>
	/// <returns>The result of the method invocation.</returns>
	object? InvokeMethod(IDictionary<string, object?>? args);
}
