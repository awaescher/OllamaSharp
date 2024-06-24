using OllamaSharp;
using Spectre.Console;

public class ChatConsole : OllamaConsole
{
	public ChatConsole(IOllamaApiClient ollama)
		: base(ollama)
	{
	}

	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Chat demo").LeftJustified());
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to chat with:");

		if (!string.IsNullOrEmpty(Ollama.SelectedModel))
		{
			AnsiConsole.MarkupLine($"You are talking to [blue]{Ollama.SelectedModel}[/] now.");
			AnsiConsole.MarkupLine("[gray]Type \"[red]exit[/]\" to leave the chat.[/]");

			var chat = Ollama.Chat(stream => AnsiConsole.MarkupInterpolated($"[cyan]{stream?.Message.Content ?? ""}[/]"));
			string message;

			do
			{
				AnsiConsole.WriteLine();
				message = ReadMultilineInput();

				if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
					break;

				await chat.Send(message);

				AnsiConsole.WriteLine();
			} while (!string.IsNullOrEmpty(message));
		}
	}
}