using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#generate-embeddings
/// </summary>
public class GenerateEmbeddingRequest
{
	[JsonPropertyName("model")]
	public string Model { get; set; }

	[JsonPropertyName("prompt")]
	public string Prompt { get; set; }

	[JsonPropertyName("options")]
	public string Options { get; set; }
}

public class GenerateEmbeddingResponse
{
	[JsonPropertyName("embedding")]
	public double[] Embedding { get; set; }
}
