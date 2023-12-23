using OllamaSharp;
using Spectre.Console;
using System.Runtime.CompilerServices;

Console.ResetColor();

AnsiConsole.Write(new Rule("OllamaSharp Api Console").LeftJustified());
AnsiConsole.WriteLine();

OllamaApiClient ollama;
var connected = false;

do
{
	AnsiConsole.MarkupLine("Enter the Ollama [blue]machine name[/] or [blue]endpoint url[/]");
	AnsiConsole.MarkupLine("[gray]Leave empty for default port on localhost[/]");

	var url = OllamaConsole.ReadInput();

	if (string.IsNullOrWhiteSpace(url))
		url = "http://localhost:11434";

	if (!url.StartsWith("http"))
		url = "http://" + url;

	if (url.IndexOf(':', 5) < 0)
		url += ":11434";

	var uri = new Uri(url);
	Console.WriteLine($"Connecting to {uri} ...");

	ollama = new OllamaApiClient(url);

	try
	{
		var models = await ollama.ListLocalModels();
		if (!models.Any())
			AnsiConsole.MarkupLineInterpolated($"[yellow]Your Ollama instance does not provide any models :([/]");

		connected = true;
	}
	catch (Exception ex)
	{
		AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
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
					.AddChoices(["Chat", "Model manager", "Exit"]));

	AnsiConsole.Clear();

	try
	{
		switch (demo)
		{
			case "Chat":
				await new ChatConsole(ollama).Run();
				break;

			case "Model manager":
				await new ModelManagerConsole(ollama).Run();
				break;
		}
	}
	catch (Exception ex)
	{
		AnsiConsole.MarkupLineInterpolated($"An error occurred. Press [blue]{"[Return]"}[/] to start over.");
		AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
		Console.ReadLine();
	}
} while (demo != "Exit");



/* use images
var imageBytes = await File.ReadAllBytesAsync("myimage.jpg");
await ollama.GenerateCompletion(new GenerateCompletionRequest 
{
	Model = "llava:13b",		// you'll need a multimodal model
	Prompt = "What do you see?",
	Images = new string[] { Convert.ToBase64String(imageBytes) }
}, new ConsoleStreamer());
*/