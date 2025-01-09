using System.Reflection;
using System.Text.Json;
using System.Text.Json.Schema;
using CSharpToJsonSchema;
using OllamaSharp;
using OllamaSharp.Models.Chat;
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

	//private static object[] GetTools() => [MyOllamaTools.GeneratedOllamaTools.ToolsJson]; // [GeneratedOllamaTools.ToolsJson, JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(WeatherTool))];

	private static object[] GetTools() => [new Weather2Tool().AsTools()]; // [GeneratedOllamaTools.ToolsJson, JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(WeatherTool))];


	//private sealed class WeatherTool : Tool
	//{
	//	public WeatherTool()
	//	{
	//		Function = new Function
	//		{
	//			Description = "Get the current weather for a location",
	//			Name = "get_current_weather",
	//			Parameters = new Parameters
	//			{
	//				Properties = new Dictionary<string, Property>
	//				{
	//					["location"] = new() { Type = "string", Description = "The location to get the weather for, e.g. San Francisco, CA" },
	//					["format"] = new() { Type = "string", Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'", Enum = ["celsius", "fahrenheit"] },
	//				},
	//				Required = ["location", "format"],
	//			}
	//		};
	//		Type = "function";
	//	}
	//}

	//private sealed class NewsTool : Tool
	//{
	//	public NewsTool()
	//	{
	//		Function = new Function
	//		{
	//			Description = "Get the current news for a location",
	//			Name = "get_current_news",
	//			Parameters = new Parameters
	//			{
	//				Properties = new Dictionary<string, Property>
	//				{
	//					["location"] = new() { Type = "string", Description = "The location to get the news for, e.g. San Francisco, CA" },
	//					["category"] = new() { Type = "string", Description = "The optional category to filter the news, can be left empty to return all.", Enum = ["politics", "economy", "sports", "entertainment", "health", "technology", "science"] },
	//				},
	//				Required = ["location"],
	//			}
	//		};
	//		Type = "function";
	//	}
	//}
}

[GenerateJsonSchema]
public interface IWeather2Tool
{
	string GetWeather(string location, Unit unit);

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}

public class Weather2Tool : IWeather2Tool
{
	public string GetWeather(string location, IWeather2Tool.Unit unit) => $"It's cold at only 6° {unit} in {location}.";
}

public class WeatherTool
{
	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns></returns>
	[OllamaTool]
	public static string GetWeather(string location, IWeather2Tool.Unit unit) => $"It's cold at only 6° {unit} in {location}.";
}