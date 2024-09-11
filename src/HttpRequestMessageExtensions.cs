using System.Net.Http;
using OllamaSharp.Models;

namespace OllamaSharp;

/// <summary>
/// Extension methods for the http request message
/// </summary>
public static class HttpRequestMessageExtensions
{
	/// <summary>
	/// Applies default headers from the OllamaApiClient and optional Ollama requests
	/// </summary>
	/// <param name="requestMessage">The http request message to set the headers on</param>
	/// <param name="apiClient">The OllamaApiClient get the default request headers from</param>
	/// <param name="ollamaRequest">The request to the Ollama API to get the custom headers from</param>
	public static void ApplyCustomHeaders(this HttpRequestMessage requestMessage, OllamaApiClient apiClient, OllamaRequest? ollamaRequest)
	{
		foreach (var header in apiClient.DefaultRequestHeaders)
			requestMessage.Headers.Add(header.Key, header.Value);

		if (ollamaRequest != null)
		{
			foreach (var header in ollamaRequest.CustomHeaders)
				requestMessage.Headers.Add(header.Key, header.Value);
		}
	}
}
