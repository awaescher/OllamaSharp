using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using System.Threading;

namespace OllamaSharp
{
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
		public static Chat Chat(this IOllamaApiClient client, Action<ChatResponseStream> streamer)
		{
			return client.Chat(new ActionResponseStreamer<ChatResponseStream>(streamer));
		}

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
		public static Chat Chat(this IOllamaApiClient client, IResponseStreamer<ChatResponseStream> streamer)
		{
			return new Chat(client, streamer);
		}

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
		public static async Task<IEnumerable<Message>> SendChat(this IOllamaApiClient client, ChatRequest chatRequest, Action<ChatResponseStream?> streamer, CancellationToken cancellationToken = default)
		{
			return await client.SendChat(chatRequest, new ActionResponseStreamer<ChatResponseStream?>(streamer), cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/copy endpoint to copy a model
		/// </summary>
		/// <param name="source">The name of the existing model to copy</param>
		/// <param name="destination">The name the copied model should get</param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task CopyModel(this IOllamaApiClient client, string source, string destination, CancellationToken cancellationToken = default)
		{
			await client.CopyModel(new CopyModelRequest { Source = source, Destination = destination }, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
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
		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, Action<CreateStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.CreateModel(name, modelFileContent, new ActionResponseStreamer<CreateStatus>(streamer), cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
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
		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, IResponseStreamer<CreateStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.CreateModel(new CreateModelRequest { Model = name, ModelFileContent = modelFileContent, Stream = true }, streamer, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
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
		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, Action<CreateStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.CreateModel(new CreateModelRequest
			{
				Model = name,
				ModelFileContent = modelFileContent,
				Path = path,
				Stream = true
			}, new ActionResponseStreamer<CreateStatus>(streamer), cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/create endpoint to create a model
		/// </summary>
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
		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, IResponseStreamer<CreateStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.CreateModel(new CreateModelRequest { Model = name, ModelFileContent = modelFileContent, Path = path, Stream = true }, streamer, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/pull endpoint to pull a new model
		/// </summary>
		/// <param name="model">The name of the model to pull</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task PullModel(this IOllamaApiClient client, string model, Action<PullStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.PullModel(model, new ActionResponseStreamer<PullStatus>(streamer), cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/pull endpoint to pull a new model
		/// </summary>
		/// <param name="model">The name of the model to pull</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task PullModel(this IOllamaApiClient client, string model, IResponseStreamer<PullStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.PullModel(new PullModelRequest { Model = model }, streamer, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/push endpoint to push a new model
		/// </summary>
		/// <param name="name">The name of the model to push</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task PushModel(this IOllamaApiClient client, string name, Action<PushStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.PushModel(name, new ActionResponseStreamer<PushStatus>(streamer), cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/push endpoint to push a new model
		/// </summary>
		/// <param name="name">The name of the model to push</param>
		/// <param name="streamer">
		/// The streamer that receives status updates as they are streamed by the Ollama endpoint.
		/// Can be used to update the user interface while the operation is running.
		/// </param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task PushModel(this IOllamaApiClient client, string name, IResponseStreamer<PushStatus> streamer, CancellationToken cancellationToken = default)
		{
			await client.PushModel(new PushRequest { Model = name, Stream = true }, streamer, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/embeddings endpoint to generate embeddings for the currently selected model
		/// </summary>
		/// <param name="prompt">The prompt to generate embeddings for</param>
		/// <param name="cancellationToken">The token to cancel the operation with</param>
		public static async Task<GenerateEmbeddingResponse> GenerateEmbeddings(this IOllamaApiClient client, string prompt, CancellationToken cancellationToken = default)
		{
			return await client.GenerateEmbeddings(new GenerateEmbeddingRequest { Model = client.SelectedModel, Prompt = prompt }, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/generate endpoint to get a completion
		/// </summary>
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
		public static async Task<ConversationContextWithResponse> GetCompletion(this IOllamaApiClient client, string prompt, ConversationContext context, CancellationToken cancellationToken = default)
		{
			var request = new GenerateCompletionRequest
			{
				Prompt = prompt,
				Model = client.SelectedModel,
				Stream = false,
				Context = context?.Context ?? Array.Empty<long>()
			};

			return await client.GetCompletion(request, cancellationToken);
		}

		/// <summary>
		/// Sends a request to the /api/generate endpoint to get a completion and streams the returned chunks to a given streamer
		/// that can be used to update the user interface in real-time.
		/// </summary>
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
		public static async Task<ConversationContext> StreamCompletion(this IOllamaApiClient client, string prompt, ConversationContext context, Action<GenerateCompletionResponseStream> streamer, CancellationToken cancellationToken = default)
		{
			var request = new GenerateCompletionRequest
			{
				Prompt = prompt,
				Model = client.SelectedModel,
				Stream = true,
				Context = context?.Context ?? Array.Empty<long>()
			};

			return await client.StreamCompletion(request, new ActionResponseStreamer<GenerateCompletionResponseStream>(streamer), cancellationToken);
		}

		public static IAsyncEnumerable<GenerateCompletionResponseStream?>
			StreamCompletion(
				this IOllamaApiClient client,
				string prompt,
				ConversationContext context,
				CancellationToken cancellationToken = default)
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
}
