using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

namespace OllamaSharp;

/// <summary>
/// Interface for the Ollama API client.
/// </summary>
public interface IOllamaApiClient
{
	/// <summary>
	/// Gets or sets the name of the model to run requests on.
	/// </summary>
	string SelectedModel { get; set; }

	/// <summary>
	/// Sends a request to the /api/chat endpoint and streams the response of the chat.
	/// To implement a fully interactive chat, you should make use of the Chat class with "new Chat(...)"
	/// </summary>
	/// <param name="chatRequest">The request to send to Ollama</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// An asynchronous enumerable that yields ChatResponseStream. Each item
	/// represents a message in the chat response stream. Returns null when the
	/// stream is completed.
	/// </returns>
	/// <remarks>
	/// This is the method to call the Ollama endpoint /api/chat. You might not want to do this manually.
	/// To implement a fully interactive chat, you should make use of the Chat class with "new Chat(...)"
	/// </remarks>
	IAsyncEnumerable<ChatResponseStream?> Chat(ChatRequest chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/copy endpoint to copy a model
	/// </summary>
	/// <param name="request">The parameters required to copy a model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="request">The request object containing the model details</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>An asynchronous enumerable of the model creation status</returns>
	IAsyncEnumerable<CreateModelResponse?> CreateModel(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/delete endpoint to delete a model
	/// </summary>
	/// <param name="model">The name of the model to delete</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task DeleteModel(string model, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/embed endpoint to generate embeddings
	/// </summary>
	/// <param name="request">The parameters to generate embeddings for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<EmbedResponse> Embed(EmbedRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/tags endpoint to get all models that are
	/// available locally
	/// </summary>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<IEnumerable<Model>> ListLocalModels(CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/ps endpoint to get the running models
	/// </summary>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<IEnumerable<RunningModel>> ListRunningModels(CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model
	/// </summary>
	/// <param name="request">The request specifying the model name and whether
	/// to use insecure connection</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// Async enumerable of PullStatus objects representing the status of the
	/// model pull operation
	/// </returns>
	IAsyncEnumerable<PullModelResponse?> PullModel(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Pushes a model to the Ollama API endpoint.
	/// </summary>
	/// <param name="modelRequest">The request containing the model information to
	/// push.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>
	/// An asynchronous enumerable of push status updates. Use the enumerator
	/// to retrieve the push status updates.
	/// </returns>
	IAsyncEnumerable<PushModelResponse?> PushModel(PushModelRequest modelRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/show endpoint to show the information of a
	/// model
	/// </summary>
	/// <param name="model">The name of the model the get the information for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>The model information</returns>
	Task<ShowModelResponse> ShowModel(string model, CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams completion responses from the /api/generate endpoint on the
	/// Ollama API based on the provided request.
	/// </summary>
	/// <param name="request">The request containing the parameters for the completion.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable of completion response streams.</returns>
	IAsyncEnumerable<GenerateResponseStream?> Generate(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a query to check whether the Ollama api is running or not
	/// </summary>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<bool> IsRunning(CancellationToken cancellationToken = default);

	/// <summary>
	/// Get the version of Ollama
	/// </summary>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<Version> GetVersion(CancellationToken cancellationToken = default);
}