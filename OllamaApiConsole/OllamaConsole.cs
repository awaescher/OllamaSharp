using OllamaSharp;
using Spectre.Console;

public abstract class OllamaConsole
{
	public OllamaConsole(IOllamaApiClient ollama)
	{
		Ollama = ollama ?? throw new ArgumentNullException(nameof(ollama));
	}

	public IOllamaApiClient Ollama { get; }

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

		AnsiConsole.MarkupLineInterpolated($"Type \"[red]-[/]\" and hit [red]{"[Return]"}[/] to submit.");

		string input = "";

		while (!input.TrimEnd().EndsWith('-'))
		{
			AnsiConsole.Markup("[blue]> [/]");
			input += Console.ReadLine() + Environment.NewLine;
		}

		input = input.TrimEnd();

		return input.EndsWith('-') ? input.Substring(0, input.Length - 1) : input;
	}

	protected async Task<string> SelectModel(string prompt, string additionalInformation = "")
	{
		const string back = "..";

		var models = await Ollama.ListLocalModels();
		var modelsWithBackChoice = models.OrderBy(m => m.Name).Select(m => m.Name).ToList();
		modelsWithBackChoice.Insert(0, back);

		if (!string.IsNullOrEmpty(additionalInformation))
			AnsiConsole.MarkupLine(additionalInformation);

		var answer = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.PageSize(10)
					.Title(prompt)
					.AddChoices(modelsWithBackChoice));

		return answer == back ? "" : answer;
	}
}
