using OllamaSharp;
using OllamaSharp.Models;
using Spectre.Console;

public class ModelManagerConsole : OllamaConsole
{
	public ModelManagerConsole(IOllamaApiClient ollama)
		: base(ollama)
	{
	}

	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Chat demo").LeftJustified());
		AnsiConsole.WriteLine();

		string command;
		var exit = false;

		do
		{
			command = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.PageSize(10)
					.Title("What do you want to do?")
					.AddChoices(["..", "Copy model", "Create model", "Delete model", "Generate embeddings", "Show model information", "List local models", "Pull model", "Push model"]));

			switch (command)
			{
				case "Copy model":
					await CopyModel();
					break;

				case "Create model":
					await CreateModel();
					break;

				case "Delete model":
					await DeleteModel();
					break;

				case "Generate embeddings":
					await GenerateEmbedding();
					break;

				case "Show model information":
					await ShowModelInformation();
					break;

				case "List local models":
					await ListLocalModels();
					break;

				case "Pull model":
					await PullModel();
					break;

				case "Push model":
					await PushModel();
					break;

				default:
					exit = true;
					break;
			}

			Console.WriteLine();
		} while (!exit);
	}

	private async Task CopyModel()
	{
		var source = await SelectModel("Which model should be copied?");
		if (!string.IsNullOrEmpty(source))
		{
			var destination = ReadInput($"Enter a name for the copy of [blue]{source}[/]:");
			await Ollama.CopyModel(source, destination);
		}
	}

	private async Task CreateModel()
	{
		var createName = ReadInput("Enter a name for your new model:");
		var createModelFileContent = ReadMultilineInput("Enter the contents for the model file:", "[gray]See [/][blue][link]https://ollama.ai/library[/][/][gray] for available models[/]");
		await Ollama.CreateModel(createName, createModelFileContent, status => AnsiConsole.MarkupLineInterpolated($"{status.Status}"));
	}

	private async Task DeleteModel()
	{
		var deleteModel = await SelectModel("Which model do you want to delete?");
		if (!string.IsNullOrEmpty(deleteModel))
			await Ollama.DeleteModel(deleteModel);
	}

	private async Task GenerateEmbedding()
	{
		var embedModel = await SelectModel("Which model should be used to create embeddings?");
		if (!string.IsNullOrEmpty(embedModel))
		{
			var embedContent = ReadInput("Enter a string to to embed:");
			Ollama.SelectedModel = embedModel;
			var embedResponse = await Ollama.GenerateEmbeddings(embedContent);
			AnsiConsole.MarkupLineInterpolated($"[cyan]{string.Join(", ", embedResponse.Embedding)}[/]");
		}
	}

	private async Task ShowModelInformation()
	{
		var infoModel = await SelectModel("Which model do you want to retrieve information for?");
		if (!string.IsNullOrEmpty(infoModel))
		{
			var infoResponse = await Ollama.ShowModelInformation(infoModel);
			PropertyConsoleRenderer.Render(infoResponse);
		}
	}

	private async Task ListLocalModels()
	{
		var models = await Ollama.ListLocalModels();
		foreach (var model in models.OrderBy(m => m.Name))
			AnsiConsole.MarkupLineInterpolated($"[cyan]{model.Name}[/]");
	}

	private async Task PullModel()
	{
		var pullModel = ReadInput("Enter the name of the model you want to pull:", "[gray]See [/][blue][link]https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md[/][/][gray] for reference[/]");

		await AnsiConsole.Progress().StartAsync(async context =>
		{
			ProgressTask? task = null;
			await Ollama.PullModel(pullModel, status => UpdateProgressTaskByStatus(context, ref task, status));
			task?.StopTask();
		});
	}

	private async Task PushModel()
	{
		var pushModel = ReadInput("Which model do you want to push?");
		await Ollama.PushModel("mattw/pygmalion:latest", status => AnsiConsole.MarkupLineInterpolated($"{status.Status}"));
	}

	private void UpdateProgressTaskByStatus(ProgressContext context, ref ProgressTask? task, PullModelResponse modelResponse)
	{
		if (modelResponse.Status != task?.Description)
		{
			task?.StopTask();
			task = context.AddTask(modelResponse.Status);
		}

		task.Increment(modelResponse.Percent - task.Value);
	}

	public static class PropertyConsoleRenderer
	{
		public static void Render(object o)
		{
			foreach (var pi in o.GetType().GetProperties())
			{
				AnsiConsole.MarkupLineInterpolated($"[cyan][underline][bold]{pi.Name}:[/][/][/]");
				AnsiConsole.MarkupLineInterpolated($"[darkcyan]{pi.GetValue(o)?.ToString() ?? ""}[/]");
				AnsiConsole.WriteLine();
			}
		}
	}
}