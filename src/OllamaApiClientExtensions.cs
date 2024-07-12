using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Streamer;

namespace OllamaSharp;

public static class OllamaApiClientExtensions
{
	/// <summary>
	/// Starts a new chat with the currently selected model.
	/// </summary>
	/// <param name="client">The client to start the chat with</param>
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <returns>
	/// A chat instance that can be used to receive and send messages from and to
	/// the Ollama endpoint while maintaining the message history.
	/// </returns>
	public static Chat Chat(this IOllamaApiClient client, Action<ChatResponseStream?> streamer)
	=> client.Chat(new ActionResponseStreamer<ChatResponseStream?>(streamer));

	/// <summary>
	/// Starts a new chat with the currently selected model.
	/// </summary>
	/// <param name="client">The client to start the chat with</param>
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <returns>
	/// A chat instance that can be used to receive and send messages from and to
	/// the Ollama endpoint while maintaining the message history.
	/// </returns>
	public static Chat Chat(this IOllamaApiClient client, IResponseStreamer<ChatResponseStream?> streamer)
	 => new(client, streamer);

	/// <summary>
	/// Sends a request to the /api/chat endpoint
	/// </summary>
	/// <param name="client">The client to start the chat with</param>
	/// <param name="chatRequest">The request to send to Ollama</param>
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>List of the returned messages including the previous context</returns>
	public static Task<IEnumerable<Message>> SendChat(this IOllamaApiClient client, ChatRequest chatRequest, Action<ChatResponseStream?> streamer, CancellationToken cancellationToken = default)
	=> client.SendChat(chatRequest, new ActionResponseStreamer<ChatResponseStream?>(streamer), cancellationToken);

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
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, Action<CreateModelResponse> streamer, CancellationToken cancellationToken = default)
	=> client.CreateModel(name, modelFileContent, new ActionResponseStreamer<CreateModelResponse>(streamer), cancellationToken);

	/// <summary>
	/// Sends a request to the /api/create endpoint to create a model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name for the new model</param>
	/// <param name="modelFileContent">
	/// The file content for the model file the new model should be built with.
	/// See https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md
	/// </param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, IResponseStreamer<CreateModelResponse> streamer, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Stream = true
		};
		return client.CreateModel(request, streamer, cancellationToken);
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
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, Action<CreateModelResponse> streamer, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Path = path,
			Stream = true
		};
		return client.CreateModel(request, new ActionResponseStreamer<CreateModelResponse>(streamer), cancellationToken);
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
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, IResponseStreamer<CreateModelResponse> streamer, CancellationToken cancellationToken = default)
	{
		var request = new CreateModelRequest
		{
			Model = name,
			ModelFileContent = modelFileContent,
			Path = path,
			Stream = true
		};
		return client.CreateModel(request, streamer, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="model">The name of the model to pull</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task PullModel(this IOllamaApiClient client, string model, Action<PullModelResponse> streamer, CancellationToken cancellationToken = default)
	 => client.PullModel(model, new ActionResponseStreamer<PullModelResponse>(streamer), cancellationToken);

	/// <summary>
	/// Sends a request to the /api/pull endpoint to pull a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="model">The name of the model to pull</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task PullModel(this IOllamaApiClient client, string model, IResponseStreamer<PullModelResponse> streamer, CancellationToken cancellationToken = default)
	=> client.PullModel(new PullModelRequest { Model = model }, streamer, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/push endpoint to push a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name of the model to push</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task PushModel(this IOllamaApiClient client, string name, Action<PushModelResponse> streamer, CancellationToken cancellationToken = default)
	=> client.PushModel(name, new ActionResponseStreamer<PushModelResponse>(streamer), cancellationToken);

	/// <summary>
	/// Sends a request to the /api/push endpoint to push a new model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="name">The name of the model to push</param>
	/// <param name="streamer">
	/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the operation is running.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task PushModel(this IOllamaApiClient client, string name, IResponseStreamer<PushModelResponse> streamer, CancellationToken cancellationToken = default)
	=> client.PushModel(new PushModelRequest { Model = name, Stream = true }, streamer, cancellationToken);

	/// <summary>
	/// Sends a request to the /api/embeddings endpoint to generate embeddings for the currently selected model
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="prompt">The prompt to generate embeddings for</param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	public static Task<GenerateEmbeddingResponse> GenerateEmbeddings(this IOllamaApiClient client, string prompt, CancellationToken cancellationToken = default)
	{
		var request = new GenerateEmbeddingRequest
		{
			Model = client.SelectedModel,
			Prompt = prompt
		};
		return client.GenerateEmbeddings(request, cancellationToken);
	}

	/// <summary>
	/// Sends a request to the /api/generate endpoint to get a completion
	/// </summary>
	/// <param name="client">The client used to execute the command</param>
	/// <param name="prompt">The prompt to generate a completion for</param>
	/// <param name="context">
	/// The context that keeps the conversation for a chat-like history.
	/// Should reuse the result from earlier calls if these calls belong together. Can be null initially.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// A context object that holds the conversation history.
	/// Should be reused for further calls to this method to keep a chat going.
	/// </returns>
	public static Task<ConversationContextWithResponse> GetCompletion(this IOllamaApiClient client, string prompt, ConversationContext? context, CancellationToken cancellationToken = default)
	{
		var request = new GenerateCompletionRequest
		{
			Prompt = prompt,
			Model = client.SelectedModel,
			Stream = false,
			Context = context?.Context ?? Array.Empty<long>()
		};
		return client.GetCompletion(request, cancellationToken);
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
	/// <param name="streamer">
	/// The streamer that receives parts of the answer as they are streamed by the Ollama endpoint.
	/// Can be used to update the user interface while the answer is still being generated.
	/// </param>
	/// <param name="cancellationToken">The token to cancel the operation with</param>
	/// <returns>
	/// A context object that holds the conversation history.
	/// Should be reused for further calls to this method to keep a chat going.
	/// </returns>
	public static Task<ConversationContext> StreamCompletion(this IOllamaApiClient client, string prompt, ConversationContext? context, Action<GenerateCompletionResponseStream?> streamer, CancellationToken cancellationToken = default)
	{
		var request = new GenerateCompletionRequest
		{
			Prompt = prompt,
			Model = client.SelectedModel,
			Stream = true,
			Context = context?.Context ?? Array.Empty<long>()
		};
		return client.StreamCompletion(request, new ActionResponseStreamer<GenerateCompletionResponseStream?>(streamer), cancellationToken);
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
	public static IAsyncEnumerable<GenerateCompletionResponseStream?> StreamCompletion(this IOllamaApiClient client, string prompt, ConversationContext? context, CancellationToken cancellationToken = default)
	{
		var request = new GenerateCompletionRequest
		{
			Prompt = prompt,
			Model = client.SelectedModel,
			Stream = true,
			Context = context?.Context ?? Array.Empty<long>()
		};
		return client.StreamCompletion(request, cancellationToken);
	}
}