using System.Text.Json;
using ModelContextProtocol.Client;
using OllamaSharp.ModelContextProtocol.Server.Types;
using ModelContextProtocolClient = ModelContextProtocol.Client;

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
	public McpClientTool(ModelContextProtocolClient.McpClientTool mcpTool, IMcpClient client)
	{
		_client = client;

		Function = new OllamaSharp.Models.Chat.Function
		{
			Name = mcpTool.Name,
			Description = mcpTool.Description
		};

		var inputSchema = mcpTool.JsonSchema.Deserialize<JsonSchema>();
		var properties = inputSchema?.Properties;
		if (properties == null)
		{
			return;
		}

		Function.Parameters = new OllamaSharp.Models.Chat.Parameters
		{
			Type = inputSchema!.Type,
			Properties = properties.ToDictionary(kvp => kvp.Key, kvp => new OllamaSharp.Models.Chat.Property
			{
				Type = kvp.Value.Type,
				Description = kvp.Value.Description
			}),
			Required = inputSchema.Required ?? []
		};
	}

	/// <inheritdoc />
	public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
	{
		var arguments = args?.ToDictionary(a => a.Key, a => (object?)(a.Value ?? string.Empty)) ?? [];

		try
		{
			var toolresult = await _client.CallToolAsync(Function!.Name!, arguments);
			var textContent = string.Join('\n', toolresult.Content.Select(c => c.Text));

			if (toolresult.IsError)
				return "Error: " + textContent;

			return textContent;
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}
}