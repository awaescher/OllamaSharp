﻿using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaSharp
{
	public interface IOllamaApiClient
	{
		/// <summary>
		/// Name of the model to run requests on.
		/// </summary>
		string SelectedModel { get; set; }

		/// <summary>
		/// Sends a request to the /api/chat endpoint
		/// </summary>
		/// <param name="chatRequest">The request to send to the AI</param>
		/// <param name="streamer">The streamer for the "ChatGPT effect" where the answer is being streamed into the output</param>
		/// <returns>List of the returned messages including the previous context</returns>
		Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer);

		/// <summary>
		/// Sends a request to the /api/copy endpoint to copy a model
		/// </summary>
		/// <param name="request">The parameters for the copy call</param>
		Task CopyModel(CopyModelRequest request);

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
		/// <param name="request">The paremeters for the model to create</param>
		/// <param name="streamer">The streamer for the status (e.g. a status bar)</param>
		Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/delete endpoint to delete a model
		/// </summary>
		/// <param name="model">The name of the model to delete</param>
		Task DeleteModel(string model);

		/// <summary>
		/// Sends a request to the /api/embeddings endpoint to generate embeddings
		/// </summary>
		Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request);

		/// <summary>
		/// Sends a request to the /api/generate endpoint to get a completion
		/// </summary>
		Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request);

		/// <summary>
		/// Sends a request to the /api/tags endpoint to get all available models
		/// </summary>
		Task<IEnumerable<Model>> ListLocalModels();

		/// <summary>
		/// Sends a request to the /api/pull endpoint to pull a new AI model
		/// </summary>
		/// <param name="request">The request parameters</param>
		/// <param name="streamer">A streamer to display something like a status bar</param>
		Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/push endpoint to push a new AI model
		/// </summary>
		/// <param name="request">The request parameters</param>
		/// <param name="streamer">A streamer to display something like a status bar</param>
		Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer);

		/// <summary>
		/// Sends a request to the /api/show endpoint to show the informations of a model
		/// </summary>
		/// <param name="model">The name of the model</param>
		/// <returns>The model informations</returns>
		Task<ShowModelResponse> ShowModelInformation(string model);

		/// <summary>
		/// Similar to <see cref="GetCompletion"/>, and basically just a "redirect"
		/// </summary>
		Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer);
	}
}
