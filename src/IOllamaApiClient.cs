using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharp
{
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
		/// <returns>List of the returned messages including the previous context</returns>
		Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer);

		/// <summary>
		/// Sends a request to the /api/copy endpoint to copy a model
		/// </summary>
		/// <param name="request">The parameters required to copy a model</param>
		Task CopyModel(CopyModelRequest request);

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
		/// <param name="request">The parameters for the model to create</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/delete endpoint to delete a model
		/// </summary>
		/// <param name="model">The name of the model to delete</param>
		Task DeleteModel(string model);

		/// <summary>
		/// Sends a request to the /api/embeddings endpoint to generate embeddings
		/// </summary>
		/// <param name="request">The parameters to generate embeddings for</param>
		Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request);

		/// <summary>
		/// Sends a request to the /api/generate endpoint to get a completion
		/// </summary>
		/// <param name="request">The parameters to generate a completion</param>
		/// <returns>
		/// A context object that holds the conversation history.
		/// Should be reused for further calls to this method to keep a chat going.
		/// </returns>
		Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request);

		/// <summary>
		/// Sends a request to the /api/tags endpoint to get all models that are available locally
		/// </summary>
		Task<IEnumerable<Model>> ListLocalModels();

		/// <summary>
		/// Sends a request to the /api/pull endpoint to pull a new model
		/// </summary>
		/// <param name="request">The request parameters</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/push endpoint to push a new model
		/// </summary>
		/// <param name="request">The request parameters</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/show endpoint to show the information of a model
		/// </summary>
		/// <param name="model">The name of the model the get the information for</param>
		/// <returns>The model information</returns>
		Task<ShowModelResponse> ShowModelInformation(string model);

		/// <summary>
		/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer
		/// that can be used to update the user interface in real-time.
		/// </summary>
		/// <param name="request">The parameters to generate a completion for</param>
		/// <param name="streamer">
		/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the answer is still being generated.
		/// </param>
		/// <returns>
		/// A context object that holds the conversation history.
		/// Should be reused for further calls to this method to keep a chat going.
		/// </returns>
		Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer);
	}
}