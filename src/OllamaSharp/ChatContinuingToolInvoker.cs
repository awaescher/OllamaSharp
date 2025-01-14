using System.Runtime.CompilerServices;
using OllamaSharp.Models.Chat;

namespace OllamaSharp;

/// <summary>
/// A tool invoker that continues a chat conversation by sending results from invoked tools back to the chat.
/// </summary>
/// <param name="chat">The chat instance to use to continue the conversation</param>
public class ChatContinuingToolInvoker(Chat chat) : IToolInvoker
{
	/// <inheritdoc />
	public async IAsyncEnumerable<string> InvokeAsync(IEnumerable<Message.ToolCall> toolCalls, IEnumerable<object> tools, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var callableTools = tools?.OfType<Tool>().ToArray() ?? [];
		foreach (var toolCall in toolCalls)
		{
			var toolToCall = callableTools.FirstOrDefault(t => (t.Function?.Name ?? string.Empty).Equals((toolCall?.Function?.Name ?? string.Empty), StringComparison.OrdinalIgnoreCase));

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

			if (toolToCall is IInvokableTool i)
				toolResult = i.InvokeMethod(normalizedArguments);
			else if (toolToCall is IAsyncInvokableTool ai)
				toolResult = await ai.InvokeMethodAsync(normalizedArguments).ConfigureAwait(false);

			if (toolResult?.ToString() is string answerString && !string.IsNullOrEmpty(answerString))
			{
				await foreach (var answer in chat.SendAsAsync(ChatRole.Tool, answerString, tools: tools, imagesAsBase64: null, format: null, cancellationToken: cancellationToken).ConfigureAwait(false))
					yield return answer;
			}
		}
	}
}
