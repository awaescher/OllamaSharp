using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using OllamaSharp.Constants;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using ChatRole = OllamaSharp.Models.Chat.ChatRole;

namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Provides mapping functionality between OllamaSharp and Microsoft.Extensions.AI models.
/// </summary>
internal static class AbstractionMapper
{
	private static readonly AIJsonSchemaTransformCache _schemaTransformCache = new(new()
	{
		ConvertBooleanSchemas = true,
		DisallowAdditionalProperties = true,
	});

	/// <summary>
	/// Maps a <see cref="ChatRequest"/> and <see cref="ChatDoneResponseStream"/> to a <see cref="ChatResponse"/>.
	/// </summary>
	/// <param name="stream">The response stream with completion data.</param>
	/// <param name="usedModel">The used model. This has to be a separate argument because there might be fallbacks from the calling method.</param>
	/// <returns>A <see cref="ChatResponse"/> object containing the mapped data.</returns>
	public static ChatResponse? ToChatResponse(ChatDoneResponseStream? stream, string? usedModel)
	{
		if (stream is null)
			return null;

		var chatMessage = ToChatMessage(stream.Message);
		chatMessage.CreatedAt = stream.CreatedAt;

		return new ChatResponse(chatMessage)
		{
			FinishReason = ToFinishReason(stream.DoneReason),
			AdditionalProperties = ParseOllamaChatResponseProps(stream),
			CreatedAt = stream.CreatedAt,
			ModelId = usedModel ?? stream.Model,
			RawRepresentation = stream,
			ResponseId = stream.CreatedAtString ?? Guid.NewGuid().ToString("N"),
			Usage = ParseOllamaChatResponseUsage(stream)
		};
	}

	/// <summary>
	/// Converts Microsoft.Extensions.AI <see cref="ChatMessage"/> objects and
	/// an option <see cref="ChatOptions"/> instance to an OllamaSharp <see cref="ChatRequest"/>.
	/// </summary>
	/// <param name="chatClient">The IChatClient being used to make the request.</param>
	/// <param name="messages">A list of chat messages.</param>
	/// <param name="options">Optional chat options to configure the request.</param>
	/// <param name="stream">Indicates if the request should be streamed.</param>
	/// <param name="serializerOptions">Serializer options</param>
	/// <returns>A <see cref="ChatRequest"/> object containing the converted data.</returns>
	public static ChatRequest ToOllamaSharpChatRequest(IChatClient? chatClient, IEnumerable<ChatMessage> messages, ChatOptions? options, bool stream, JsonSerializerOptions serializerOptions)
	{
		object? format = null;

		if (options?.ResponseFormat is ChatResponseFormatJson jsonFormat)
			format = jsonFormat.Schema.HasValue ? jsonFormat.Schema.Value : Application.Json;

		var mappedTools = ToOllamaSharpTools(options?.Tools);
		var mappedMessages = ToOllamaSharpMessages(messages, options, serializerOptions);

		ChatRequest request = chatClient is not null && options?.RawRepresentationFactory?.Invoke(chatClient) is ChatRequest cr ? cr : new();
		request.Format ??= format;
		request.Stream = stream;
		request.Model ??= options?.ModelId ?? string.Empty; // will be set OllamaApiClient.SelectedModel if not set

		request.Options ??= new RequestOptions();
		request.Options.FrequencyPenalty ??= options?.FrequencyPenalty;
		request.Options.PresencePenalty ??= options?.PresencePenalty;
		request.Options.Seed ??= (int?)options?.Seed;
		request.Options.Stop ??= options?.StopSequences?.ToArray();
		request.Options.Temperature ??= options?.Temperature;
		request.Options.TopP ??= options?.TopP;
		request.Options.TopK ??= options?.TopK;
		request.Options.NumPredict = options?.MaxOutputTokens;

		request.Messages =
			request.Messages is null ? mappedMessages :
			mappedMessages is null ? request.Messages :
			[.. request.Messages, .. mappedMessages];

		request.Tools =
			request.Tools is null ? mappedTools :
			mappedTools is null ? request.Tools :
			[.. request.Tools, .. mappedTools];

		var hasAdditionalProperties = options?.AdditionalProperties?.Any() ?? false;
		if (!hasAdditionalProperties)
			return request;

		TryAddOllamaOption<bool?>(options, OllamaOption.F16kv, v => request.Options.F16kv = (bool?)v);
		TryAddOllamaOption<float?>(options, OllamaOption.FrequencyPenalty, v => request.Options.FrequencyPenalty = Convert.ToSingle(v));
		TryAddOllamaOption<bool?>(options, OllamaOption.LogitsAll, v => request.Options.LogitsAll = (bool?)v);
		TryAddOllamaOption<bool?>(options, OllamaOption.LowVram, v => request.Options.LowVram = (bool?)v);
		TryAddOllamaOption<int?>(options, OllamaOption.MainGpu, v => request.Options.MainGpu = Convert.ToInt32(v));
		TryAddOllamaOption<float?>(options, OllamaOption.MinP, v => request.Options.MinP = Convert.ToSingle(v));
		TryAddOllamaOption<int?>(options, OllamaOption.MiroStat, v => request.Options.MiroStat = Convert.ToInt32(v));
		TryAddOllamaOption<float?>(options, OllamaOption.MiroStatEta, v => request.Options.MiroStatEta = Convert.ToSingle(v));
		TryAddOllamaOption<float?>(options, OllamaOption.MiroStatTau, v => request.Options.MiroStatTau = Convert.ToSingle(v));
		TryAddOllamaOption<bool?>(options, OllamaOption.Numa, v => request.Options.Numa = (bool?)v);
		TryAddOllamaOption<int?>(options, OllamaOption.NumBatch, v => request.Options.NumBatch = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumCtx, v => request.Options.NumCtx = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumGpu, v => request.Options.NumGpu = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumGqa, v => request.Options.NumGqa = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumKeep, v => request.Options.NumKeep = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumPredict, v => request.Options.NumPredict = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.MaxOutputTokens, v => request.Options.NumPredict = Convert.ToInt32(v));
		TryAddOllamaOption<int?>(options, OllamaOption.NumThread, v => request.Options.NumThread = Convert.ToInt32(v));
		TryAddOllamaOption<bool?>(options, OllamaOption.PenalizeNewline, v => request.Options.PenalizeNewline = (bool?)v);
		TryAddOllamaOption<float?>(options, OllamaOption.PresencePenalty, v => request.Options.PresencePenalty = Convert.ToSingle(v));
		TryAddOllamaOption<int?>(options, OllamaOption.RepeatLastN, v => request.Options.RepeatLastN = Convert.ToInt32(v));
		TryAddOllamaOption<float?>(options, OllamaOption.RepeatPenalty, v => request.Options.RepeatPenalty = Convert.ToSingle(v));
		TryAddOllamaOption<int?>(options, OllamaOption.Seed, v => request.Options.Seed = Convert.ToInt32(v));
		TryAddOllamaOption<string[]?>(options, OllamaOption.Stop, v => request.Options.Stop = (v as IEnumerable<string>)?.ToArray());
		TryAddOllamaOption<float?>(options, OllamaOption.Temperature, v => request.Options.Temperature = Convert.ToSingle(v));
		TryAddOllamaOption<float?>(options, OllamaOption.TfsZ, v => request.Options.TfsZ = Convert.ToSingle(v));
		TryAddOllamaOption<int?>(options, OllamaOption.TopK, v => request.Options.TopK = Convert.ToInt32(v));
		TryAddOllamaOption<float?>(options, OllamaOption.TopP, v => request.Options.TopP = Convert.ToSingle(v));
		TryAddOllamaOption<float?>(options, OllamaOption.TypicalP, v => request.Options.TypicalP = Convert.ToSingle(v));
		TryAddOllamaOption<bool?>(options, OllamaOption.UseMlock, v => request.Options.UseMlock = (bool?)v);
		TryAddOllamaOption<bool?>(options, OllamaOption.UseMmap, v => request.Options.UseMmap = (bool?)v);
		TryAddOllamaOption<bool?>(options, OllamaOption.VocabOnly, v => request.Options.VocabOnly = (bool?)v);
		TryAddOllamaOption<bool?>(options, OllamaOption.Think, v => request.Think = (bool?)v);
		TryAddOption<string?>(options, Application.KeepAlive, v => request.KeepAlive = (string?)v);

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
		TryAddOption<T>(microsoftChatOptions, option.Name, optionSetter);
	}
	private static void TryAddOption<T>(ChatOptions? microsoftChatOptions, string option, Action<object?> optionSetter)
	{
		if ((microsoftChatOptions?.AdditionalProperties?.TryGetValue(option, out var value) ?? false) && value is not null)
			optionSetter(value);
	}

	/// <summary>
	/// Converts a collection of Microsoft.Extensions.AI.<see cref="AITool"/> to a collection of OllamaSharp tools.
	/// </summary>
	/// <param name="tools">The tools to convert.</param>
	/// <returns>An enumeration of <see cref="Tool"/> objects containing the converted data.</returns>
	private static IEnumerable<object>? ToOllamaSharpTools(IEnumerable<AITool>? tools)
	{
		return tools?.Select(ToOllamaSharpTool).Where(t => t is not null)!;
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="AITool"/> to an OllamaSharp <see cref="Tool" />.
	/// </summary>
	/// <param name="tool">The tool to convert.</param>
	/// <returns>
	/// If parseable, a <see cref="Tool"/> object containing the converted data,
	/// otherwise <see langword="null"/>.
	/// </returns>
	private static object? ToOllamaSharpTool(AITool tool)
	{
		if (tool is AIFunctionDeclaration f)
			return ToOllamaSharpTool(f);

		return null;
	}

	/// <summary>
	/// Converts an <see cref="AIFunctionDeclaration"/> to a <see cref="Tool"/>.
	/// </summary>
	/// <param name="function">The function to convert.</param>
	/// <returns>A <see cref="Tool"/> object containing the converted data.</returns>
	private static Tool ToOllamaSharpTool(AIFunctionDeclaration function)
	{
		JsonElement transformedSchema = _schemaTransformCache.GetOrCreateTransformedSchema(function);
		return new Tool
		{
			Function = new Function
			{
				Description = function.Description,
				Name = function.Name,
				Parameters = JsonSerializer.Deserialize<Parameters>(transformedSchema),
			},
			Type = Application.Function
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
	/// <param name="messages">The chat messages to convert.</param>
	/// <param name="options">The options used with the request.</param>
	/// <param name="serializerOptions">Serializer options</param>
	/// <returns>An enumeration of <see cref="Message"/> objects containing the converted data.</returns>
	private static IEnumerable<Message> ToOllamaSharpMessages(IEnumerable<ChatMessage> messages, ChatOptions? options, JsonSerializerOptions serializerOptions)
	{
		if (options?.Instructions is string instructions && !string.IsNullOrWhiteSpace(instructions))
		{
			yield return new Message
			{
				Content = instructions,
				Role = Application.System,
			};
		}

		foreach (var cm in messages)
		{
			var images = cm.Contents.OfType<DataContent>().Where(dc => dc.HasTopLevelMediaType("image")).Select(ToOllamaImage).ToArray();
			var toolCalls = cm.Contents.OfType<FunctionCallContent>().Select(ToOllamaSharpToolCall).ToArray();

			// Only generates a message if there is text/content, images or tool calls
			if (cm.Text is { Length: > 0 } || images.Length > 0 || toolCalls.Length > 0)
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
					Role = ChatRole.Tool,
				};
			}
		}
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="DataContent"/> to a base64 image string.
	/// </summary>
	/// <param name="content">The data content to convert.</param>
	/// <returns>A string containing the base64 image data.</returns>
	private static string ToOllamaImage(DataContent content)
	{
		return content.Base64Data.ToString();
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
	private static ChatRole ToOllamaSharpRole(Microsoft.Extensions.AI.ChatRole role)
	{
		return role.Value switch
		{
			Application.Assistant => ChatRole.Assistant,
			Application.System => ChatRole.System,
			Application.User => ChatRole.User,
			Application.Tool => ChatRole.Tool,
			_ => new ChatRole(role.Value),
		};
	}

	/// <summary>
	/// Maps an <see cref="OllamaSharp.Models.Chat.ChatRole"/> to a <see cref="Microsoft.Extensions.AI.ChatRole"/>.
	/// </summary>
	/// <param name="role">The chat role to map.</param>
	/// <returns>A <see cref="Microsoft.Extensions.AI.ChatRole"/> object containing the mapped role.</returns>
	private static Microsoft.Extensions.AI.ChatRole ToAbstractionRole(ChatRole? role)
	{
		if (role is null)
			return new Microsoft.Extensions.AI.ChatRole("unknown");

		return role.ToString() switch
		{
			Application.Assistant => Microsoft.Extensions.AI.ChatRole.Assistant,
			Application.System => Microsoft.Extensions.AI.ChatRole.System,
			Application.User => Microsoft.Extensions.AI.ChatRole.User,
			Application.Tool => Microsoft.Extensions.AI.ChatRole.Tool,
			_ => new Microsoft.Extensions.AI.ChatRole(role.ToString()!),
		};
	}

	/// <summary>
	/// Converts a <see cref="ChatResponseStream"/> to a <see cref="ChatResponseUpdate"/>.
	/// </summary>
	/// <param name="response">The response stream to convert.</param>
	/// <param name="responseId">The response ID to store onto the created update.</param>
	/// <returns>A <see cref="ChatResponseUpdate"/> object containing the latest chat completion chunk.</returns>
	public static ChatResponseUpdate ToChatResponseUpdate(ChatResponseStream? response, string responseId)
	{
		var contents = response?.Message is null ? [new TextContent(string.Empty)] : 
			GetAIContentsFromMessage(response.Message);
		
		if (response is ChatDoneResponseStream done)
		{
			contents.Add(new UsageContent(ParseOllamaChatResponseUsage(done)));
			
			return new ChatResponseUpdate(ToAbstractionRole(done.Message.Role), contents)
			{
				CreatedAt = done.CreatedAt,
				FinishReason = done.DoneReason is null ? null : new ChatFinishReason(done.DoneReason),
				RawRepresentation = response,
				ResponseId = responseId,
				ModelId = done.Model
			};
		}
		
		return new ChatResponseUpdate(ToAbstractionRole(response?.Message.Role), contents)
		{
			// no need to set "Contents" as we set the text
			CreatedAt = response?.CreatedAt,
			FinishReason = response?.Done == true ? ChatFinishReason.Stop : null,
			RawRepresentation = response,
			ResponseId = responseId,
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
		return new ChatMessage(ToAbstractionRole(message.Role), GetAIContentsFromMessage(message)) { RawRepresentation = message };
	}

	private static List<AIContent> GetAIContentsFromMessage(Message message)
	{
		var contents = new List<AIContent>();

		if (message is null)
			return contents;

		if (message.ToolCalls?.Any() ?? false)
		{
			foreach (var toolCall in message.ToolCalls)
			{
				if (toolCall.Function is { } function)
				{
					var id = Guid.NewGuid().ToString().Substring(0, 8);
					contents.Add(new FunctionCallContent(id, function.Name ?? Application.NotApplicable, function.Arguments) { RawRepresentation = toolCall });
				}
			}
		}

		if (message.Thinking?.Length > 0)
			contents.Insert(0, new TextReasoningContent(message.Thinking));

		// Ollama frequently sends back empty content with tool calls. Rather than always adding an empty
		// content, we only add the content if either it's not empty or there weren't any tool calls.
		if (message.Content?.Length > 0 || contents.Count == 0)
			contents.Insert(0, new TextContent(message.Content));

		return contents;
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
			[Application.LoadDuration] = TimeSpan.FromMilliseconds(response.LoadDuration / NANOSECONDS_PER_MILLISECOND),
			[Application.TotalDuration] = TimeSpan.FromMilliseconds(response.TotalDuration / NANOSECONDS_PER_MILLISECOND),
			[Application.PromptEvalDuration] = TimeSpan.FromMilliseconds(response.PromptEvalDuration / NANOSECONDS_PER_MILLISECOND),
			[Application.EvalDuration] = TimeSpan.FromMilliseconds(response.EvalDuration / NANOSECONDS_PER_MILLISECOND)
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
			[Application.LoadDuration] = TimeSpan.FromMilliseconds((response.LoadDuration ?? 0) / NANOSECONDS_PER_MILLISECOND),
			[Application.TotalDuration] = TimeSpan.FromMilliseconds((response.TotalDuration ?? 0) / NANOSECONDS_PER_MILLISECOND)
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
			Application.Length => ChatFinishReason.Length,
			Application.Stop => ChatFinishReason.Stop,
			_ => new ChatFinishReason(ollamaDoneReason),
		};
	}

	/// <summary>
	/// Parses usage details from a <see cref="ChatDoneResponseStream"/>.
	/// </summary>
	/// <param name="response">The response to parse.</param>
	/// <returns>A <see cref="UsageDetails"/> object containing the parsed usage details.</returns>
#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
	[return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("response")]
#endif
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
			if (requestProps.TryGetValue(Application.KeepAlive, out string? keepAlive))
				request.KeepAlive = keepAlive;

			if (requestProps.TryGetValue(Application.Truncate, out bool truncate))
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