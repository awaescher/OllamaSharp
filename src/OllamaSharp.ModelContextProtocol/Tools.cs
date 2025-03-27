using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;
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
	public static async Task<object[]> GetFromMcpServers(string configurationFilePath, McpClientOptions? clientOptions = null)
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
	public static async Task<object[]> GetFromMcpServers(params McpServerConfiguration[] mcpServers)
		=> await GetFromMcpServers(null, mcpServers);

	/// <summary>
	/// Gets the tools from the specified MCP server configurations.
	/// </summary>
	/// <param name="mcpServers">List of MCP server configurations</param>
	/// <param name="clientOptions">The client options to use when connecting to the MCP servers.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static async Task<object[]> GetFromMcpServers(McpClientOptions? clientOptions, params McpServerConfiguration[] mcpServers)
	{
		if (mcpServers == null || mcpServers.Length == 0)
			throw new ArgumentNullException(nameof(mcpServers));

		var options = CreateMcpClientOptions(clientOptions);
		var servers = ConvertServerConfigurations(mcpServers);

		var result = new List<object>();

		foreach (var server in servers)
		{
			var client = await McpClientFactory.CreateAsync(server, options, clientOptions?.TransportFactoryMethod, clientOptions?.LoggerFactory ?? NullLoggerFactory.Instance);

			await foreach (var tool in client.ListToolsAsync())
				result.Add(new McpClientTool(tool, client));
		}

		return result.ToArray();
	}

	private static List<McpServerConfig> ConvertServerConfigurations(McpServerConfiguration[] mcpServers)
	{
		var result = new List<McpServerConfig>();

		foreach (var server in mcpServers)
		{
			var config = new McpServerConfig
			{
				Id = server.Name ?? Guid.NewGuid().ToString("n"),
				Name = server.Name ?? Guid.NewGuid().ToString("n"),
				TransportType = server.TransportType.ToString(),
				Arguments = ResolveVariables(server.Arguments),
				Location = server.Command,
				TransportOptions = server.Options ?? []
			};

			if (server.Environment != null)
			{
				foreach (var kvp in server.Environment)
					config.TransportOptions[$"env:{kvp.Key}"] = Environment.GetEnvironmentVariable(GetEnvironmentVariableName(kvp.Value)) ?? kvp.Value;
			}

			if (config.Arguments != null)
				config.TransportOptions["arguments"] = string.Join(' ', config.Arguments);

			if (config.TransportOptions?.TryGetValue("workingDirectory", out var dir) == true)
				config.TransportOptions["workingDirectory"] = ResolveVariables(dir);

			result.Add(config);
		}

		return result;
	}

	private static string GetEnvironmentVariableName(string name)
	{
		if (name.StartsWith("${") && name.EndsWith('}'))
			return name.Substring(2, name.Length - 3);

		return name;
	}

	private static string[]? ResolveVariables(string[]? arguments)
	{
		if (arguments == null)
			return null;

		return arguments.Select(a => ResolveVariables(a)).ToArray();
	}

	private static string ResolveVariables(string argument)
	{
		return Environment.ExpandEnvironmentVariables(argument);
	}

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