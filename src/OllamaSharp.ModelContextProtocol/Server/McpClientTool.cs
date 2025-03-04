
using McpDotNet.Client;

namespace OllamaSharp.ModelContextProtocol.Server;

/// <summary>
/// Represents a tool to interact with an MCP server.
/// </summary>
public class McpClientTool : OllamaSharp.Models.Chat.Tool, OllamaSharp.Tools.IAsyncInvokableTool
{
	private readonly IMcpClient _client;

	/// <summary>
	/// Initializes a new instance with metadata about the original method.
	/// </summary>
	public McpClientTool(McpDotNet.Protocol.Types.Tool mcpTool, IMcpClient client)
	{
		_client = client;

		this.Function = new OllamaSharp.Models.Chat.Function
		{
			Name = mcpTool.Name,
			Description = mcpTool.Description
		};

		var properties = mcpTool.InputSchema?.Properties;
		if (properties == null)
			return;

		this.Function.Parameters = new OllamaSharp.Models.Chat.Parameters
		{
			Type = mcpTool.InputSchema!.Type,
			Properties = properties.ToDictionary(kvp => kvp.Key, kvp => new OllamaSharp.Models.Chat.Property
			{
				Type = kvp.Value.Type,
				Description = kvp.Value.Description
			}),
			Required = mcpTool.InputSchema?.Required ?? []
		};
	}

	/// <inheritdoc />
	public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
	{
		var arguments = args?.ToDictionary(a => a.Key, a => a.Value ?? string.Empty) ?? [];

		var toolresult = await _client.CallToolAsync(Function!.Name!, arguments);

		if (toolresult.IsError)
			return null;

		return toolresult.Content;
	}
}
