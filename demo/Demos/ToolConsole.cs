using OllamaSharp;
using OllamaSharp.Models.Exceptions;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

public class ToolConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Tool chat").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			var keepChatting = true;
			var systemPrompt = ReadInput($"Define a system prompt [{HintTextColor}](optional)[/]");

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather or the news for a given location, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				var chat = new Chat(Ollama, systemPrompt);

				string message;

				do
				{
					AnsiConsole.WriteLine();
					message = ReadInput();

					if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = false;
						break;
					}

					if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						break;
					}

					var currentMessageCount = chat.Messages.Count;

					try
					{
						await foreach (var answerToken in chat.SendAsync(message, GetTools()))
							AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
					}
					catch (OllamaException ex)
					{
						AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}

					// find the latest message from the assistant and possible tools
					var newMessages = chat.Messages.Skip(currentMessageCount);

					foreach (var newMessage in newMessages)
					{
						if (newMessage.ToolCalls?.Any() ?? false)
						{
							AnsiConsole.MarkupLine("\n[purple]Tools used:[/]");

							foreach (var function in newMessage.ToolCalls.Where(t => t.Function != null).Select(t => t.Function))
							{
								AnsiConsole.MarkupLineInterpolated($"  - [purple]{function!.Name}[/]");
								AnsiConsole.MarkupLineInterpolated($"    - [purple]parameters[/]");

								if (function?.Arguments is not null)
								{
									foreach (var argument in function.Arguments)
										AnsiConsole.MarkupLineInterpolated($"      - [purple]{argument.Key}[/]: [purple]{argument.Value}[/]");
								}
							}
						}

						if (newMessage.Role.GetValueOrDefault() == OllamaSharp.Models.Chat.ChatRole.Tool)
							AnsiConsole.MarkupLineInterpolated($"    [blue]-> \"{newMessage.Content}\"[/]");
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static object[] GetTools() => [new GetWeatherTool()];

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}

	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public static string GetWeather(string location, Unit unit) => $"It's cold at only 6° {unit} in {location}.";

	// TODO:
	// only static?
	// passing arguments?
	// enum argments may crash if not provided
	// not automatically excute tools -> Interfaces?
	// image as tool result, as ImagesAsBase64 in Chat.ToolInvoker?
}