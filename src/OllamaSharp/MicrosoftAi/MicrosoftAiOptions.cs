using Microsoft.Extensions.AI;

namespace OllamaSharp.MicrosoftAi;
/// <summary>
/// To Store the extract Microsoft AI options
/// </summary>
public class MicrosoftAiOptions
{
	/// <summary>
	/// Gets or sets the AI Chat options
	/// </summary>
	public ChatOptions? ChatOptions { get; set; }


	/// <summary>
	/// The ollama messge message history for tool calling purpuse
	/// </summary>
	public List<Models.Chat.Message>? OllamaMessageHistory { get; set; }
}
