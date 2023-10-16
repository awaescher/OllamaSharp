using System.Text.Json.Serialization;

public class GenerateRequest
{
    public string Model { get; set; }
    public string Prompt { get; set; }
    public object Options { get; set; }
    public string System { get; set; }
    public string Template { get; set; }
	
    public long[] Context { get; set; }

    public bool Stream { get; set; }
}

public class CreateRequest
{
    public string Name { get; set; }
    public string Path { get; set; }
}

public class ShowRequest
{
    public string Name { get; set; }
}

public class ShowResponse
{
	[JsonPropertyName("license")]
	public string License { get; set; }

	[JsonPropertyName("modelfile")]
	public string Modelfile { get; set; }

	[JsonPropertyName("parameters")]
	public string Parameters { get; set; }

	[JsonPropertyName("template")]
	public string Template { get; set; }
}

public class CopyRequest
{
    public string Source { get; set; }
    public string Destination { get; set; }
}

public class PullRequest
{
    public string Name { get; set; }
    public bool Insecure { get; set; }
}

public class PushRequest
{
    public string Name { get; set; }
    public bool Insecure { get; set; }
}

public class EmbeddingsRequest
{
    public string Model { get; set; }
    public string Prompt { get; set; }
    public object Options { get; set; }
}
public class EmbeddingsResponse
{
	[JsonPropertyName("embeddings")]
	public List<double> Embeddings { get; set; }
}

