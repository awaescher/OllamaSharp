using System.Text;
using OllamaSharp;
using Spectre.Console;

namespace OllamaApiConsole;

public abstract class OllamaConsole(IOllamaApiClient ollama)
{
	public IOllamaApiClient Ollama { get; } = ollama ?? throw new ArgumentNullException(nameof(ollama));

	public abstract Task Run();

	public static string ReadInput(string prompt = "", string additionalInformation = "")
	{
		if (!string.IsNullOrEmpty(prompt))
			AnsiConsole.MarkupLine(prompt);
		if (!string.IsNullOrEmpty(additionalInformation))
			AnsiConsole.MarkupLine(additionalInformation);

		return AnsiConsole.Ask<string>("[blue]> [/]");
	}

	public static string ReadMultilineInput(string prompt = "", string additionalInformation = "")
	{
		if (!string.IsNullOrEmpty(prompt))
			AnsiConsole.MarkupLine(prompt);
		if (!string.IsNullOrEmpty(additionalInformation))
			AnsiConsole.MarkupLine(additionalInformation);

		var builder = new StringBuilder();
		var input = "";

		while (!string.IsNullOrEmpty(input) || builder.Length == 0)
		{
			AnsiConsole.Markup("[blue]> [/]");
			input = Console.ReadLine();
			builder.AppendLine(input);
		}

		return builder.ToString().TrimEnd();
	}

	protected async Task<string> SelectModel(string prompt, string additionalInformation = "")
	{
		const string BACK = "..";

		var models = await Ollama.ListLocalModels();
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
