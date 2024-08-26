using System.Text.RegularExpressions;
using OllamaSharp;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

public partial class ImageChatConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Image chat demo").LeftJustified());
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
				AnsiConsole.MarkupLine("[gray]To send an image, enter its filename in curly braces,[/]");
				AnsiConsole.MarkupLine("[gray]like this {c:/image.jpg}[/]");
				AnsiConsole.MarkupLine("[gray]Submit your messages by hitting return twice.[/]");
				AnsiConsole.MarkupLine("[gray]Type \"[red]/new[/]\" to start over.[/]");
				AnsiConsole.MarkupLine("[gray]Type \"[red]/exit[/]\" to leave the chat.[/]");

				var chat = new Chat(Ollama, systemPrompt);

				string message;

				do
				{
					AnsiConsole.WriteLine();
					message = ReadInput();

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

					var imageMatches = ImagePathRegex().Matches(message).Where(m => !string.IsNullOrEmpty(m.Value));
					var imageCount = imageMatches.Count();
					var hasImages = imageCount > 0;

					if (hasImages)
					{
						byte[][] imageBytes;
						var imagePathsWithCurlyBraces = imageMatches.Select(m => m.Value);
						var imagePaths = imageMatches.Select(m => m.Groups[1].Value);

						try
						{
							imageBytes = imagePaths.Select(File.ReadAllBytes).ToArray();
						}
						catch (IOException ex)
						{
							AnsiConsole.MarkupLineInterpolated($"Could not load your {(imageCount == 1 ? "image" : "images")}:");
							AnsiConsole.MarkupLineInterpolated($"[red]{Markup.Escape(ex.Message)}[/]");
							AnsiConsole.MarkupLine("Please try again");
							continue;
						}

						var imagesBase64 = imageBytes.Select(Convert.ToBase64String);

						foreach (var path in imagePathsWithCurlyBraces)
							message = message.Replace(path, "");

						AnsiConsole.WriteLine();
						AnsiConsole.MarkupLine("[yellow]Image chat will only work with multimodal models like llava![/]");
						AnsiConsole.MarkupLine("[gray]Image paths have been removed from your message, sending this:[/]");
						AnsiConsole.MarkupLineInterpolated($"[silver]{Markup.Escape(message)}[/]");
						AnsiConsole.WriteLine();
						if (imageCount == 1)
							AnsiConsole.MarkupLine("[gray]Here is the image, that is sent to the chat model in addition to your message.[/]");
						else
							AnsiConsole.MarkupLine("[gray]Here are the images, that are sent to the chat model in addition to your message.[/]");
						AnsiConsole.WriteLine();

						foreach (var consoleImage in imageBytes.Select(bytes => new CanvasImage(bytes)))
						{
							consoleImage.MaxWidth = 40;
							AnsiConsole.Write(consoleImage);
						}

						AnsiConsole.WriteLine();
						if (imageCount == 1)
							AnsiConsole.MarkupLine("[gray]The image was scaled down for the console only, the model gets the full version.[/]");
						else
							AnsiConsole.MarkupLine("[gray]The images were scaled down for the console only, the model gets full versions.[/]");
						AnsiConsole.WriteLine();

						await foreach (var answerToken in chat.Send(message, [], imagesBase64))
							AnsiConsole.MarkupInterpolated($"[cyan]{answerToken}[/]");
					}
					else
					{
						await foreach (var answerToken in chat.Send(message))
							AnsiConsole.MarkupInterpolated($"[cyan]{answerToken}[/]");
					}

					AnsiConsole.WriteLine();
				} while (!string.IsNullOrEmpty(message));
			} while (keepChatting);
		}
	}

	[GeneratedRegex("{([^}]*)}")]
	private static partial Regex ImagePathRegex();
}