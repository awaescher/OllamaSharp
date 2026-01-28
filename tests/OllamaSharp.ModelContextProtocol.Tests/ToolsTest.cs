using Microsoft.Extensions.Logging;
using OllamaSharp.ModelContextProtocol.Server;
using OllamaSharp.ModelContextProtocol.Tests.Infrastructure;
using Shouldly;

namespace OllamaSharp.ModelContextProtocol.Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

/// <summary>
/// Contains unit tests for the <see cref="Tools"/> helper class.
/// </summary>
public class ToolsTests
{
	/// <summary>
	/// Verifies that an <see cref="ArgumentNullException"/> is thrown when an empty configuration file path is supplied.
	/// </summary>
	[Test]
	public async Task Throws_Exception_When_Empty_File_Path()
	{
		var action = async () => await Tools.GetFromMcpServers("");
		var ex = await action.ShouldThrowAsync<ArgumentNullException>();
		ex.Message.ShouldContain("Value cannot be null. (Parameter 'configurationFilePath')");
	}

	/// <summary>
	/// Verifies that a <see cref="FileNotFoundException"/> is thrown when the specified configuration file does not exist.
	/// </summary>
	[Test]
	public async Task Throws_Exception_When_File_Does_Not_Exists()
	{
		var action = async () => await Tools.GetFromMcpServers("someConfig.txt");
		var ex = await action.ShouldThrowAsync<FileNotFoundException>();
		ex.Message.ShouldContain("The specified configuration file 'someConfig.txt' does not exist.");
	}

	/// <summary>
	/// Verifies that an <see cref="ArgumentNullException"/> is thrown when no MCP server list is provided.
	/// </summary>
	[Test]
	public async Task Throws_Exception_When_Server_Are_Empty()
	{
		var action = async () => await Tools.GetFromMcpServers();
		var ex = await action.ShouldThrowAsync<ArgumentNullException>();
		ex.Message.ShouldContain("Value cannot be null. (Parameter 'mcpServers')");
	}

	/// <summary>
	/// Verifies that tools can be read from a valid configuration file.
	/// </summary>
	[Test]
	public async Task Reads_From_File()
	{
		var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

		var options = new McpClientOptions
		{
			LoggerFactory = loggerFactory,
			ClientTransportFactoryMethod = (cfg, _) => new TestClientTransport(cfg.Name!)
		};

		var tools = await Tools.GetFromMcpServers("./TestData/server_config.json", options);
		tools.ShouldNotBeEmpty();
		tools.Length.ShouldBe(2);

		var tool = tools[0];
		tool.ShouldBeOfType<McpClientTool>();
		var clientTools = tool as McpClientTool;
		clientTools.Type.ShouldBe("function");
		clientTools.Function.ShouldNotBeNull();
		clientTools.Function!.Name.ShouldBe("test_for_filesystem");
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.