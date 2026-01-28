namespace OllamaSharp.ModelContextProtocol.Server;

/// <summary>
/// Represents the transport type of an MCP server.
/// </summary>
public enum McpServerTransportType
{
	/// <summary>
	/// Represents a standard input/output transport type.
	/// </summary>
	Stdio,

	/// <summary>
	/// Represents a SSE (Server-Sent Events) transport type.
	/// </summary>
	Sse
}
