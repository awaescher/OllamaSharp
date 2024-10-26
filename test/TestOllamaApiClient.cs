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

	public Uri Uri { get; } = new("http://localhost");

	public string SelectedModel { get; set; } = string.Empty;

	internal void SetExpectedChatResponses(params ChatResponseStream[] responses)
	{
		_expectedChatResponses = responses;
	}

	internal void SetExpectedGenerateResponses(params GenerateResponseStream[] responses)
	{
		_expectedGenerateResponses = responses;
	}

	public async IAsyncEnumerable<ChatResponseStream?> ChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedChatResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	public Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<EmbedResponse> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public async IAsyncEnumerable<GenerateResponseStream?> GenerateAsync(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedGenerateResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	public Task<Version> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Model>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RunningModel>> ListRunningModelsAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PullModelResponse?> PullModelAsync(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PushModelResponse?> PushModelAsync(PushModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<ShowModelResponse> ShowModelAsync(ShowModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
}