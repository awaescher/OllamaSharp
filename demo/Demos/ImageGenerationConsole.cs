using OllamaSharp;
using OllamaSharp.Models;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Provides an interactive console for generating images using an Ollama image generation model (experimental).
/// </summary>
/// <param name="ollama">The <see cref="IOllamaApiClient"/> used to communicate with the Ollama service.</param>
public class ImageGenerationConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	/// <inheritdoc/>
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Image Generation (Experimental)").LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"[{WarningTextColor}]This feature is experimental and requires an image generation model (e.g. stable-diffusion).[/]");
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select an image generation model:");

		if (string.IsNullOrEmpty(Ollama.SelectedModel))
			return;

		AnsiConsole.MarkupLine($"You are using [{AccentTextColor}]{Ollama.SelectedModel}[/] for image generation.");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Enter a prompt to generate an image. Type [{AccentTextColor}]{EXIT_COMMAND}[/] to leave.[/]");

		string message;

		do
		{
			AnsiConsole.WriteLine();
			message = ReadInput("Describe the image to generate:");

			if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
				break;

			if (string.IsNullOrWhiteSpace(message))
				continue;

			var width = AnsiConsole.Prompt(
				new TextPrompt<int>($"[{HintTextColor}]Width in pixels[/]")
					.DefaultValue(512)
					.ValidationErrorMessage($"[{ErrorTextColor}]Please enter a valid number[/]"));

			var height = AnsiConsole.Prompt(
				new TextPrompt<int>($"[{HintTextColor}]Height in pixels[/]")
					.DefaultValue(512)
					.ValidationErrorMessage($"[{ErrorTextColor}]Please enter a valid number[/]"));

			var steps = AnsiConsole.Prompt(
				new TextPrompt<int>($"[{HintTextColor}]Diffusion steps[/]")
					.DefaultValue(20)
					.ValidationErrorMessage($"[{ErrorTextColor}]Please enter a valid number[/]"));

			var request = new GenerateRequest
			{
				Model = Ollama.SelectedModel,
				Prompt = message,
				Width = width,
				Height = height,
				Steps = steps,
				Stream = true
			};

			string? lastImage = null;

			await AnsiConsole.Progress()
				.StartAsync(async ctx =>
				{
					var task = ctx.AddTask("Generating image...", maxValue: 100);

					await foreach (var response in Ollama.GenerateAsync(request))
					{
						if (response is null)
							continue;

						if (response.TotalSteps > 0)
							task.MaxValue = response.TotalSteps.Value;

						if (response.CompletedSteps.HasValue)
							task.Value = response.CompletedSteps.Value;

						if (!string.IsNullOrEmpty(response.Image))
							lastImage = response.Image;
					}

					task.Value = task.MaxValue;
				});

			if (!string.IsNullOrEmpty(lastImage))
			{
				try
				{
					var imageBytes = Convert.FromBase64String(lastImage);
					var consoleImage = new CanvasImage(imageBytes);
					consoleImage.MaxWidth = 80;

					AnsiConsole.WriteLine();
					AnsiConsole.Write(consoleImage);
					AnsiConsole.MarkupLine($"[{HintTextColor}]Image rendered above (scaled down for console). Generated at {width}x{height} with {steps} steps.[/]");
				}
				catch (Exception ex)
				{
					AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]Could not render the generated image: {Markup.Escape(ex.Message)}[/]");
				}
			}
			else
			{
				AnsiConsole.MarkupLine($"[{WarningTextColor}]No image was generated. Make sure you are using an image generation model.[/]");
			}
		} while (!string.IsNullOrEmpty(message));
	}
}
