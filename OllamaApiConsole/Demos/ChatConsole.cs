using OllamaSharp;
using OllamaSharp.Models.Chat;
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
			var keepChatting = true;
			var systemPrompt = ReadMultilineInput("Define a system prompt (optional)");

			do
			{
				AnsiConsole.MarkupLine("");
				AnsiConsole.MarkupLine($"You are talking to [blue]{Ollama.SelectedModel}[/] now.");
				AnsiConsole.MarkupLine("[gray]Submit your messages by hitting return twice.[/]");
				AnsiConsole.MarkupLine("[gray]Type \"[red]/new[/]\" to start over.[/]");
				AnsiConsole.MarkupLine("[gray]Type \"[red]/exit[/]\" to leave the chat.[/]");

				var chat = Ollama.Chat(stream => AnsiConsole.MarkupInterpolated($"[cyan]{stream?.Message.Content ?? ""}[/]"));

				if (!string.IsNullOrEmpty(systemPrompt))
					chat.SetMessages([new Message { Role = ChatRole.System, Content = systemPrompt }]);

				string message;

				do
				{
					AnsiConsole.WriteLine();
					message = ReadMultilineInput();

					if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = false;
						break;
					}

					if (message.Equals("/new", StringComparison.OrdinalIgnoreCase))
					{
						keepChatting = true;
						break;
					}

					await chat.Send(message);

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}
}