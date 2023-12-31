using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharp
{
	public interface IOllamaApiClient
	{
		string SelectedModel { get; set; }

		Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer);

		Task CopyModel(CopyModelRequest request);

		Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer);

		Task DeleteModel(string model);

		Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request);

		Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request);

		Task<IEnumerable<Model>> ListLocalModels();

		Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer);

		Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer);

		Task<ShowModelResponse> ShowModelInformation(string model);

		Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer);
	}
}