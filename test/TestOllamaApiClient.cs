#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

using System.Runtime.CompilerServices;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Tests;

/// <summary>
/// Test implementation of <see cref="IOllamaApiClient"/> used for unit testing.
/// </summary>
public class TestOllamaApiClient : IOllamaApiClient
{
	private ChatResponseStream[] _expectedChatResponses = [];
	private GenerateResponseStream[] _expectedGenerateResponses = [];

	/// <inheritdoc/>
	public Uri Uri { get; } = new("http://localhost");

	/// <inheritdoc/>
	public string SelectedModel { get; set; } = string.Empty;

	internal void SetExpectedChatResponses(params ChatResponseStream[] responses)
	{
		_expectedChatResponses = responses;
	}

	internal void SetExpectedGenerateResponses(params GenerateResponseStream[] responses)
	{
		_expectedGenerateResponses = responses;
	}

	/// <inheritdoc/>
	public async IAsyncEnumerable<ChatResponseStream?> ChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedChatResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	/// <inheritdoc/>
	public Task CopyModelAsync(CopyModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<CreateModelResponse?> CreateModelAsync(CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task DeleteModelAsync(DeleteModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<EmbedResponse> EmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public async IAsyncEnumerable<GenerateResponseStream?> GenerateAsync(GenerateRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		foreach (var response in _expectedGenerateResponses)
		{
			await Task.Yield();
			yield return response;
		}
	}

	/// <inheritdoc/>
	public Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task PushBlobAsync(string digest, byte[] bytes, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<bool> IsBlobExistsAsync(string digest, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<IEnumerable<Model>> ListLocalModelsAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<IEnumerable<RunningModel>> ListRunningModelsAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<PullModelResponse?> PullModelAsync(PullModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<PushModelResponse?> PushModelAsync(PushModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<ShowModelResponse> ShowModelAsync(ShowModelRequest request, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}