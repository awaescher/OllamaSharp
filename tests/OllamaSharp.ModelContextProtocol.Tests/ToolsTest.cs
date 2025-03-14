using Microsoft.Extensions.Logging;
using OllamaSharp.ModelContextProtocol.Server;
using OllamaSharp.ModelContextProtocol.Tests.Infrastructure;
using Shouldly;

namespace OllamaSharp.ModelContextProtocol.Tests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class ToolsTests
{
	[Test]
	public async Task Throws_Exception_When_Empty_File_Path()
	{
		var action = async () => await Tools.GetFromMcpServers("");
		var ex = await action.ShouldThrowAsync<ArgumentNullException>();
		ex.Message.ShouldContain("Value cannot be null. (Parameter 'configurationFilePath')");
	}

	[Test]
	public async Task Throws_Exception_When_File_Does_Not_Exists()
	{
		var action = async () => await Tools.GetFromMcpServers("someConfig.txt");
		var ex = await action.ShouldThrowAsync<FileNotFoundException>();
		ex.Message.ShouldContain("The specified configuration file 'someConfig.txt' does not exist.");
	}

	[Test]
	public async Task Throws_Exception_When_Server_Are_Empty()
	{
		var action = async () => await Tools.GetFromMcpServers();
		var ex = await action.ShouldThrowAsync<ArgumentNullException>();
		ex.Message.ShouldContain("Value cannot be null. (Parameter 'mcpServers')");
	}

	[Test]
	public async Task Reads_From_File()
	{
		var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

		var options = new McpClientOptions
		{
			LoggerFactory = loggerFactory,
			TransportFactoryMethod = config => new TestClientTransport(config),
		};

		var tools = await Tools.GetFromMcpServers("./TestData/server_config.json", options);
		tools.ShouldNotBeEmpty();
		tools.Count().ShouldBe(2);

		var tool = tools[0];
		tool.ShouldBeOfType<McpClientTool>();
		var clientTools = tool as McpClientTool;
		clientTools.Type.ShouldBe("function");
		clientTools.Function.ShouldNotBeNull();
		clientTools.Function!.Name.ShouldBe("test_for_filesystem");
	}

}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
