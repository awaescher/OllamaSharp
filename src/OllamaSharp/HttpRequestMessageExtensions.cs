using System.Net.Http.Headers;
using OllamaSharp.Models;

namespace OllamaSharp;

/// <summary>
/// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
/// </summary>
internal static class HttpRequestMessageExtensions
{
	/// <summary>
	/// Applies custom headers to the <see cref="HttpRequestMessage"/> instance.
	/// </summary>
	/// <param name="requestMessage">The <see cref="HttpRequestMessage"/> to set the headers on.</param>
	/// <param name="headers">A dictionary containing the headers to set on the request message.</param>
	/// <param name="ollamaRequest">An optional <see cref="OllamaRequest"/> to get additional custom headers from.</param>
	public static void ApplyCustomHeaders(this HttpRequestMessage requestMessage, Dictionary<string, string> headers, OllamaRequest? ollamaRequest)
	{
		var concatenated = headers.Concat(ollamaRequest?.CustomHeaders ?? []);
		concatenated.ForEachItem(header => requestMessage.Headers.AddOrUpdateHeaderValue(header.Key, header.Value));
	}

	/// <summary>
	/// Adds or updates a header value in the <see cref="HttpRequestHeaders"/> collection.
	/// </summary>
	/// <param name="requestMessageHeaders">The <see cref="HttpRequestHeaders"/> collection to update.</param>
	/// <param name="headerKey">The key of the header to add or update.</param>
	/// <param name="headerValue">The value of the header to add or update.</param>
	private static void AddOrUpdateHeaderValue(this HttpRequestHeaders requestMessageHeaders, string headerKey, string headerValue)
	{
		requestMessageHeaders.Remove(headerKey);
		requestMessageHeaders.Add(headerKey, headerValue);
	}
}