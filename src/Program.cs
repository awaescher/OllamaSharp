
var client = new OllamaApiClient(new Uri("http://localhost:11434"));
var models = await client.ListLocalModelsAsync();

Console.ForegroundColor = ConsoleColor.White;

var streamer = new ConsoleStreamer();

string prompt;
ConversationContext context = null;

if (models.Any())
{
	var model = models.First().Name;

	if (models.Count() > 0)
	{
		Console.WriteLine("Which model do you want to use? (Press Enter to use the first)");
		foreach (var m in models)
			Console.WriteLine("  " + m.Name);

		Console.Write("> ");
		var chosenModel = Console.ReadLine();
		if (!string.IsNullOrEmpty(chosenModel))
			model = chosenModel;
	}

	Console.WriteLine($"You are talking to {model} now.");

	do
	{
		Console.Write("> ");
		prompt = Console.ReadLine();

		streamer.Start();
		context = await client.GenerateAsync(prompt, model, streamer, context);
		streamer.Stop();

		Console.WriteLine();
	} while (!string.IsNullOrEmpty(prompt));
}
else
{
	Console.WriteLine("No models available.");
}

public class ConsoleStreamer : IResponseStreamer
{
	public void Stream(StreamedResponse response)
	{
		Console.Write(response.Response);
	}

	public void Start()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
	}

	public void Stop()
	{
		Console.ForegroundColor = ConsoleColor.White;
	}
}