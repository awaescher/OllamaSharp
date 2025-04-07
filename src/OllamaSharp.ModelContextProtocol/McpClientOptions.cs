using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

namespace OllamaSharp.ModelContextProtocol;

/// <summary>
/// Options used for configuring an MCP client.
/// </summary>
public class McpClientOptions
{
	/// <summary>
	/// Logger factory to use for creating clients.
	/// </summary>
	public ILoggerFactory? LoggerFactory { get; set; }

	/// <summary>
	/// An optional factory method which returns transport implementations based on a server configuration.
	/// </summary>
	public Func<McpServerConfig, ILoggerFactory?, IClientTransport>? TransportFactoryMethod { get; set; }

	/// <summary>
	/// An optional factory method which creates a client based on client options and transport implementation.
	/// </summary>
	public Func<IClientTransport, McpServerConfig, McpClientOptions, IMcpClient>? ClientFactoryMethod { get; set; }

	/// <summary>
	/// Client capabilities to advertise to the server.
	/// </summary>
	public ClientCapabilities? Capabilities { get; init; }

	/// <summary>
	/// Timeout for initialization sequence.
	/// </summary>
	public TimeSpan InitializationTimeout { get; init; } = TimeSpan.FromSeconds(60);
}
