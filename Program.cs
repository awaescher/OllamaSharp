
var prompt = "How is the weather today?";

var ollama = new Ollama();
var answer = await ollama.Promt(prompt);

Console.WriteLine(answer);


var client = new OllamaApiClient("http://localhost:11434");
// var response = await client.GenerateAsync(new GenerateRequest() { Model = "llama2", Prompt = "Hallo?" });

// Console.WriteLine(response.Data);
var r = await client.ListLocalModelsAsync<string>();
Console.Write(r);

await client.GenerateAsync("What is Llama?", "llama2", new ConsoleStreamer());

public class ConsoleStreamer : OllamaApiClient.IResponseStreamer
{
    public void Done()
    {
        
    }

    public void Stream(string response)
    {
        //Console.Write(response);
    }
}
