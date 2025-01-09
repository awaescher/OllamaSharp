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

					try
					{
						await foreach (var answerToken in chat.SendAsync(message, GetTools()))
							AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
					}
					catch (OllamaException ex)
					{
						AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}

					var toolCalls = chat.Messages.LastOrDefault()?.ToolCalls?.ToArray() ?? [];
					if (toolCalls.Any())
					{
						AnsiConsole.MarkupLine("\n[purple]Tools used:[/]");

						foreach (var function in toolCalls.Where(t => t.Function != null).Select(t => t.Function))
						{
							AnsiConsole.MarkupLineInterpolated($"  - [purple]{function!.Name}[/]");

							AnsiConsole.MarkupLineInterpolated($"    - [purple]parameters[/]");

							if (function?.Arguments is not null)
							{
								foreach (var argument in function.Arguments)
									AnsiConsole.MarkupLineInterpolated($"      - [purple]{argument.Key}[/]: [purple]{argument.Value}[/]");
							}

							if (function is not null)
							{
								//var result = FunctionHelper.ExecuteFunction(function);
								//AnsiConsole.MarkupLineInterpolated($"    - [purple]return value[/]: [purple]\"{result}\"[/]");

								//await foreach (var answerToken in chat.SendAsAsync(ChatRole.Tool, result, GetTools()))
								//	AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
							}
						}
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static object[] GetTools() => [new GetWeatherTool(), new GetUserTool(), new GoogleTool()];

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

	[OllamaTool]
	public static string GetUser(string name, int userId = -1) => $"{name} ({userId}) is unknown.";

	[OllamaTool]
	public static GoogleResult Google(string query) => new(["Match 1", "Match 2", "Match 3"], 2);

	public record GoogleResult(string[] Matches, int Pages);
}