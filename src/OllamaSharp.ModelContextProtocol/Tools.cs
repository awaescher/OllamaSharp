using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using OllamaSharp.ModelContextProtocol.Server;
using ModelContextProtocolClient = ModelContextProtocol.Client;

namespace OllamaSharp.ModelContextProtocol;

/// <summary>
/// Contains entry point extensions for the Model Context Protocol (MCP) support.
/// </summary>
public static class Tools
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = CreateJsonOptions();

	private static JsonSerializerOptions CreateJsonOptions()
	{
		JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
		options.Converters.Add(new JsonStringEnumConverter());

		return options;
	}

	/// <summary>
	/// Gets the tools from the specified MCP server configuration file.
	/// </summary>
	/// <param name="configurationFilePath">File path to the configuration file.</param>
	/// <param name="clientOptions">The client options to use when connecting to the MCP servers.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public static async Task<Server.McpClientTool[]> GetFromMcpServers(string configurationFilePath, McpClientOptions? clientOptions = null)
	{
		if (string.IsNullOrEmpty(configurationFilePath))
			throw new ArgumentNullException(nameof(configurationFilePath));

		if (!File.Exists(configurationFilePath))
			throw new FileNotFoundException($"The specified configuration file '{configurationFilePath}' does not exist.", configurationFilePath);

		using var fileStream = File.OpenRead(configurationFilePath);
		var configuration = await JsonSerializer.DeserializeAsync<McpServerConfigurationFile>(fileStream, _jsonSerializerOptions)
			?? throw new InvalidOperationException($"Could not read MCP server configuration from '{configurationFilePath}'.");

		return await GetFromMcpServers(clientOptions, configuration.Servers.Select(kvp => kvp.Value.SetNameIfEmpty(kvp.Key)).ToArray());
	}

	/// <summary>
	/// Gets the tools from the specified MCP server configurations.
	/// </summary>
	/// <param name="mcpServers"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static async Task<Server.McpClientTool[]> GetFromMcpServers(params McpServerConfiguration[] mcpServers) => await GetFromMcpServers(null, mcpServers);

	/// <summary>
	/// Gets the tools from the specified MCP server configurations.
	/// </summary>
	/// <param name="mcpServers">List of MCP server configurations</param>
	/// <param name="clientOptions">The client options to use when connecting to the MCP servers.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static async Task<Server.McpClientTool[]> GetFromMcpServers(McpClientOptions? clientOptions, params McpServerConfiguration[] mcpServers)
	{
		if (mcpServers == null || mcpServers.Length == 0)
			throw new ArgumentNullException(nameof(mcpServers));

		var loggerFactory = clientOptions?.LoggerFactory ?? NullLoggerFactory.Instance;
		var options = CreateMcpClientOptions(clientOptions);

		var result = new List<Server.McpClientTool>();
		foreach (var server in mcpServers)
		{
			var clientTransport = clientOptions?.ClientTransportFactoryMethod != null
									? clientOptions.ClientTransportFactoryMethod(server, loggerFactory)
									: ConvertServerConfigurations(server, loggerFactory);

			var client = await ModelContextProtocolClient.McpClientFactory.CreateAsync(clientTransport, options, loggerFactory);
			foreach (var tool in await ModelContextProtocolClient.McpClientExtensions.ListToolsAsync(client))
				result.Add(new Server.McpClientTool(tool, client));
		}

		return result.ToArray();
	}

	private static IClientTransport ConvertServerConfigurations(McpServerConfiguration server, ILoggerFactory loggerFactory)
	{
		if (server.TransportType == McpServerTransportType.Stdio)
		{
			var stdioOptions = new StdioClientTransportOptions
			{
				Command = server.Command,
				Name = server.Name
			};

			if (server.Arguments != null)
				stdioOptions.Arguments = ResolveVariables(server.Arguments);

			if (server.Environment != null)
			{
				stdioOptions.EnvironmentVariables = [];
				foreach (var kvp in server.Environment)
					stdioOptions.EnvironmentVariables[kvp.Key] = GetEnvironmentVariableName(kvp);
			}

			if (server.Options?.TryGetValue("workingDirectory", out var workingDirectory) == true)
				stdioOptions.WorkingDirectory = workingDirectory;

			return new StdioClientTransport(stdioOptions, loggerFactory);
		}

		var sseOptions = new SseClientTransportOptions
		{
			Endpoint = new Uri(server.Command),
			Name = server.Name
		};

		if (server.Environment != null)
		{
			sseOptions.AdditionalHeaders = [];
			foreach (var kvp in server.Environment)
				sseOptions.AdditionalHeaders[kvp.Key] = GetEnvironmentVariableName(kvp);
		}

		return new SseClientTransport(sseOptions, loggerFactory);
	}

	private static string GetEnvironmentVariableName(KeyValuePair<string, string> kvp)
		=> Environment.GetEnvironmentVariable(GetEnvironmentVariableName(kvp.Value)) ?? kvp.Value;

	private static string GetEnvironmentVariableName(string name)
	{
		if (name.StartsWith("${") && name.EndsWith('}'))
			return name.Substring(2, name.Length - 3);

		return name;
	}

	private static string[] ResolveVariables(string[] arguments) => arguments.Select(ResolveVariables).ToArray();

	private static string ResolveVariables(string argument) => Environment.ExpandEnvironmentVariables(argument);

	private static ModelContextProtocolClient.McpClientOptions CreateMcpClientOptions(McpClientOptions? clientOptions)
	{
		return new ModelContextProtocolClient.McpClientOptions
		{
			Capabilities = clientOptions?.Capabilities,
			InitializationTimeout = clientOptions?.InitializationTimeout ?? TimeSpan.FromSeconds(60),
			ClientInfo = new()
			{
				Name = "OllamaSharp",
				Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0",
			}
		};
	}

	private sealed class McpServerConfigurationFile
	{
		[JsonPropertyName("mcpServers")]
		public Dictionary<string, McpServerConfiguration> Servers { get; set; } = [];
	}
}