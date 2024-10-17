using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OllamaSharp.Abstraction;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// See https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI.Ollama/OllamaChatClient.cs
/// </summary>
public static class AbstractionMapper
{
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
			CompletionId = response.CreatedAt,
			CreatedAt = DateTimeOffset.TryParse(response.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdAt) ? createdAt : null,
			ModelId = response.Model ?? request.Model,
			RawRepresentation = response,
			Usage = ParseOllamaChatResponseUsage(response)
		};

		return completion;
	}

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
				Stop = options?.StopSequences.ToArray(),
				Temperature = options?.Temperature,
				TopP = options?.TopP,
			},
			Stream = stream,
			Template = null,
			Tools = ToOllamaSharpTools(options?.Tools)
		};
	}

	private static IEnumerable<Tool> ToOllamaSharpTools(IEnumerable<AITool>? tools)
	{
		return tools?.Select(ToOllamaSharpTool)
			.Where(t => t is not null)
			.Cast<Tool>() ?? [];
	}

	private static Tool? ToOllamaSharpTool(AITool tool)
	{
		if (tool is AIFunction f)
			return ToOllamaSharpTool(f.Metadata);

		return null;
	}

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

	private static string ToFunctionTypeString(Type? _)
	{
		return "string"; // TODO others supported?
	}

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

	public static StreamingChatCompletionUpdate ToStreamingChatCompletionUpdate(ChatResponseStream? response)
	{
		return new StreamingChatCompletionUpdate // TODO
		{
			//AdditionalProperties
			//AuthorName
			//ChoiceIndex
			//CompletionId
			//Contents
			CreatedAt = DateTimeOffset.TryParse(response?.CreatedAt ?? "", CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdAt) ? createdAt : null,
			FinishReason = response?.Done == true ? ChatFinishReason.Stop : null,
			RawRepresentation = response,
			Text = response?.Message?.Content ?? string.Empty,
			Role = ToAbstractionRole(response?.Message?.Role)
		};
	}

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
