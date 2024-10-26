using System.Reflection;
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
								var result = FunctionHelper.ExecuteFunction(function);
								AnsiConsole.MarkupLineInterpolated($"    - [purple]return value[/]: [purple]\"{result}\"[/]");

								await foreach (var answerToken in chat.SendAsAsync(ChatRole.Tool, result, GetTools()))
									AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
							}
						}
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static IEnumerable<Tool> GetTools() => [new WeatherTool(), new NewsTool()];

	private sealed class WeatherTool : Tool
	{
		public WeatherTool()
		{
			Function = new Function
			{
				Description = "Get the current weather for a location",
				Name = "get_current_weather",
				Parameters = new Parameters
				{
					Properties = new Dictionary<string, Properties>
					{
						["location"] = new() { Type = "string", Description = "The location to get the weather for, e.g. San Francisco, CA" },
						["format"] = new() { Type = "string", Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'", Enum = ["celsius", "fahrenheit"] },
					},
					Required = ["location", "format"],
				}
			};
			Type = "function";
		}
	}

	private sealed class NewsTool : Tool
	{
		public NewsTool()
		{
			Function = new Function
			{
				Description = "Get the current news for a location",
				Name = "get_current_news",
				Parameters = new Parameters
				{
					Properties = new Dictionary<string, Properties>
					{
						["location"] = new() { Type = "string", Description = "The location to get the news for, e.g. San Francisco, CA" },
						["category"] = new() { Type = "string", Description = "The optional category to filter the news, can be left empty to return all.", Enum = ["politics", "economy", "sports", "entertainment", "health", "technology", "science"] },
					},
					Required = ["location"],
				}
			};
			Type = "function";
		}
	}

	private static class FunctionHelper
	{
		public static string ExecuteFunction(Message.Function function)
		{
			var toolFunction = _availableFunctions[function.Name!];
			var parameters = MapParameters(toolFunction.Method, function.Arguments!);
			return toolFunction.DynamicInvoke(parameters)?.ToString()!;
		}

		private static readonly Dictionary<string, Func<string, string?, string>> _availableFunctions = new()
		{
			["get_current_weather"] = (location, format) =>
			{
				var (temperature, unit) = format switch
				{
					"fahrenheit" => (Random.Shared.Next(23, 104), "°F"),
					_ => (Random.Shared.Next(-5, 40), "°C"),
				};

				return $"{temperature} {unit} in {location}";
			},
			["get_current_news"] = (location, category) =>
			{
				category = string.IsNullOrEmpty(category) ? "all" : category;
				return $"Could not find news for {location} (category: {category}).";
			}
		};

		private static object[] MapParameters(MethodBase method, IDictionary<string, object> namedParameters)
		{
			var paramNames = method.GetParameters().Select(p => p.Name).ToArray();
			var parameters = new object[paramNames.Length];

			for (var i = 0; i < parameters.Length; ++i)
				parameters[i] = Type.Missing;

			foreach (var (paramName, value) in namedParameters)
			{
				var paramIndex = Array.IndexOf(paramNames, paramName);
				if (paramIndex >= 0)
					parameters[paramIndex] = value?.ToString() ?? "";
			}

			return parameters;
		}
	}
}