using System.Collections.Generic;
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
	/// <param name="headers">The headers to set on the request message</param>
	/// <param name="ollamaRequest">The request to the Ollama API to get the custom headers from</param>
	public static void ApplyCustomHeaders(this HttpRequestMessage requestMessage, Dictionary<string, string> headers, OllamaRequest? ollamaRequest)
	{
		foreach (var header in headers)
			AddOrUpdateHeaderValue(requestMessage.Headers, header.Key, header.Value);

		if (ollamaRequest != null)
		{
			foreach (var header in ollamaRequest.CustomHeaders)
				AddOrUpdateHeaderValue(requestMessage.Headers, header.Key, header.Value);
		}
	}

	private static void AddOrUpdateHeaderValue(System.Net.Http.Headers.HttpRequestHeaders requestMessageHeaders, string headerKey, string headerValue)
	{
		if (requestMessageHeaders.Contains(headerKey))
			requestMessageHeaders.Remove(headerKey);

		requestMessageHeaders.Add(headerKey, headerValue);
	}
}
