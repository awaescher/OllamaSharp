using System.Text;
using OllamaSharp;
using Spectre.Console;

namespace OllamaApiConsole;

public abstract class OllamaConsole(IOllamaApiClient ollama)
{
	private const char MULTILINE_OPEN = '[';

	private const char MULTILINE_CLOSE = ']';

	public static string HintTextColor { get; } = "gray";

	public static string AccentTextColor { get; } = "blue";

	public static string WarningTextColor { get; } = "yellow";

	public static string ErrorTextColor { get; } = "red";

	public static string AiTextColor { get; } = "cyan";

	public static string START_NEW_COMMAND { get; } = "/new";

	public static string EXIT_COMMAND { get; } = "/exit";

	public IOllamaApiClient Ollama { get; } = ollama ?? throw new ArgumentNullException(nameof(ollama));

	public abstract Task Run();

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

	protected void WriteChatInstructionHint()
	{
		AnsiConsole.MarkupLine($"[{HintTextColor}]Enter [{AccentTextColor}]{START_NEW_COMMAND}[/] to start over or [{AccentTextColor}]{EXIT_COMMAND}[/] to leave.[/]");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Begin with [{AccentTextColor}]{Markup.Escape(MULTILINE_OPEN.ToString())}[/] to start multiline input. Sumbmit it by ending with [{AccentTextColor}]{Markup.Escape(MULTILINE_CLOSE.ToString())}[/].[/]");
	}

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
