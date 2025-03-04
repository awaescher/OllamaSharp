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
			var mcpServersAdded = false;
			var systemPrompt = ReadInput($"Define a system prompt [{HintTextColor}](optional)[/]");

			var tools = await GetTools(false);

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("When asked for the weather or the news for a given location, it will try to use a predefined tool.");
				AnsiConsole.MarkupLine("If any tool is used, the intended usage information is printed.");
				WriteChatInstructionHint();

				if (!mcpServersAdded)
				{
					AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{USE_MCP_SERVER_COMMAND}[/] to use tools from MCP servers. Caution, please install following MCP servers for this demo: [/]");
					AnsiConsole.MarkupLine($"[{HintTextColor}]npm install -g @modelcontextprotocol/server-filesystem [/]");
					AnsiConsole.MarkupLine($"[{HintTextColor}]npm install -g @modelcontextprotocol/server-github [/]");
				}
				AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{LIST_TOOLS_COMMAND}[/] to list all available tools.[/]");

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

					if (message.Equals(USE_MCP_SERVER_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						mcpServersAdded = true;
						tools = await GetTools(true);
						break;
					}

					if (message.Equals(LIST_TOOLS_COMMAND, StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						ListTools(tools);
						break;
					}

					var currentMessageCount = chat.Messages.Count;

					try
					{
						await foreach (var answerToken in chat.SendAsync(message, tools))
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

	private static void ListTools(object[] tools)
	{
		AnsiConsole.MarkupLine("\n[purple]Available tools:[/]");

		foreach (var tool in tools)
		{
			if (tool is not OllamaSharp.Models.Chat.Tool chatTool)
				break;

			AnsiConsole.MarkupLineInterpolated($"{chatTool.Function?.Name ?? "Unknown"}\t\t[purple]{chatTool.Function?.Description}[/]");
		}
	}

	private static async Task<object[]> GetTools(bool withMcpServers)
	{
		object[] tools = [new GetWeatherTool(), new GetLatLonAsyncTool()];

		if (withMcpServers)
			tools = tools.Union(await OllamaSharp.ModelContextProtocol.Tools.GetFromMcpServers("server_config.json")).ToArray();

		return tools.ToArray();
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
		await Task.Delay(1000).ConfigureAwait(false);
		return $"{new Random().Next(20, 50)}.4711, {new Random().Next(3, 15)}.0815";
	}
}