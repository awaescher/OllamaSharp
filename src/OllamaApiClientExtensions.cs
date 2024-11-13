using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
	/// Sends a request to the /api/create endpoint to create a model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="name">The name for the new model.</param>
	/// <param name="modelFileContent">
	/// The file content for the model file the new model should be built with.
	/// See <see href="https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md"/>.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An async enumerable that can be used to iterate over the streamed responses. See <see cref="CreateModelResponse"/>.</returns>
	public static IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(this IOllamaApiClient client, string name, string modelFileContent, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Stream = true
		};
		return client.CreateModelAsync(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model.
	/// </summary>
	/// <param name="client">The client used to execute the command.</param>
	/// <param name="name">The name for the new model.</param>
	/// <param name="modelFileContent">
	/// The file content for the model file the new model should be built with.
	/// See <see href="https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md"/>.
	/// </param>
	/// <param name="path">The name path to the model file.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An async enumerable that can be used to iterate over the streamed responses. See <see cref="CreateModelResponse"/>.</returns>
	public static IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(this IOllamaApiClient client, string name, string modelFileContent, string path, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Path = path,
			Stream = true
		};
		return client.CreateModelAsync(request, cancellationToken);
	}

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
	/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer
	/// that can be used to update the user interface in real-time.
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
			Context = context?.Context ?? []
		};
		return client.GenerateAsync(request, cancellationToken);
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
}
