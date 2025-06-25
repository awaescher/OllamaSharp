using System.Runtime.CompilerServices;
using OllamaSharp.Models.Chat;
using AIFunction = Microsoft.Extensions.AI.AIFunction;
using AIFunctionArguments = Microsoft.Extensions.AI.AIFunctionArguments;
namespace OllamaSharp.MicrosoftAi.Tools;

/// <summary>
/// A tool invoker that continues a ollamaCLient conversation by sending results from invoked tools back to the ollamaCLient.
/// </summary>
public class MsAIToolInvoker()
{
	/// <summary>
	/// Invoke the AI Function and return the tool result messages
	/// </summary>
	/// <param name="toolCalls"></param>
	/// <param name="chatRequest"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static async Task<List<Message>> InvokeAsync(IEnumerable<Message.ToolCall> toolCalls, ChatRequest chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var toolMessages = new List<Message>();
		if (chatRequest.MicrosoftAi?.ChatOptions?.Tools != null)
		{
			var aiTools = chatRequest.MicrosoftAi?.ChatOptions.Tools;
			foreach (var toolCall in toolCalls)
			{
				var toolCallFunctionName = toolCall?.Function?.Name;
				var toolCallArgs = toolCall?.Function?.Arguments;
				var aiTool = (aiTools?.FirstOrDefault(t => t.Name.Equals(toolCallFunctionName, StringComparison.OrdinalIgnoreCase))) ?? throw new Exception($"AI Function \"{toolCallFunctionName}\" does not exists");
				object? toolResult = null;
				var aiFunctionArgs = new AIFunctionArguments();
				if (toolCallArgs is not null)
				{
					// make sure to translate JsonElements to strings
					foreach (var pair in toolCallArgs)
					{
						if (pair.Value is System.Text.Json.JsonElement je)
						{
							aiFunctionArgs.Add(pair.Key, je.ToString());
						}
						else
						{
							aiFunctionArgs.Add(pair.Key, pair.Value);
						}
					}
				}
				// Invoke the AI function with the argument
				if (aiTool is AIFunction aiFunction)
					toolResult = await aiFunction.InvokeAsync(aiFunctionArgs, cancellationToken);
				if (toolResult?.ToString() is string answerString && !string.IsNullOrEmpty(answerString))
				{
					var toolMessage = new Message(ChatRole.Tool, answerString);
					toolMessages.Add(toolMessage);
				}
			}
		}
		return toolMessages;
	}
}
