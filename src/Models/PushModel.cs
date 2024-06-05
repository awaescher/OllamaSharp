using System;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

/// <summary>
/// https://github.com/jmorganca/ollama/blob/main/docs/api.md#push-a-model
/// </summary>
// TODO: Rename to PushModelRequest or similar _____Request to match the other models
public class PushRequest
{
	/// <summary>
	/// Name of the model to push in the form of <namespace>/<model>:<tag>
	/// </summary>
	public class PushRequest
	{
        /// <summary>
        /// Name of the model to push in the form of <namespace>/<model>:<tag> (Obsolete)
        /// </summary>
        [Obsolete("Name is deprecated, see Model")]
        [JsonPropertyName("name")]
		public string? Name { get; set; }

        /// <summary>
        /// Name of the model to push in the form of <namespace>/<model>:<tag> 
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("insecure")]
		public bool Insecure { get; set; }

	/// <summary>
	/// Whether to stream the response
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }
}

/// <summary>
/// The response from the /api/push endpoint
/// </summary>
public class PushStatus
{
	/// <summary>
	/// The status of the push operation
	/// </summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = null!;

	/// <summary>
	/// The hash of the model file
	/// </summary>
	[JsonPropertyName("digest")]
	public string Digest { get; set; } = null!;

	/// <summary>
	/// The total number of bytes to push
	/// </summary>
	[JsonPropertyName("total")]
	public int Total { get; set; }
}