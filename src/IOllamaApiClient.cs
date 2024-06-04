using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using System.Threading;

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
	/// Sends a request to the /api/chat endpoint
	/// </summary>
	/// <param name="chatRequest">The request to send to Ollama</param>
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>List of the returned messages including the previous context</returns>
	Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/chat endpoint and streams the response of the chat.
	/// </summary>
	/// <param name="chatRequest">The request to send to Ollama</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// An asynchronous enumerable that yields ChatResponseStream. Each item represents a message in the chat response stream.
	/// Returns null when the stream is completed.</returns>
	IAsyncEnumerable<ChatResponseStream?> StreamChat(ChatRequest chatRequest, [EnumeratorCancellation] CancellationToken cancellationToken = default);
		
	/// <summary>
	/// Sends a request to the /api/copy endpoint to copy a model
	/// </summary>
	/// <param name="request">The parameters required to copy a model</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="request">The parameters for the model to create</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="request">The request object containing the model details</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>An asynchronous enumerable of the model creation status</returns>
	IAsyncEnumerable<CreateStatus?> CreateModel(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/delete endpoint to delete a model
	/// </summary>
	/// <param name="model">The name of the model to delete</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task DeleteModel(string model, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/embeddings endpoint to generate embeddings
	/// </summary>
	/// <param name="request">The parameters to generate embeddings for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/generate endpoint to get a completion
	/// </summary>
	/// <param name="request">The parameters to generate a completion</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// A context object that holds the conversation history.
	/// Should be reused for further calls to this method to keep a chat going.
	/// </returns>
	Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/tags endpoint to get all models that are available locally
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
	/// <param name="request">The request parameters</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model
	/// </summary>
	/// <param name="request">The request specifying the model name and whether to use insecure connection</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>Async enumerable of PullStatus objects representing the status of the model pull operation</returns>
	IAsyncEnumerable<PullStatus?> PullModel(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/push endpoint to push a new model
	/// </summary>
	/// <param name="request">The request parameters</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer, CancellationToken cancellationToken = default);

	/// <summary>
	/// Pushes a model to the Ollama API endpoint.
	/// </summary>
	/// <param name="request">The request containing the model information to push.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable of push status updates. Use the enumerator to retrieve the push status updates.</returns>
	IAsyncEnumerable<PushStatus?> PushModel(PushRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/show endpoint to show the information of a model
	/// </summary>
	/// <param name="model">The name of the model the get the information for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>The model information</returns>
	Task<ShowModelResponse> ShowModelInformation(string model, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer
	/// that can be used to update the user interface in real-time.
	/// </summary>
	/// <param name="request">The parameters to generate a completion for</param>
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// A context object that holds the conversation history.
	/// Should be reused for further calls to this method to keep a chat going.
	/// </returns>
	Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer, CancellationToken cancellationToken = default);

	/// <summary>
	/// Streams completion responses from the /api/generate endpoint on the
	/// Ollama API based on the provided request.
	/// </summary>
	/// <param name="request">The request containing the parameters for the completion.</param>
	/// <param name="cancellationToken">The token to cancel the operation with.</param>
	/// <returns>An asynchronous enumerable of completion response streams.</returns>
	IAsyncEnumerable<GenerateCompletionResponseStream?> StreamCompletion(GenerateCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default);
		
	/// <summary>
	/// Sends a query to check whether the Ollama api is running or not
	/// </summary>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	Task<bool> IsRunning(CancellationToken cancellationToken = default);
}