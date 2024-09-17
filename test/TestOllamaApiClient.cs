using System.Runtime.CompilerServices;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

public class TestOllamaApiClient : IOllamaApiClient
{
	private ChatResponseStream[] _expectedChatResponses = [];
	private GenerateResponseStream[] _expectedGenerateResponses = [];
	private ChatResponse _expectedChatResponse;

	public string SelectedModel { get; set; } = string.Empty;

	internal void SetExpectedChatResponses(params ChatResponseStream[] responses)
	{
		_expectedChatResponses = responses;
	}

	internal void SetExpectedGenerateResponses(params GenerateResponseStream[] responses)
	{
		_expectedGenerateResponses = responses;
	}

	internal void SetExpectedChatResponse(ChatResponse chatResponse)
	{
		_expectedChatResponse = chatResponse;
	}

	public async IAsyncEnumerable<ChatResponseStream?> Chat(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedChatResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	public Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_expectedChatResponse);
	}

	public Task CopyModel(CopyModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<CreateModelResponse?> CreateModel(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task DeleteModel(DeleteModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<EmbedResponse> Embed(EmbedRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public async IAsyncEnumerable<GenerateResponseStream?> Generate(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedGenerateResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	public Task<Version> GetVersion(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<bool> IsRunning(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Model>> ListLocalModels(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RunningModel>> ListRunningModels(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PullModelResponse?> PullModel(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PushModelResponse?> PushModel(PushModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<ShowModelResponse> ShowModel(ShowModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
}