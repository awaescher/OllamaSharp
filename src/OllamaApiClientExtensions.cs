﻿using OllamaSharp.Models;
using OllamaSharp.Streamer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharp
{
	public static class OllamaApiClientExtensions
	{
		public static Chat Chat(this IOllamaApiClient client, Action<ChatResponseStream> streamer)
		{
			return client.Chat(new ActionResponseStreamer<ChatResponseStream>(streamer));
		}

		public static Chat Chat(this IOllamaApiClient client, IResponseStreamer<ChatResponseStream> streamer)
		{
			return new Chat(client, streamer);
		}

		public static async Task<IEnumerable<Message>> SendChat(this IOllamaApiClient client, ChatRequest chatRequest, Action<ChatResponseStream> streamer)
		{
			return await client.SendChat(chatRequest, new ActionResponseStreamer<ChatResponseStream>(streamer));
		}

		public static async Task CopyModel(this IOllamaApiClient client, string source, string destination)
		{
			await client.CopyModel(new CopyModelRequest { Source = source, Destination = destination });
		}

		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, Action<CreateStatus> streamer)
		{
			await client.CreateModel(name, modelFileContent, new ActionResponseStreamer<CreateStatus>(streamer));
		}

		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, IResponseStreamer<CreateStatus> streamer)
		{
			await client.CreateModel(new CreateModelRequest { Name = name, ModelFileContent = modelFileContent, Stream = true }, streamer);
		}

		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, Action<CreateStatus> streamer)
		{
			await client.CreateModel(new CreateModelRequest { Name = name, ModelFileContent = modelFileContent, Path = path, Stream = true }, new ActionResponseStreamer<CreateStatus>(streamer));
		}

		public static async Task CreateModel(this IOllamaApiClient client, string name, string modelFileContent, string path, IResponseStreamer<CreateStatus> streamer)
		{
			await client.CreateModel(new CreateModelRequest { Name = name, ModelFileContent = modelFileContent, Path = path, Stream = true }, streamer);
		}

		public static async Task PullModel(this IOllamaApiClient client, string model, Action<PullStatus> streamer)
		{
			await client.PullModel(model, new ActionResponseStreamer<PullStatus>(streamer));
		}

		public static async Task PullModel(this IOllamaApiClient client, string model, IResponseStreamer<PullStatus> streamer)
		{
			await client.PullModel(new PullModelRequest { Name = model }, streamer);
		}

		public static async Task PushModel(this IOllamaApiClient client, string name, Action<PushStatus> streamer)
		{
			await client.PushModel(name, new ActionResponseStreamer<PushStatus>(streamer));
		}

		public static async Task PushModel(this IOllamaApiClient client, string name, IResponseStreamer<PushStatus> streamer)
		{
			await client.PushModel(new PushRequest { Name = name, Stream = true }, streamer);
		}

		public static async Task<GenerateEmbeddingResponse> GenerateEmbeddings(this IOllamaApiClient client, string prompt)
		{
			return await client.GenerateEmbeddings(new GenerateEmbeddingRequest { Model = client.SelectedModel, Prompt = prompt });
		}

		public static async Task<ConversationContext> StreamCompletion(this IOllamaApiClient client, string prompt, ConversationContext context, Action<GenerateCompletionResponseStream> streamer)
		{
			var request = new GenerateCompletionRequest
			{
				Prompt = prompt,
				Model = client.SelectedModel,
				Stream = true,
				Context = context?.Context ?? Array.Empty<long>()
			};

			return await client.StreamCompletion(request, new ActionResponseStreamer<GenerateCompletionResponseStream>(streamer));
		}

		public static async Task<ConversationContextWithResponse> GetCompletion(this IOllamaApiClient client, string prompt, ConversationContext context)
		{
			var request = new GenerateCompletionRequest
			{
				Prompt = prompt,
				Model = client.SelectedModel,
				Stream = false,
				Context = context?.Context ?? Array.Empty<long>()
			};

			return await client.GetCompletion(request);
		}
	}
}
