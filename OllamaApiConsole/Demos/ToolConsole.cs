using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;
using Spectre.Console;

public class ToolConsole : OllamaConsole
{
	public ToolConsole(IOllamaApiClient ollama)
		: base(ollama)
	{
	}

	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Tool demo").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			AnsiConsole.MarkupLineInterpolated($"You are talking to [blue]{Ollama.SelectedModel}[/] now.");
			AnsiConsole.MarkupLineInterpolated($"When asked for the weather or the news for a given location, it will try to use a predefined tool.");
			AnsiConsole.MarkupLineInterpolated($"If any tool is used, the intended usage information is printed.");
			AnsiConsole.MarkupLine("[gray]Type \"[red]exit[/]\" to leave the chat.[/]");

			var chat = Ollama.Chat(stream => AnsiConsole.MarkupInterpolated($"[cyan]{stream?.Message.Content ?? ""}[/]"));
			string message;

			do
			{
				AnsiConsole.WriteLine();
				message = ReadMultilineInput();

				if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
					break;

				try
				{
					await chat.SendAs(ChatRole.User, message, GetTools());
				}
				catch (OllamaException ex)
				{
					AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
				}

				var toolCalls = chat.Messages.LastOrDefault()?.ToolCalls ?? [];
				if (toolCalls.Any())
				{
					AnsiConsole.MarkupLine("\n[purple]Tools used:[/]");

					foreach (var tool in toolCalls.Where(t => t.Function != null))
					{
						AnsiConsole.MarkupLineInterpolated($"  - [purple]{tool.Function!.Name}[/]");

						foreach (var argument in tool.Function.Arguments ?? [])
							AnsiConsole.MarkupLineInterpolated($"    - [purple]{argument.Key}[/]: [purple]{argument.Value}[/]");
					}
				}

				AnsiConsole.WriteLine();
			} while (!string.IsNullOrEmpty(message));
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
						["location"] = new Properties { Type = "string", Description = "The location to get the weather for, e.g. San Francisco, CA" },
						["format"] = new Properties { Type = "string", Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'", Enum = ["celsius", "fahrenheit"] },
					},
					Required = ["location", "fahrenheit"],
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
						["location"] = new Properties { Type = "string", Description = "The location to get the news for, e.g. San Francisco, CA" },
						["category"] = new Properties { Type = "string", Description = "The optional category to filter the news, can be left empty to return all.", Enum = ["politics", "economy", "sports", "entertainment", "health", "technology", "science"] },
					},
					Required = ["location"],
				}
			};
			Type = "function";
		}
	}
}