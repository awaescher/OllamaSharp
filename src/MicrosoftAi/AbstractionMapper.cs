using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Provides mapping functionality between OllamaSharp and Microsoft.Extensions.AI models.
/// </summary>
internal static class AbstractionMapper
{
	/// <summary>
	/// Maps a <see cref="ChatRequest"/> and <see cref="ChatDoneResponseStream"/> to a <see cref="ChatCompletion"/>.
	/// </summary>
	/// <param name="stream">The response stream with completion data.</param>
	/// <param name="usedModel">The used model. This has to be a separate argument because there might be fallbacks from the calling method.</param>
	/// <returns>A <see cref="ChatCompletion"/> object containing the mapped data.</returns>
	public static ChatCompletion? ToChatCompletion(ChatDoneResponseStream? stream, string? usedModel)
	{
		if (stream is null)
			return null;

		var chatMessage = ToChatMessage(stream.Message);

		return new ChatCompletion(chatMessage)
		{
			FinishReason = ToFinishReason(stream.DoneReason),
			AdditionalProperties = ParseOllamaChatResponseProps(stream),
			Choices = [chatMessage],
			CompletionId = stream.CreatedAtString,
			CreatedAt = stream.CreatedAt,
			ModelId = usedModel ?? stream.Model,
			RawRepresentation = stream,
			Usage = ParseOllamaChatResponseUsage(stream)
		};
	}

	/// <summary>
	/// Converts Microsoft.Extensions.AI <see cref="ChatMessage"/> objects and
	/// an option <see cref="ChatOptions"/> instance to an OllamaSharp <see cref="ChatRequest"/>.
	/// </summary>
	/// <param name="chatMessages">A list of chat messages.</param>
	/// <param name="options">Optional chat options to configure the request.</param>
	/// <param name="stream">Indicates if the request should be streamed.</param>
	/// <param name="serializerOptions">Serializer options</param>
	/// <returns>A <see cref="ChatRequest"/> object containing the converted data.</returns>
	public static ChatRequest ToOllamaSharpChatRequest(IList<ChatMessage> chatMessages, ChatOptions? options, bool stream, JsonSerializerOptions serializerOptions)
	{
		var request = new ChatRequest
		{
			Format = Equals(options?.ResponseFormat, ChatResponseFormat.Json) ? "json" : null,
			KeepAlive = null,
			Messages = ToOllamaSharpMessages(chatMessages, serializerOptions),
			Model = options?.ModelId ?? "", // will be set OllamaApiClient.SelectedModel if not set
			Options = new RequestOptions
			{
				FrequencyPenalty = options?.FrequencyPenalty,
				PresencePenalty = options?.PresencePenalty,
				Seed = (int?)options?.Seed,
				Stop = options?.StopSequences?.ToArray(),
				Temperature = options?.Temperature,
				TopP = options?.TopP,
				TopK = options?.TopK,
			},
			Stream = stream,
			Template = null,
			Tools = ToOllamaSharpTools(options?.Tools)
		};

		if (options?.AdditionalProperties?.Any() ?? false)
		{
			TryAddOllamaOption<bool?>(options, OllamaOption.F16kv, v => request.Options.F16kv = (bool?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.FrequencyPenalty, v => request.Options.FrequencyPenalty = (float?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.LogitsAll, v => request.Options.LogitsAll = (bool?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.LowVram, v => request.Options.LowVram = (bool?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.MainGpu, v => request.Options.MainGpu = (int?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.MinP, v => request.Options.MinP = (float?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.MiroStat, v => request.Options.MiroStat = (int?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.MiroStatEta, v => request.Options.MiroStatEta = (float?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.MiroStatTau, v => request.Options.MiroStatTau = (float?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.Numa, v => request.Options.Numa = (bool?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumBatch, v => request.Options.NumBatch = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumCtx, v => request.Options.NumCtx = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumGpu, v => request.Options.NumGpu = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumGqa, v => request.Options.NumGqa = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumKeep, v => request.Options.NumKeep = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumPredict, v => request.Options.NumPredict = (int?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.NumThread, v => request.Options.NumThread = (int?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.PenalizeNewline, v => request.Options.PenalizeNewline = (bool?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.PresencePenalty, v => request.Options.PresencePenalty = (float?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.RepeatLastN, v => request.Options.RepeatLastN = (int?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.RepeatPenalty, v => request.Options.RepeatPenalty = (float?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.Seed, v => request.Options.Seed = (int?)v);
			TryAddOllamaOption<string[]?>(options, OllamaOption.Stop,
				v => request.Options.Stop = (v as IEnumerable<string>)?.ToArray());
			TryAddOllamaOption<float?>(options, OllamaOption.Temperature, v => request.Options.Temperature = (float?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.TfsZ, v => request.Options.TfsZ = (float?)v);
			TryAddOllamaOption<int?>(options, OllamaOption.TopK, v => request.Options.TopK = (int?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.TopP, v => request.Options.TopP = (float?)v);
			TryAddOllamaOption<float?>(options, OllamaOption.TypicalP, v => request.Options.TypicalP = (float?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.UseMlock, v => request.Options.UseMlock = (bool?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.UseMmap, v => request.Options.UseMmap = (bool?)v);
			TryAddOllamaOption<bool?>(options, OllamaOption.VocabOnly, v => request.Options.VocabOnly = (bool?)v);
		}

		return request;
	}

	/// <summary>
	/// Tries to find Ollama options in the additional properties and adds them to the ChatRequest options
	/// </summary>
	/// <typeparam name="T">The type of the option</typeparam>
	/// <param name="microsoftChatOptions">The chat options from the Microsoft abstraction</param>
	/// <param name="option">The Ollama setting to add</param>
	/// <param name="optionSetter">The setter to set the Ollama option if available in the chat options</param>
	private static void TryAddOllamaOption<T>(ChatOptions? microsoftChatOptions, OllamaOption option, Action<object?> optionSetter)
	{
		if ((microsoftChatOptions?.AdditionalProperties?.TryGetValue(option.Name, out var value) ?? false) && value is not null)
			optionSetter(value);
	}

	/// <summary>
	/// Converts a collection of Microsoft.Extensions.AI.<see cref="AITool"/> to a collection of OllamaSharp tools.
	/// </summary>
	/// <param name="tools">The tools to convert.</param>
	/// <returns>An enumeration of <see cref="Tool"/> objects containing the converted data.</returns>
	private static IEnumerable<Tool>? ToOllamaSharpTools(IEnumerable<AITool>? tools)
	{
		return tools?.Select(ToOllamaSharpTool)
					 .Where(t => t is not null)
					 .Cast<Tool>();
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="AITool"/> to an OllamaSharp <see cref="Tool" />.
	/// </summary>
	/// <param name="tool">The tool to convert.</param>
	/// <returns>
	/// If parseable, a <see cref="Tool"/> object containing the converted data,
	/// otherwise <see langword="null"/>.
	/// </returns>
	private static Tool? ToOllamaSharpTool(AITool tool)
	{
		if (tool is AIFunction f)
			return ToOllamaSharpTool(f.Metadata);

		return null;
	}

	/// <summary>
	/// Converts <see cref="AIFunctionMetadata"/> to a <see cref="Tool"/>.
	/// </summary>
	/// <param name="functionMetadata">The function metadata to convert.</param>
	/// <returns>A <see cref="Tool"/> object containing the converted data.</returns>
	private static Tool ToOllamaSharpTool(AIFunctionMetadata functionMetadata)
	{
		return new Tool
		{
			Function = new Function
			{
				Description = functionMetadata.Description,
				Name = functionMetadata.Name,
				Parameters = new Parameters
				{
					Properties = functionMetadata.Parameters.ToDictionary(p => p.Name, p => new Models.Chat.Property
					{
						Description = p.Description,
						Enum = GetPossibleValues(p.Schema as JsonObject),
						Type = ToFunctionTypeString(p.Schema as JsonObject)
					}),
					Required = functionMetadata.Parameters.Where(p => p.IsRequired).Select(p => p.Name),
					Type = "object"
				}
			},
			Type = "function"
		};
	}

	/// <summary>
	/// Converts parameter schema object to a function type string.
	/// </summary>
	/// <param name="schema">The schema object holding schema type information.</param>
	/// <returns>A collection of strings containing the function types.</returns>
	private static IEnumerable<string> GetPossibleValues(JsonObject? schema)
	{
		return []; // TODO others supported?
	}

	/// <summary>
	/// Converts parameter schema object to a function type string.
	/// </summary>
	/// <param name="schema">The schema object holding schema type information.</param>
	/// <returns>A string containing the function type.</returns>
	private static string ToFunctionTypeString(JsonObject? schema)
	{
		return "string"; // TODO others supported?
	}

	/// <summary>
	/// Converts a list of Microsoft.Extensions.AI.<see cref="ChatMessage"/> to a list of Ollama <see cref="Message"/>.
	/// </summary>
	/// <param name="chatMessages">The chat messages to convert.</param>
	/// <param name="serializerOptions">Serializer options</param>
	/// <returns>An enumeration of <see cref="Message"/> objects containing the converted data.</returns>
	private static IEnumerable<Message> ToOllamaSharpMessages(IList<ChatMessage> chatMessages, JsonSerializerOptions serializerOptions)
	{
		foreach (var cm in chatMessages)
		{
			var images = cm.Contents.OfType<ImageContent>().Select(ToOllamaImage).Where(s => !string.IsNullOrEmpty(s)).ToArray();
			var toolCalls = cm.Contents.OfType<FunctionCallContent>().Select(ToOllamaSharpToolCall).ToArray();

			// Only generates a message if there is text/content, images or tool calls
			if (cm.Text is not null || images.Length > 0 || toolCalls.Length > 0)
			{
				yield return new Message
				{
					Content = cm.Text,
					Images = images.Length > 0 ? images : null,
					Role = ToOllamaSharpRole(cm.Role),
					ToolCalls = toolCalls.Length > 0 ? toolCalls : null,
				};
			}

			// If the message contains a function result, add it as a separate tool message
			foreach (var frc in cm.Contents.OfType<FunctionResultContent>())
			{
				var jsonResult = JsonSerializer.SerializeToElement(frc.Result, serializerOptions);

				yield return new Message
				{
					Content = JsonSerializer.Serialize(new OllamaFunctionResultContent
					{
						CallId = frc.CallId,
						Result = jsonResult,
					}, serializerOptions),
					Role = Models.Chat.ChatRole.Tool,
				};
			}
		}
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="ImageContent"/> to a base64 image string.
	/// </summary>
	/// <param name="content">The data content to convert.</param>
	/// <returns>A string containing the base64 image data.</returns>
	private static string ToOllamaImage(ImageContent? content)
	{
		if (content is null)
			return string.Empty;

		if (content.ContainsData && content.Data.HasValue)
			return Convert.ToBase64String(content.Data.Value.ToArray());

		throw new NotSupportedException("Images have to be provided as content (byte-Array or base64-string) for Ollama to be used. Other image sources like links are not supported.");
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="FunctionCallContent"/> to a <see cref="Message.ToolCall"/>.
	/// </summary>
	/// <param name="functionCall">The function call content to convert.</param>
	/// <returns>A <see cref="Message.ToolCall"/> object containing the converted data.</returns>
	private static Message.ToolCall ToOllamaSharpToolCall(FunctionCallContent functionCall)
	{
		return new Message.ToolCall
		{
			Function = new Message.Function
			{
				Arguments = functionCall.Arguments,
				Name = functionCall.Name
			}
		};
	}

	/// <summary>
	/// Maps a <see cref="Microsoft.Extensions.AI.ChatRole"/> to an <see cref="OllamaSharp.Models.Chat.ChatRole"/>.
	/// </summary>
	/// <param name="role">The chat role to map.</param>
	/// <returns>A <see cref="OllamaSharp.Models.Chat.ChatRole"/> object containing the mapped role.</returns>
	private static Models.Chat.ChatRole ToOllamaSharpRole(Microsoft.Extensions.AI.ChatRole role)
	{
		return role.Value switch
		{
			"assistant" => Models.Chat.ChatRole.Assistant,
			"system" => Models.Chat.ChatRole.System,
			"user" => Models.Chat.ChatRole.User,
			"tool" => Models.Chat.ChatRole.Tool,
			_ => new OllamaSharp.Models.Chat.ChatRole(role.Value),
		};
	}

	/// <summary>
	/// Maps an <see cref="OllamaSharp.Models.Chat.ChatRole"/> to a <see cref="Microsoft.Extensions.AI.ChatRole"/>.
	/// </summary>
	/// <param name="role">The chat role to map.</param>
	/// <returns>A <see cref="Microsoft.Extensions.AI.ChatRole"/> object containing the mapped role.</returns>
	private static Microsoft.Extensions.AI.ChatRole ToAbstractionRole(OllamaSharp.Models.Chat.ChatRole? role)
	{
		if (role is null)
			return new Microsoft.Extensions.AI.ChatRole("unknown");

		return role.ToString() switch
		{
			"assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
			"system" => Microsoft.Extensions.AI.ChatRole.System,
			"user" => Microsoft.Extensions.AI.ChatRole.User,
			"tool" => Microsoft.Extensions.AI.ChatRole.Tool,
			_ => new Microsoft.Extensions.AI.ChatRole(role.ToString()),
		};
	}

	/// <summary>
	/// Converts a <see cref="ChatResponseStream"/> to a <see cref="StreamingChatCompletionUpdate"/>.
	/// </summary>
	/// <param name="response">The response stream to convert.</param>
	/// <returns>A <see cref="StreamingChatCompletionUpdate"/> object containing the latest chat completion chunk.</returns>
	public static StreamingChatCompletionUpdate ToStreamingChatCompletionUpdate(ChatResponseStream? response)
	{
		return new StreamingChatCompletionUpdate
		{
			// no need to set "Contents" as we set the text
			CompletionId = response?.CreatedAtString,
			ChoiceIndex = 0, // should be left at 0 as Ollama does not support this
			CreatedAt = response?.CreatedAt,
			FinishReason = response?.Done == true ? ChatFinishReason.Stop : null,
			RawRepresentation = response,
			// TODO: Check if "Message" can ever actually be null. If not, remove the null-coalescing operator
			Text = response?.Message?.Content ?? string.Empty,
			Role = ToAbstractionRole(response?.Message?.Role),
			ModelId = response?.Model
		};
	}

	/// <summary>
	/// Converts a <see cref="Message"/> to a <see cref="ChatMessage"/>.
	/// </summary>
	/// <param name="message">The message to convert.</param>
	/// <returns>A <see cref="ChatMessage"/> object containing the converted data.</returns>
	public static ChatMessage ToChatMessage(Message message)
	{
		var contents = new List<AIContent>();

		if (message.ToolCalls?.Any() ?? false)
		{
			foreach (var toolCall in message.ToolCalls)
			{
				if (toolCall.Function is { } function)
				{
					var id = Guid.NewGuid().ToString().Substring(0, 8);
					contents.Add(new FunctionCallContent(id, function.Name ?? "n/a", function.Arguments) { RawRepresentation = toolCall });
				}
			}
		}

		// Ollama frequently sends back empty content with tool calls. Rather than always adding an empty
		// content, we only add the content if either it's not empty or there weren't any tool calls.
		if (message.Content?.Length > 0 || contents.Count == 0)
			contents.Insert(0, new TextContent(message.Content));

		return new ChatMessage(ToAbstractionRole(message.Role), contents) { RawRepresentation = message };
	}

	/// <summary>
	/// Parses additional properties from a <see cref="ChatDoneResponseStream"/>.
	/// </summary>
	/// <param name="response">The response to parse.</param>
	/// <returns>An <see cref="AdditionalPropertiesDictionary"/> object containing the parsed additional properties.</returns>
	private static AdditionalPropertiesDictionary ParseOllamaChatResponseProps(ChatDoneResponseStream response)
	{
		const double NANOSECONDS_PER_MILLISECOND = 1_000_000;

		return new AdditionalPropertiesDictionary
		{
			["load_duration"] = TimeSpan.FromMilliseconds(response.LoadDuration / NANOSECONDS_PER_MILLISECOND),
			["total_duration"] = TimeSpan.FromMilliseconds(response.TotalDuration / NANOSECONDS_PER_MILLISECOND),
			["prompt_eval_duration"] = TimeSpan.FromMilliseconds(response.PromptEvalDuration / NANOSECONDS_PER_MILLISECOND),
			["eval_duration"] = TimeSpan.FromMilliseconds(response.EvalDuration / NANOSECONDS_PER_MILLISECOND)
		};
	}

	/// <summary>
	/// Parses additional properties from a <see cref="EmbedResponse"/>.
	/// </summary>
	/// <param name="response">The response to parse.</param>
	/// <returns>An <see cref="AdditionalPropertiesDictionary"/> object containing the parsed additional properties.</returns>
	private static AdditionalPropertiesDictionary ParseOllamaEmbedResponseProps(EmbedResponse response)
	{
		const double NANOSECONDS_PER_MILLISECOND = 1_000_000;

		return new AdditionalPropertiesDictionary
		{
			["load_duration"] = TimeSpan.FromMilliseconds((response.LoadDuration ?? 0) / NANOSECONDS_PER_MILLISECOND),
			["total_duration"] = TimeSpan.FromMilliseconds((response.TotalDuration ?? 0) / NANOSECONDS_PER_MILLISECOND)
		};
	}

	/// <summary>
	/// Maps a string representation of a finish reason to a <see cref="ChatFinishReason"/>.
	/// </summary>
	/// <param name="ollamaDoneReason">The finish reason string.</param>
	/// <returns>A <see cref="ChatFinishReason"/> object containing the chat finish reason.</returns>
	private static ChatFinishReason? ToFinishReason(string? ollamaDoneReason)
	{
		return ollamaDoneReason switch
		{
			null => null,
			"length" => ChatFinishReason.Length,
			"stop" => ChatFinishReason.Stop,
			_ => new ChatFinishReason(ollamaDoneReason),
		};
	}

	/// <summary>
	/// Parses usage details from a <see cref="ChatDoneResponseStream"/>.
	/// </summary>
	/// <param name="response">The response to parse.</param>
	/// <returns>A <see cref="UsageDetails"/> object containing the parsed usage details.</returns>
	private static UsageDetails? ParseOllamaChatResponseUsage(ChatDoneResponseStream? response)
	{
		if (response is not null)
		{
			return new()
			{
				InputTokenCount = response.PromptEvalCount,
				OutputTokenCount = response.EvalCount,
				TotalTokenCount = response.PromptEvalCount + response.EvalCount,
			};
		}

		return null;
	}

	/// <summary>
	/// Gets an <see cref="EmbedRequest"/> for the Ollama API.
	/// </summary>
	/// <param name="values">The values to get embeddings for.</param>
	/// <param name="options">The options for the embeddings.</param>
	/// <returns>An <see cref="EmbedRequest"/> object containing the request data.</returns>
	public static EmbedRequest ToOllamaEmbedRequest(IEnumerable<string> values, EmbeddingGenerationOptions? options)
	{
		var request = new EmbedRequest()
		{
			Input = values.ToList(),
			Model = options?.ModelId ?? "" // will be set OllamaApiClient.SelectedModel if not set
		};

		if (options?.AdditionalProperties is { } requestProps)
		{
			if (requestProps.TryGetValue("keep_alive", out long keepAlive))
				request.KeepAlive = keepAlive;

			if (requestProps.TryGetValue("truncate", out bool truncate))
				request.Truncate = truncate;
		}

		return request;
	}

	/// <summary>
	/// Gets Microsoft GeneratedEmbeddings mapped from Ollama embeddings.
	/// </summary>
	/// <param name="ollamaRequest">The original Ollama request that was used to generate the embeddings.</param>
	/// <param name="ollamaResponse">The response from Ollama containing the embeddings.</param>
	/// <param name="usedModel">The used model. This has to be a separate argument because there might be fallbacks from the calling method.</param>
	/// <returns>A <see cref="GeneratedEmbeddings{T}"/> object containing the mapped embeddings.</returns>
	public static GeneratedEmbeddings<Embedding<float>> ToGeneratedEmbeddings(EmbedRequest ollamaRequest, EmbedResponse ollamaResponse, string? usedModel)
	{
		// TODO: Check if this can ever actually be null. If not, remove the null-coalescing operator
		var mapped = (ollamaResponse.Embeddings ?? []).Select(vector => new Embedding<float>(vector)
		{
			CreatedAt = DateTimeOffset.UtcNow,
			ModelId = usedModel ?? ollamaRequest.Model
		});

		return new GeneratedEmbeddings<Embedding<float>>(mapped)
		{
			AdditionalProperties = ParseOllamaEmbedResponseProps(ollamaResponse),
			Usage = new UsageDetails
			{
				InputTokenCount = ollamaResponse.PromptEvalCount,
				TotalTokenCount = ollamaResponse.PromptEvalCount
			}
		};
	}
}