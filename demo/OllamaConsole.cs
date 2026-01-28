using System.Text;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Spectre.Console;

namespace OllamaApiConsole;

/// <summary>
/// Provides common functionality for console applications that interact with an Ollama API client.
/// </summary>
/// <param name="ollama">The Ollama API client to use.</param>
public abstract class OllamaConsole(IOllamaApiClient ollama)
{
	/// <summary>
	/// Character used to indicate the start of a multiline input.
	/// </summary>
	private const char MULTILINE_OPEN = '[';

	/// <summary>
	/// Character used to indicate the end of a multiline input.
	/// </summary>
	private const char MULTILINE_CLOSE = ']';

	/// <summary>
	/// Gets the color name used for hint text.
	/// </summary>
	public static string HintTextColor { get; } = "gray";

	/// <summary>
	/// Gets the color name used for accent text.
	/// </summary>
	public static string AccentTextColor { get; } = "blue";

	/// <summary>
	/// Gets the color name used for warning text.
	/// </summary>
	public static string WarningTextColor { get; } = "yellow";

	/// <summary>
	/// Gets the color name used for error text.
	/// </summary>
	public static string ErrorTextColor { get; } = "red";

	/// <summary>
	/// Gets the color name used for AI-generated text.
	/// </summary>
	public static string AiTextColor { get; } = "cyan";

	/// <summary>
	/// Gets the color name used for AI thinking indicator text.
	/// </summary>
	public static string AiThinkTextColor { get; } = "gray";

	/// <summary>
	/// Command to start a new conversation.
	/// </summary>
	public static string START_NEW_COMMAND { get; } = "/new";

	/// <summary>
	/// Command to use the MCP server.
	/// </summary>
	public static string USE_MCP_SERVER_COMMAND { get; } = "/mcp";

	/// <summary>
	/// Command to list available tools.
	/// </summary>
	public static string LIST_TOOLS_COMMAND { get; } = "/tools";

	/// <summary>
	/// Command to exit the application.
	/// </summary>
	public static string EXIT_COMMAND { get; } = "/exit";

	/// <summary>
	/// Command to toggle think mode.
	/// </summary>
	public static string TOGGLETHINK_COMMAND { get; } = "/togglethink";

	/// <summary>
	/// Gets the Ollama API client used by this console.
	/// </summary>
	public IOllamaApiClient Ollama { get; } = ollama ?? throw new ArgumentNullException(nameof(ollama));

	/// <summary>
	/// Gets or sets the current think mode value.
	/// </summary>
	public ThinkValue? Think { get; private set; }

	/// <summary>
	/// Runs the console application. Implementations should contain the main interaction loop.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public abstract Task Run();

	/// <summary>
	/// Reads user input from the console, supporting optional multiline entry.
	/// </summary>
	/// <param name="prompt">Optional prompt to display before reading input.</param>
	/// <param name="additionalInformation">Optional additional information to display before reading input.</param>
	/// <returns>The trimmed user input string.</returns>
	public static string ReadInput(string prompt = "", string additionalInformation = "")
	{
		if (!string.IsNullOrEmpty(prompt))
			AnsiConsole.MarkupLine(prompt);

		if (!string.IsNullOrEmpty(additionalInformation))
			AnsiConsole.MarkupLine(additionalInformation);

		var builder = new StringBuilder();
		bool? isMultiLineActive = null;
		var needsCleaning = false;

		while (!isMultiLineActive.HasValue || isMultiLineActive.Value)
		{
			AnsiConsole.Markup($"[{AccentTextColor}]> [/]");
			var input = Console.ReadLine() ?? "";

			if (!isMultiLineActive.HasValue)
			{
				isMultiLineActive = input.TrimStart().StartsWith(MULTILINE_OPEN);
				needsCleaning = isMultiLineActive.GetValueOrDefault();
			}

			builder.AppendLine(input);

			if (input.TrimEnd().EndsWith(MULTILINE_CLOSE) && isMultiLineActive.GetValueOrDefault())
				isMultiLineActive = false;
		}

		if (needsCleaning)
			return builder.ToString().Trim().TrimStart(MULTILINE_OPEN).TrimEnd(MULTILINE_CLOSE);

		return builder.ToString().TrimEnd();
	}

	/// <summary>
	/// Writes a hint line describing available console commands and the current think mode.
	/// </summary>
	protected void WriteChatInstructionHint()
	{
		AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{START_NEW_COMMAND}[/] to start over or [{AccentTextColor}]{EXIT_COMMAND}[/] to leave.[/]");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Begin with [{AccentTextColor}]{Markup.Escape(MULTILINE_OPEN.ToString())}[/] to start multiline input. Submit it by ending with [{AccentTextColor}]{Markup.Escape(MULTILINE_CLOSE.ToString())}[/].[/]");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Think mode is [{AccentTextColor}]{Think?.ToString()?.ToLower() ?? "(null)"}[/]. Type [{AccentTextColor}]{TOGGLETHINK_COMMAND}[/] to change.[/]");
	}

	/// <summary>
	/// Toggles the think mode between null, false, and true.
	/// </summary>
	internal void ToggleThink()
	{
		// null -> false -> true -> null -> ...
		Think = Think == null ? false : ((bool?)Think == false ? true : ((bool?)Think == true ? null : false));
		AnsiConsole.MarkupLine($"[{HintTextColor}]Think mode is [{AccentTextColor}]{Think?.ToString()?.ToLower() ?? "(null)"}[/].[/]");
	}

	/// <summary>
	/// Prompts the user to select a model from the list of locally available models.
	/// </summary>
	/// <param name="prompt">The prompt text displayed to the user.</param>
	/// <param name="additionalInformation">Optional additional information displayed before the selection.</param>
	/// <returns>The selected model name, or an empty string if the back option is chosen.</returns>
	protected async Task<string> SelectModel(string prompt, string additionalInformation = "")
	{
		const string BACK = "..";

		var models = await Ollama.ListLocalModelsAsync();
		var modelsWithBackChoice = models.OrderBy(m => m.Name).Select(m => m.Name).ToList();
		if (modelsWithBackChoice.Count == 1)
		{
			return modelsWithBackChoice[0];
		}
		else
		{
			modelsWithBackChoice.Insert(0, BACK);

			if (!string.IsNullOrEmpty(additionalInformation))
				AnsiConsole.MarkupLine(additionalInformation);

			var answer = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.PageSize(10)
						.Title(prompt)
						.AddChoices(modelsWithBackChoice));

			return answer == BACK ? "" : answer;
		}
	}
}