using System.Security.Cryptography;
using OllamaSharp.Models;

namespace OllamaSharp;

/// <summary>
/// Extension methods to simplify the usage of the <see cref="IOllamaApiClient"/>.
/// </summary>
public static class OllamaApiClientExtensions
{
	/// <summary>
	/// Sends a request to the /api/copy endpoint to copy a model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="source">The name of the existing model to copy.</param>
	/// <param name="destination">The name the copied model should get.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task CopyModelAsync(this IOllamaApiClient client, string source, string destination, CancellationToken cancellationToken = default)
		=> client.CopyModelAsync(new CopyModelRequest { Source = source, Destination = destination }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/delete endpoint to delete a model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="model">The name of the model to delete.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task DeleteModelAsync(this IOllamaApiClient client, string model, CancellationToken cancellationToken = default)
		=> client.DeleteModelAsync(new DeleteModelRequest { Model = model }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="model">The name of the model to pull.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An async enumerable that can be used to iterate over the streamed responses. See <see cref="PullModelResponse"/>.</returns>
	public static IAsyncEnumerable<PullModelResponse?> PullModelAsync(this IOllamaApiClient client, string model, CancellationToken cancellationToken = default)
		=> client.PullModelAsync(new PullModelRequest { Model = model }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/push endpoint to push a new model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="name">The name of the model to push.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An async enumerable that can be used to iterate over the streamed responses. See <see cref="PullModelResponse"/>.</returns>
	public static IAsyncEnumerable<PushModelResponse?> PushModelAsync(this IOllamaApiClient client, string name, CancellationToken cancellationToken = default)
		=> client.PushModelAsync(new PushModelRequest { Model = name, Stream = true }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/embed endpoint to generate embeddings for the currently selected model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="input">The input text to generate embeddings for.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A <see cref="EmbedResponse"/> containing the embeddings.</returns>
	public static Task<EmbedResponse> EmbedAsync(this IOllamaApiClient client, string input, CancellationToken cancellationToken = default)
	{
		var request = new EmbedRequest
		{
			Model = client.SelectedModel,
			Input = [input]
		};
		return client.EmbedAsync(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer that can be used to update the user interface in real-time.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="prompt">The prompt to generate a completion for.</param>
	/// <param name="context">
	/// The context that keeps the conversation for a chat-like history.
	/// Should reuse the result from earlier calls if these calls belong together. Can be null initially.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An async enumerable that can be used to iterate over the streamed responses. See <see cref="GenerateResponseStream"/>.</returns>
	public static IAsyncEnumerable<GenerateResponseStream?> GenerateAsync(this IOllamaApiClient client, string prompt, ConversationContext? context = null, CancellationToken cancellationToken = default)
	{
		var request = new GenerateRequest
		{
			Prompt = prompt,
			Model = client.SelectedModel,
			Stream = true,
			Context = context?.Context
		};
		return client.GenerateAsync(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to /api/generate with <c>keep_alive</c> set to 0 to immediately unload a model from memory.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="model">The name of the model to unload.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A task that completes when the unload request has been sent.</returns>
	public static async Task RequestModelUnloadAsync(this IOllamaApiClient client, string model, CancellationToken cancellationToken = default)
	{
		var request = new GenerateRequest
		{
			Model = client.SelectedModel,
			Stream = false,
			KeepAlive = "0s"
		};
		await foreach (var _ in client.GenerateAsync(request, cancellationToken))
		{
			break;
		}
	}

	/// <summary>
	/// Sends a request to the /api/show endpoint to show the information of a model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="model">The name of the model to get the information for.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ShowModelResponse"/> with the model information.</returns>
	public static Task<ShowModelResponse> ShowModelAsync(this IOllamaApiClient client, string model, CancellationToken cancellationToken = default)
		=> client.ShowModelAsync(new ShowModelRequest { Model = model }, cancellationToken);

	/// <summary>
	/// Pushes a file to the Ollama server to create a "blob" (Binary Large Object).
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="bytes">The byte array containing the file data.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>A task that represents the asynchronous push operation.</returns>
	public static Task PushBlobAsync(this IOllamaApiClient client, byte[] bytes, CancellationToken cancellationToken = default)
		=> client.PushBlobAsync($"sha256:{BitConverter.ToString(SHA256.Create().ComputeHash(bytes)).Replace("-", string.Empty).ToLower()}", bytes, cancellationToken);
}