using OllamaApiConsole;
using OllamaApiConsole.Demos;
using OllamaSharp;
using Spectre.Console;

Console.ResetColor();

AnsiConsole.Write(new Rule("OllamaSharp Api Console").LeftJustified());
AnsiConsole.WriteLine();

OllamaApiClient? ollama = null;
var connected = false;

do
{
	AnsiConsole.MarkupLine($"Enter the Ollama [{OllamaConsole.AccentTextColor}]machine name[/] or [{OllamaConsole.AccentTextColor}]endpoint url[/]");

	var url = OllamaConsole.ReadInput();

	if (string.IsNullOrWhiteSpace(url))
		url = "http://localhost:11434";

	if (!url.StartsWith("http"))
		url = "http://" + url;

	if (url.IndexOf(':', 5) < 0)
		url += ":11434";

	var uri = new Uri(url);
	Console.WriteLine($"Connecting to {uri} ...");

	try
	{
		ollama = new OllamaApiClient(url);
		connected = await ollama.IsRunningAsync();

		var models = await ollama.ListLocalModelsAsync();
		if (!models.Any())
			AnsiConsole.MarkupLineInterpolated($"[{OllamaConsole.WarningTextColor}]Your Ollama instance does not provide any models :([/]");
	}
	catch (Exception ex)
	{
		AnsiConsole.MarkupLineInterpolated($"[{OllamaConsole.ErrorTextColor}]{Markup.Escape(ex.Message)}[/]");
		AnsiConsole.WriteLine();
	}
} while (!connected);

string demo;

do
{
	AnsiConsole.Clear();

	demo = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.PageSize(10)
					.Title("What demo do you want to run?")
					.AddChoices("Chat", "Image chat", "Tool chat", "Model manager", "Exit"));

	AnsiConsole.Clear();

	try
	{
		switch (demo)
		{
			case "Chat":
				await new ChatConsole(ollama!).Run();
				break;

			case "Image chat":
				await new ImageChatConsole(ollama!).Run();
				break;

			case "Tool chat":
				await new ToolConsole(ollama!).Run();
				break;

			case "Model manager":
				await new ModelManagerConsole(ollama!).Run();
				break;
		}
	}
	catch (Exception ex)
	{
		AnsiConsole.MarkupLine($"An error occurred. Press [{OllamaConsole.AccentTextColor}]Return[/] to start over.");
		AnsiConsole.MarkupLineInterpolated($"[{OllamaConsole.ErrorTextColor}]{Markup.Escape(ex.Message)}[/]");
		Console.ReadLine();
	}
} while (demo != "Exit");
