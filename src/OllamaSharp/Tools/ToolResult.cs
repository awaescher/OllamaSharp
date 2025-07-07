using OllamaSharp.Models.Chat;

namespace OllamaSharp.Tools;

/// <summary>
/// Represents the result of executing a tool function call.
/// </summary>
/// <param name="Tool">The tool that was executed.</param>
/// <param name="ToolCall">The tool call that triggered the execution, or null if not applicable.</param>
/// <param name="Result">The result returned by the tool execution, or null if no result was produced.</param>
public record ToolResult(Tool Tool, Message.ToolCall ToolCall, object? Result);