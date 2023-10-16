static string ReadInput()
{
	var color = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.White;

	try
	{
		Console.Write("> ");
		return Console.ReadLine();
	}
	finally
	{
		Console.ForegroundColor = color;
	}
}

Console.ForegroundColor = ConsoleColor.Gray;

var uri = new Uri("http://localhost:11434");

Console.WriteLine($"Connecting to {uri} ...");

var ollama = new OllamaApiClient(uri);

//var info = await ollama.ShowModelInformation("codellama");
//await ollama.PullModel("mistral", status => Console.WriteLine($"({status.Percent}%) {status.Status}"));
//await ollama.CopyModel("codellama", "dude");
//await ollama.GenerateEmbeddings("dude", "You are C3PO");
//await ollama.DeleteModel("dude");
//await ollama.PushModel("mattw/pygmalion:latest", status => Console.WriteLine(status.Status));
//await ollama.CreateModel("dude", "no file here", status => Console.WriteLine(status.Status));

Console.WriteLine("Loading models ...");

var models = await ollama.ListLocalModels();

var streamer = new ConsoleStreamer();

string prompt;
ConversationContext context = null;

if (models.Any())
{
	Console.Clear();

	var model = models.First().Name;

	if (models.Count() > 1)
	{
		Console.WriteLine("Which model do you want to use?");
		Console.WriteLine("(press Enter to use the first one)");
		foreach (var m in models)
			Console.WriteLine("  " + m.Name);

		var userModelInput = ReadInput();
		if (!string.IsNullOrEmpty(userModelInput))
		{
			var chosen = models.FirstOrDefault(m => m.Name.Equals(userModelInput.Trim(), StringComparison.OrdinalIgnoreCase));
			if (chosen is object)
				model = userModelInput;
			else
				Console.WriteLine($"Model {userModelInput} not found");
		}
	}

	Console.WriteLine($"You are talking to {model} now.");

	do
	{
		prompt = ReadInput();

		streamer.Start();
		
		// stream
		context = await ollama.StreamCompletion(prompt, model, context, streamer);
		// get
		var rc = await ollama.GetCompletion(prompt, model, context);
		
		streamer.Stop();

		Console.WriteLine();
	} while (!string.IsNullOrEmpty(prompt));
}
else
{
	Console.WriteLine("No models available.");
}

public class ConsoleStreamer : IResponseStreamer<GenerateCompletionResponseStream>
{
	public void Stream(GenerateCompletionResponseStream stream)
	{
		Console.Write(stream.Response);
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