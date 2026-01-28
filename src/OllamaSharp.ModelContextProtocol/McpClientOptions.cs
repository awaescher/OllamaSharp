using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OllamaSharp.ModelContextProtocol.Server;

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
	public Func<McpServerConfiguration, ILoggerFactory?, IClientTransport>? ClientTransportFactoryMethod { get; set; }

	/// <summary>
	/// Client capabilities to advertise to the server.
	/// </summary>
	public ClientCapabilities? Capabilities { get; init; }

	/// <summary>
	/// Timeout for initialization sequence.
	/// </summary>
	public TimeSpan InitializationTimeout { get; init; } = TimeSpan.FromSeconds(60);
}