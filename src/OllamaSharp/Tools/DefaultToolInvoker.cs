using OllamaSharp.Models.Chat;

namespace OllamaSharp.Tools;

/// <summary>
/// The default tool invoker that supports sync and async tools
/// </summary>
public class DefaultToolInvoker : IToolInvoker
{
	/// <inheritdoc />
	public async Task<ToolResult> InvokeAsync(Message.ToolCall toolCall, IEnumerable<object> tools, CancellationToken cancellationToken)
	{
		var callableTools = tools?.OfType<Tool>().ToArray() ?? [];
		var tool = callableTools.FirstOrDefault(t => (t.Function?.Name ?? string.Empty).Equals(toolCall?.Function?.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase));

		object? toolResult = null;

		var normalizedArguments = new Dictionary<string, object?>();

		if (toolCall?.Function?.Arguments is not null)
		{
			// make sure to translate JsonElements to strings
			foreach (var pair in toolCall.Function.Arguments)
			{
				if (pair.Value is System.Text.Json.JsonElement je)
					normalizedArguments[pair.Key] = je.ToString();
				else
					normalizedArguments[pair.Key] = pair.Value;
			}
		}

		if (tool is IInvokableTool i)
			toolResult = i.InvokeMethod(normalizedArguments);
		else if (tool is IAsyncInvokableTool ai)
			toolResult = await ai.InvokeMethodAsync(normalizedArguments).ConfigureAwait(false);

		return new ToolResult(Tool: tool, ToolCall: toolCall, Result: toolResult);
	}
}
