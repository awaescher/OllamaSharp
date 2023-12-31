﻿using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Streamer;

namespace Tests;

public class TestOllamaApiClient : IOllamaApiClient
{
	private ChatRole _role;
	private string _answer;

	public string SelectedModel { get; set; }

	public Task CopyModel(CopyModelRequest request)
	{
		throw new NotImplementedException();
	}

	public Task CreateModel(CreateModelRequest request, IResponseStreamer<CreateStatus> streamer)
	{
		throw new NotImplementedException();
	}

	public Task DeleteModel(string model)
	{
		throw new NotImplementedException();
	}

	public Task<GenerateEmbeddingResponse> GenerateEmbeddings(GenerateEmbeddingRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<ConversationContextWithResponse> GetCompletion(GenerateCompletionRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Model>> ListLocalModels()
	{
		throw new NotImplementedException();
	}

	public Task PullModel(PullModelRequest request, IResponseStreamer<PullStatus> streamer)
	{
		throw new NotImplementedException();
	}

	public Task PushModel(PushRequest request, IResponseStreamer<PushStatus> streamer)
	{
		throw new NotImplementedException();
	}

	public Task<ShowModelResponse> ShowModelInformation(string model)
	{
		throw new NotImplementedException();
	}

	public async Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, Action<ChatResponseStream> streamer)
	{
		var message = new Message { Content = _answer, Role = _role };
		streamer(new ChatResponseStream { Done = true, Message = message, CreatedAt = DateTime.UtcNow.ToString(), Model = chatRequest.Model });

		await Task.Yield();

		var messages = chatRequest.Messages.ToList();
		messages.Add(message);
		return messages;
	}

	public async Task<IEnumerable<Message>> SendChat(ChatRequest chatRequest, IResponseStreamer<ChatResponseStream> streamer)
	{
		var message = new Message { Content = _answer, Role = _role };
		streamer.Stream(new ChatResponseStream { Done = true, Message = message, CreatedAt = DateTime.UtcNow.ToString(), Model = chatRequest.Model });

		await Task.Yield();

		var messages = chatRequest.Messages.ToList();
		messages.Add(message);
		return messages;
	}

	public Task<ConversationContext> StreamCompletion(GenerateCompletionRequest request, IResponseStreamer<GenerateCompletionResponseStream> streamer)
	{
		throw new NotImplementedException();
	}

	internal void DefineChatResponse(ChatRole role, string answer)
	{
		_role = role;
		_answer = answer;
	}
}