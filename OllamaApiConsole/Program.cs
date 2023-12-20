static string ReadInput()
{
	var color = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.Green;

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

Console.WriteLine("Enter the Ollama machine name or endpoint url");
Console.WriteLine("(leave empty for default port on localhost)");

var url = ReadInput();

if (string.IsNullOrWhiteSpace(url))
	url = "http://localhost:11434";

if (!url.StartsWith("http"))
	url = "http://" + url;

if (url.IndexOf(':', 5) < 0)
	url += ":11434";

var uri = new Uri(url);
Console.WriteLine($"Connecting to {uri} ...");

var ollama = new OllamaApiClient(url, "mistral");

//var info = await ollama.ShowModelInformation("codellama");
//await ollama.PullModel("mistral", status => Console.WriteLine($"({status.Percent}%) {status.Status}"));
//await ollama.CopyModel("codellama", "dude");
//await ollama.GenerateEmbeddings("dude", "You are C3PO");
//await ollama.DeleteModel("dude");
//await ollama.PushModel("mattw/pygmalion:latest", status => Console.WriteLine(status.Status));
//await ollama.CreateModel("dude", "no file here", status => Console.WriteLine(status.Status));

/* use images
var imageBytes = await File.ReadAllBytesAsync("myimage.jpg");
await ollama.GenerateCompletion(new GenerateCompletionRequest 
{
	Model = "llava:13b",		// you'll need a multimodal model
	Prompt = "What do you see?",
	Images = new string[] { Convert.ToBase64String(imageBytes) }
}, new ConsoleStreamer());
*/

Console.WriteLine("Loading models ...");

var models = await ollama.ListLocalModels();

var streamer = new ConsoleChatStreamer();

string prompt;

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
			var chosen = models.FirstOrDefault(m => m.Name.Contains(userModelInput.Trim(), StringComparison.OrdinalIgnoreCase));
			if (chosen is object)
				model = chosen.Name;
			else
				Console.WriteLine($"Model {userModelInput} not found");
		}
	}

	Console.WriteLine($"You are talking to {model} now.");

	ollama.SelectedModel = model;

	var chat = ollama.Chat(streamer);

	do
	{
		prompt = ReadInput();

		streamer.Start();
		await chat.Send(prompt);
		streamer.Stop();

		Console.WriteLine();
	} while (!string.IsNullOrEmpty(prompt));
}
else
{
	Console.WriteLine("No models available.");
}

public class ConsoleChatStreamer : IResponseStreamer<ChatResponseStream>
{
	public void Stream(ChatResponseStream stream)
	{
		Console.Write(stream.Message?.Content ?? "");
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