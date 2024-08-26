using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;

namespace OllamaSharp;

/// <summary>
/// Extension methods to simplify the usage of the IOllamaApiClient
/// </summary>
public static class OllamaApiClientExtensions
{
	/// <summary>
	/// Sends a request to the /api/copy endpoint to copy a model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="source">The name of the existing model to copy</param>
	/// <param name="destination">The name the copied model should get</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task CopyModel(this IOllamaApiClient client, string source, string destination, CancellationToken cancellationToken = default)
		=> client.CopyModel(new CopyModelRequest { Source = source, Destination = destination }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name for the new model</param>
	/// <param name="modelFileContent">
	/// The file content for the model file the new model should be built with.
	/// See https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static IAsyncEnumerable<CreateModelResponse?> CreateModel(this IOllamaApiClient client, string name, string modelFileContent, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Stream = true
		};
		return client.CreateModel(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name for the new model</param>
	/// <param name="modelFileContent">
	/// The file content for the model file the new model should be built with.
	/// See https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md
	/// </param>
	/// <param name="path">The name path to the model file</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static IAsyncEnumerable<CreateModelResponse?> CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Path = path,
			Stream = true
		};
		return client.CreateModel(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="model">The name of the model to pull</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static IAsyncEnumerable<PullModelResponse?> PullModel(this IOllamaApiClient client, string model, CancellationToken cancellationToken = default)
		=> client.PullModel(new PullModelRequest { Model = model }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/push endpoint to push a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name of the model to push</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static IAsyncEnumerable<PushModelResponse?> PushModel(this IOllamaApiClient client, string name, CancellationToken cancellationToken = default)
		=> client.PushModel(new PushModelRequest { Model = name, Stream = true }, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/embed endpoint to generate embeddings for the currently selected model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="input">The input text to generate embeddings for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task<EmbedResponse> Embed(this IOllamaApiClient client, string input, CancellationToken cancellationToken = default)
	{
		var request = new EmbedRequest
		{
			Model = client.SelectedModel,
			Input = new List<string> { input }
		};
		return client.Embed(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer
	/// that can be used to update the user interface in real-time.
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="prompt">The prompt to generate a completion for</param>
	/// <param name="context">
	/// The context that keeps the conversation for a chat-like history.
	/// Should reuse the result from earlier calls if these calls belong together. Can be null initially.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// An async enumerable that can be used to iterate over the streamed responses.
	/// </returns>
	public static IAsyncEnumerable<GenerateResponseStream?> Generate(this IOllamaApiClient client, string prompt, ConversationContext? context = null, CancellationToken cancellationToken = default)
	{
		var request = new GenerateRequest
		{
			Prompt = prompt,
			Model = client.SelectedModel,
			Stream = true,
			Context = context?.Context ?? Array.Empty<long>()
		};
		return client.Generate(request, cancellationToken);
	}
}