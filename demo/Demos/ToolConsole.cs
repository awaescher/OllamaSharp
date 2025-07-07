using System.Reflection;
using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Exceptions;
using OllamaSharp.Tools;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

public class ToolConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	public List<object> Tools { get; } = [new GetWeatherTool(), new GetLatLonAsyncTool(), new GetPopulationTool()];

	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Tool chat").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			var keepChatting = true;
			var mcpAdded = false;
			var systemPrompt = ReadInput($"Define a system prompt [{HintTextColor}](optional)[/]");

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather, population or the GPS coordinates for a city, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				if (!mcpAdded)
				{
					AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{USE_MCP_SERVER_COMMAND}[/] to use tools from MCP servers. Caution, please install following MCP servers for this demo: [/]");
					AnsiConsole.MarkupLine($"[{HintTextColor}]npm install -g @modelcontextprotocol/server-filesystem [/]");
					AnsiConsole.MarkupLine($"[{HintTextColor}]npm install -g @modelcontextprotocol/server-github [/]");
				}
				AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{LIST_TOOLS_COMMAND}[/] to list all available tools.[/]");

				var chat = new Chat(Ollama, systemPrompt) { Think = Think };
				chat.OnThink += (sender, thoughts) => AnsiConsole.MarkupInterpolated($"[{AiThinkTextColor}]{thoughts}[/]");
				chat.OnToolResult += (sender, result) => RenderToolResult(result);

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
						chat.Think = Think;
						continue;
					}

					if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						break;
					}

					if (message.Equals(USE_MCP_SERVER_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;

						if (!mcpAdded)
						{
							var mcpTools = await GetMcpTools();

							if (mcpTools.Any())
							{
								Tools.AddRange(mcpTools);
								mcpAdded = true;
							}
							AnsiConsole.MarkupLine($"[{HintTextColor}]{mcpTools.Count()} tool(s) added.[/]");
						}

						break;
					}

					if (message.Equals(LIST_TOOLS_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						ListTools(Tools);
						break;
					}

					try
					{
						await foreach (var answerToken in chat.SendAsync(message, Tools))
							AnsiConsole.MarkupInterpolated($"[{AiTextColor}]{answerToken}[/]");
					}
					catch (OllamaException ex)
					{
						AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	private static void RenderToolResult(ToolResult toolResult)
	{
		AnsiConsole.MarkupLine($"[gray]Tool: [/][purple]{RenderToolSignature(toolResult)}[/]");
		AnsiConsole.MarkupLineInterpolated($"      = [blue]{toolResult.Result?.ToString() ?? ""}[/]");
		AnsiConsole.WriteLine("");
	}

	private static string RenderToolSignature(ToolResult toolResult)
	{
		var builder = new StringBuilder(toolResult.Tool.Function?.Name ?? "Unknown");
		builder.Append('(');

		var separator = "";

		if (toolResult.ToolCall?.Function?.Arguments is not null)
		{
			foreach (var argument in toolResult.ToolCall.Function.Arguments)
			{
				builder.Append($"[gray]{separator}{argument.Key}:[/] {argument.Value}");
				separator = ", ";
			}
		}

		builder.Append(")");
		return builder.ToString();
	}

	private static void ListTools(IEnumerable<object> tools)
	{
		AnsiConsole.MarkupLine("\n[purple]Available tools:[/]");

		foreach (var tool in tools)
		{
			if (tool is not OllamaSharp.Models.Chat.Tool chatTool)
				break;

			AnsiConsole.MarkupLineInterpolated($"{chatTool.Function?.Name ?? "Unknown"}\t\t[purple]{chatTool.Function?.Description}[/]");
		}
	}

	private static async Task<object[]> GetMcpTools()
	{
		// expect a config file for the demo app
		var config = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "server_config.json");
		return await OllamaSharp.ModelContextProtocol.Tools.GetFromMcpServers(config);
	}

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

	/// <summary>
	/// Gets the latitude and longitude for a given location.
	/// </summary>
	/// <param name="location">The location to get the latitude and longitude for</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public async static Task<string> GetLatLonAsync(string location)
	{
		await Task.Delay(200).ConfigureAwait(false);
		return $"{new Random().Next(20, 50)}.4711, {new Random().Next(3, 15)}.0815";
	}

	/// <summary>
	/// Gets the amount of people living in a given city
	/// </summary>
	/// <param name="city">The city to get the population info for</param>
	/// <returns>The population of a given city</returns>
	[OllamaTool]
	public static int GetPopulation(string city) => new Random().Next(1000, 10000000);
}