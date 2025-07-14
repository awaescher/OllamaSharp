using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using OllamaSharp.Models;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

public class ExtensionsAiToolConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Extensions.AI").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			var keepChatting = true;
			var systemPrompt = ReadInput($"Define a system prompt [{HintTextColor}](optional)[/]");

			var services = new ServiceCollection();
			services.AddChatClient((IChatClient)Ollama).UseFunctionInvocation();
			var serviceProvider = services.BuildServiceProvider();

			var chatClient = serviceProvider.GetRequiredService<IChatClient>();
			var chatOptions = new ChatOptions();
			chatOptions.AddOllamaOption(OllamaOption.Think, Think);
			chatOptions.Tools = [AIFunctionFactory.Create(GetWeather), AIFunctionFactory.Create(GetLatLonAsync), AIFunctionFactory.Create(GetPopulation)];

			var messages = new List<ChatMessage>();

			if (!string.IsNullOrEmpty(systemPrompt))
				messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather, population or the GPS coordinates for a city, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{LIST_TOOLS_COMMAND}[/] to list all available tools.[/]");
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

					if (message.Equals(TOGGLETHINK_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						ToggleThink();
						keepChatting = true;
						chatOptions.AddOllamaOption(OllamaOption.Think, Think);
						continue;
					}

					if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						break;
					}

					if (message.Equals(LIST_TOOLS_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						ListTools(chatOptions.Tools);
						break;
					}

					try
					{
						messages.Add(new ChatMessage(ChatRole.User, message));

						await foreach (var answerToken in chatClient.GetStreamingResponseAsync(message, chatOptions))
						{
							if (answerToken.Role == ChatRole.Assistant && answerToken.Contents.Count == 1 && answerToken.Contents[0] is FunctionCallContent functionCall)
							{
								RenderFunctionCall(functionCall);
								continue;
							}

							if (answerToken.Role == ChatRole.Tool && answerToken.Contents.Count == 1 && answerToken.Contents[0] is FunctionResultContent functionResult)
							{
								RenderFunctionResult(functionResult);
								continue;
							}

							// model thoughts
							foreach (var toughts in answerToken.Contents.OfType<TextReasoningContent>())
								AnsiConsole.MarkupInterpolated($"[{AiThinkTextColor}]{toughts.Text}[/]");

							// model response
							if (!string.IsNullOrEmpty(answerToken.Text))
								AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
						}
					}
					catch (Exception ex)
					{
						AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static void ListTools(IEnumerable<AITool> tools)
	{
		AnsiConsole.MarkupLine("\n[purple]Available tools:[/]");

		foreach (var tool in tools)
		{
			AnsiConsole.MarkupLineInterpolated($"{tool.Name ?? "Unknown"}\t\t[purple]{tool.Description}[/]");
		}
	}

	private static void RenderFunctionCall(FunctionCallContent functionCall)
	{
		AnsiConsole.MarkupLine($"[gray]Tool: [/][purple]{RenderToolSignature(functionCall)}[/]");
	}

	private static void RenderFunctionResult(FunctionResultContent functionResult)
	{
		AnsiConsole.MarkupLineInterpolated($"[gray]Tool result: [/][purple]CallId {functionResult.CallId} =[/] [blue]{functionResult.Result?.ToString() ?? ""}[/]");
		AnsiConsole.WriteLine("");
	}

	private static string RenderToolSignature(FunctionCallContent functionCall)
	{
		var builder = new StringBuilder($"CallId {functionCall.CallId} -> {functionCall.Name ?? "Unknown"}");
		builder.Append('(');

		var separator = "";

		if (functionCall.Arguments is not null)
		{
			foreach (var argument in functionCall.Arguments)
			{
				builder.Append($"[gray]{separator}{argument.Key}:[/] {argument.Value}");
				separator = ", ";
			}
		}

		builder.Append(")");
		return builder.ToString();
	}

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}


	[Description("Gets the current weather for a given location.")]
	private static string GetWeather(
		[Description("The location or city to get the weather for")] string location,
		[Description("The unit to measure the temperature in")] Unit unit)
			=> $"It's cold at only 6° {unit} in {location}.";

	[Description("Gets the latitude and longitude for a given location.")]
	private async static Task<string> GetLatLonAsync(
		[Description("The location to get the latitude and longitude for")] string location)
	{
		await Task.Delay(200).ConfigureAwait(false);
		return $"{new Random().Next(20, 50)}.4711, {new Random().Next(3, 15)}.0815";
	}

	[Description("Gets the amount of people living in a given city")]
	private static int GetPopulation([Description("The city to get the population info for")] string city) => new Random().Next(1000, 10000000);

}
