using System.Text.Json.Serialization;

namespace OllamaSharp.ModelContextProtocol.Server;

/// <summary>
/// Represents the configuration for an MCP server.
/// </summary>
public class McpServerConfiguration
{
	/// <summary>
	/// Gets or sets the name of the MCP server.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the command to start the MCP server.
	/// </summary>
	public string Command { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the arguments used when the <see cref="Command"/> is executed.
	/// </summary>
	[JsonPropertyName("args")]
	public string[]? Arguments { get; set; }

	/// <summary>
	/// Gets or sets the environment variables used when the <see cref="Command"/> is executed.
	/// </summary>
	[JsonPropertyName("env")]
	public Dictionary<string, string>? Environment { get; set; }

	/// <summary>
	/// Gets or sets the type of transport used to communicate with the server.
	/// </summary>
	public McpServerTransportType TransportType { get; set; } = McpServerTransportType.Stdio;

	/// <summary>
	/// Gets or sets any additional options.
	/// </summary>	
	public Dictionary<string, string>? Options { get; set; }

	/// <summary>
	/// Sets the name of the configuration if it is currently empty.
	/// </summary>
	/// <param name="name">The new name to assign when <see cref="Name"/> is null or empty.</param>
	/// <returns>The current <see cref="McpServerConfiguration"/> instance.</returns>
	public McpServerConfiguration SetNameIfEmpty(string name)
	{
		if (string.IsNullOrEmpty(Name))
			Name = name;

		return this;
	}
}