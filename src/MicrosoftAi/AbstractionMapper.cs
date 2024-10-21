using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;

namespace OllamaSharp.MicrosoftAi;

/// <summary>
/// Provides mapping functionality between OllamaSharp and Microsoft.Extensions.AI models.
/// </summary>
public static class AbstractionMapper
{
	/// <summary>
	/// Maps a <see cref="ChatRequest"/> and <see cref="ChatDoneResponseStream"/> to a <see cref="ChatCompletion"/>.
	/// </summary>
	/// <param name="request">The chat request containing initial request data.</param>
	/// <param name="response">The response stream with completion data.</param>
	public static ChatCompletion? ToChatCompletion(ChatRequest request, ChatDoneResponseStream? response)
	{
		if (response is null)
			return null;

		var chatMessage = ToChatMessage(response.Message);
		var completion = new ChatCompletion(chatMessage)
		{
			FinishReason = ToFinishReason(response.DoneReason),
			AdditionalProperties = ParseOllamaChatResponseProps(response),
			Choices = [chatMessage],
			CompletionId = response.CreatedAtString,
			CreatedAt = response.CreatedAt,
			ModelId = response.Model ?? request.Model,
			RawRepresentation = response,
			Usage = ParseOllamaChatResponseUsage(response)
		};

		return completion;
	}

	/// <summary>
	/// Converts Microsoft.Extensions.AI messages and options to an OllamaSharp chat request.
	/// </summary>
	/// <param name="apiClient">The API client used for communication.</param>
	/// <param name="chatMessages">A list of chat messages.</param>
	/// <param name="options">Optional chat options to configure the request.</param>
	/// <param name="stream">Indicates if the request should be streamed.</param>
	public static ChatRequest ToOllamaSharpChatRequest(IOllamaApiClient apiClient, IList<ChatMessage> chatMessages, ChatOptions? options, bool stream)
	{
		// unused ChatOptions properties
		// options.MaxOutputTokens,
		// options.ToolMode,

		return new ChatRequest
		{
			Format = options?.ResponseFormat == ChatResponseFormat.Json ? "json" : null,
			KeepAlive = null,
			Messages = ToOllamaSharpMessages(chatMessages),
			Model = options?.ModelId ?? apiClient.SelectedModel,
			Options = new Models.RequestOptions
			{
				FrequencyPenalty = options?.FrequencyPenalty,
				PresencePenalty = options?.PresencePenalty,
				Stop = options?.StopSequences?.ToArray(),
				Temperature = options?.Temperature,
				TopP = options?.TopP,
			},
			Stream = stream,
			Template = null,
			Tools = ToOllamaSharpTools(options?.Tools)
		};
	}

	/// <summary>
	/// Converts a collection of Microsoft.Extensions.AI.<see cref="AITool"/> to a collection of OllamaSharp tools.
	/// </summary>
	/// <param name="tools">The tools to convert.</param>
	private static IEnumerable<Tool>? ToOllamaSharpTools(IEnumerable<AITool>? tools)
	{
		return tools?.Select(ToOllamaSharpTool)
					 .Where(t => t is not null)
					 .Cast<Tool>();
	}

	/// <summary>
	/// Converts an Microsoft.Extensions.AI.<see cref="AITool"/> to an OllamaSharp tool.
	/// </summary>
	/// <param name="tool">The tool to convert.</param>
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
	private static Tool? ToOllamaSharpTool(AIFunctionMetadata functionMetadata)
	{
		return new Tool
		{
			Function = new Function
			{
				Description = functionMetadata.Description,
				Name = functionMetadata.Name,
				Parameters = new Parameters
				{
					Properties = functionMetadata.Parameters.ToDictionary(p => p.Name, p => new Properties
					{
						Description = p.Description,
						Enum = [], // TODO is there such as possible values in AIFunctionParameterMetadata?
						Type = ToFunctionTypeString(p.ParameterType)
					}),
					Required = functionMetadata.Parameters.Where(p => p.IsRequired).Select(p => p.Name),
					Type = "object"
				}
			},
			Type = "function"
		};
	}

	/// <summary>
	/// Converts a <see cref="Type"/> to a function type string.
	/// </summary>
	/// <param name="_">The type to convert.</param>
	private static string ToFunctionTypeString(Type? _)
	{
		return "string"; // TODO others supported?
	}

	/// <summary>
	/// Converts a list of Microsoft.Extensions.AI.<see cref="ChatMessage"/> to a list of Ollama <see cref="Message"/>.
	/// </summary>
	/// <param name="chatMessages">The chat messages to convert.</param>
	private static IEnumerable<Message> ToOllamaSharpMessages(IList<ChatMessage> chatMessages)
	{
		foreach (var cm in chatMessages)
		{
			yield return new Message
			{
				Content = cm.Text,
				Images = cm.Contents.OfType<DataContent>().Select(ToOllamaImage).Where(s => !string.IsNullOrEmpty(s)).ToArray(),
				Role = ToOllamaSharpRole(cm.Role),
				ToolCalls = cm.Contents.OfType<FunctionCallContent>().Select(ToOllamaSharpToolCall),
			};
		}
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="DataContent"/> to a base64 image string.
	/// </summary>
	/// <param name="content">The data content to convert.</param>
	private static string ToOllamaImage(DataContent content)
	{
		if (content is null || !content.ContainsData)
			return string.Empty;

		if (content.MediaType?.StartsWith("image", StringComparison.OrdinalIgnoreCase) ?? false)
		{
			return content.Data.ToString(); // TODO convert to base64?
		}

		return string.Empty;
	}

	/// <summary>
	/// Converts a Microsoft.Extensions.AI.<see cref="FunctionCallContent"/> to a <see cref="Message.ToolCall"/>.
	/// </summary>
	/// <param name="functionCall">The function call content to convert.</param>
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
	private static Models.Chat.ChatRole ToOllamaSharpRole(Microsoft.Extensions.AI.ChatRole role)
	{
		return role.Value switch
		{
			"assistant" => OllamaSharp.Models.Chat.ChatRole.Assistant,
			"system" => OllamaSharp.Models.Chat.ChatRole.System,
			"user" => OllamaSharp.Models.Chat.ChatRole.User,
			"tool" => OllamaSharp.Models.Chat.ChatRole.Tool,
			_ => new OllamaSharp.Models.Chat.ChatRole(role.Value),
		};
	}

	/// <summary>
	/// Maps an <see cref="OllamaSharp.Models.Chat.ChatRole"/> to a <see cref="Microsoft.Extensions.AI.ChatRole"/>.
	/// </summary>
	/// <param name="role">The chat role to map.</param>
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
	public static StreamingChatCompletionUpdate ToStreamingChatCompletionUpdate(ChatResponseStream? response)
	{
		return new StreamingChatCompletionUpdate // TODO
		{
			//AdditionalProperties
			//AuthorName
			//ChoiceIndex
			//CompletionId
			//Contents
			CreatedAt = response?.CreatedAt,
			FinishReason = response?.Done == true ? ChatFinishReason.Stop : null,
			RawRepresentation = response,
			Text = response?.Message?.Content ?? string.Empty,
			Role = ToAbstractionRole(response?.Message?.Role)
		};
	}

	/// <summary>
	/// Converts a <see cref="Message"/> to a <see cref="ChatMessage"/>.
	/// </summary>
	/// <param name="message">The message to convert.</param>
	private static ChatMessage ToChatMessage(Message message)
	{
		var contents = new List<AIContent>();

		if (message.ToolCalls?.Any() ?? false)
		{
			foreach (var toolCall in message.ToolCalls)
			{
				if (toolCall.Function is { } function)
				{
					var id = Guid.NewGuid().ToString().Substring(0, 8);
					contents.Add(new FunctionCallContent(id, function.Name ?? "", function.Arguments));
				}
			}
		}

		// Ollama frequently sends back empty content with tool calls. Rather than always adding an empty
		// content, we only add the content if either it's not empty or there weren't any tool calls.
		if (message.Content?.Length > 0 || contents.Count == 0)
			contents.Insert(0, new TextContent(message.Content));

		var roleString = message.Role?.ToString() ?? "";
		return new ChatMessage(new Microsoft.Extensions.AI.ChatRole(roleString), contents);
	}

	/// <summary>
	/// Parses additional properties from a <see cref="ChatDoneResponseStream"/>.
	/// </summary>
	/// <param name="response">The response to parse.</param>
	private static AdditionalPropertiesDictionary? ParseOllamaChatResponseProps(ChatDoneResponseStream response)
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
	/// Maps a string representation of a finish reason to a <see cref="ChatFinishReason"/>.
	/// </summary>
	/// <param name="ollamaDoneReason">The finish reason string.</param>
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
		if (response?.PromptEvalCount is not null || response?.EvalCount is not null)
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
}
