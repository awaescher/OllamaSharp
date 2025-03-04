using FluentAssertions;
using Microsoft.Extensions.Logging;
using OllamaSharp.ModelContextProtocol.Server;
using OllamaSharp.ModelContextProtocol.Tests.Infrastructure;

namespace OllamaSharp.ModelContextProtocol.Tests;

public class ToolsTests
{
	[Test]
	public async Task Throws_Exception_When_Empty_File_Path()
	{
		var action = async () => await Tools.GetFromMcpServers("");
		await action.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'configurationFilePath')");
	}

	[Test]
	public async Task Throws_Exception_When_File_Does_Not_Exists()
	{
		var action = async () => await Tools.GetFromMcpServers("someConfig.txt");
		await action.Should().ThrowAsync<FileNotFoundException>().WithMessage("The specified configuration file 'someConfig.txt' does not exist.");
	}

	[Test]
	public async Task Throws_Exception_When_Server_Are_Empty()
	{
		var action = async () => await Tools.GetFromMcpServers();
		await action.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'mcpServers')");
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
		tools.Should().NotBeEmpty();
		tools.Should().HaveCount(2);

		var tool = tools[0];
		tool.Should().BeOfType<McpClientTool>();
		var clientTools = tool.As<McpClientTool>();
		clientTools.Type.Should().Be("function");
		clientTools.Function.Should().NotBeNull();
		clientTools.Function!.Name.Should().Be("test_for_filesystem");
	}

}
