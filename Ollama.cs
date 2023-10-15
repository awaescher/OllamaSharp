public class Ollama
{
    public async Task<string> Promt(string prompt)   
    {


        return await Task.FromResult("Answer to " + prompt);
    }

    
}