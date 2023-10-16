using System.Text.Json.Serialization;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#pull-a-model
/// </summary>
public class PullModelRequest
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("insecure")]
	public bool Insecure { get; set; }
}

public class PullStatus
{
	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("digest")]
	public string Digest { get; set; }

	[JsonPropertyName("total")]
	public long Total { get; set; }

	[JsonPropertyName("completed")]
	public long Completed { get; set; }

	[JsonIgnore]
	public double Percent => Total == 0 ? 100.0 : Completed * 100 / Total;
}