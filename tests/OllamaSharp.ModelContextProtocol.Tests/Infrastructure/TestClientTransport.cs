using System.Text.Json;
using System.Threading.Channels;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using Moq;
using ModelContextProtocolTypes = ModelContextProtocol.Protocol.Types;

namespace OllamaSharp.ModelContextProtocol.Tests.Infrastructure;

internal class TestClientTransport : IClientTransport
{
	private readonly Channel<IJsonRpcMessage> _messageChannel;

	public TestClientTransport(McpServerConfig config)
	{
		Config = config;

		_messageChannel = Channel.CreateUnbounded<IJsonRpcMessage>(new UnboundedChannelOptions
		{
			SingleReader = true,
			SingleWriter = true,
		});
	}

	public bool IsConnected => true;

	public ChannelReader<IJsonRpcMessage> MessageReader => _messageChannel.Reader;

	public McpServerConfig Config { get; }

	public Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(Mock.Of<ITransport>());

	public ValueTask DisposeAsync()
		=> ValueTask.CompletedTask;

	public async Task SendMessageAsync(IJsonRpcMessage message, CancellationToken cancellationToken = default)
	{
		if (message is JsonRpcRequest request)
		{
			if (request.Method == "initialize")
				await Initialize(request, cancellationToken);
			else if (request.Method == "tools/list")
				await ListTools(request, cancellationToken);
		}
	}

	private async Task ListTools(JsonRpcRequest request, CancellationToken cancellationToken)
	{
		await WriteMessageAsync(new JsonRpcResponse
		{
			Id = request.Id,
			Result = JsonSerializer.SerializeToNode(new ModelContextProtocolTypes.ListToolsResult
			{
				Tools =
				[
					new ModelContextProtocolTypes.Tool
					{
						Name = $"test_for_{Config.Name}",
						Description = $"This is a test tool for {Config} server"
					}
				]
			})
		}, cancellationToken);
	}

	private async Task Initialize(JsonRpcRequest request, CancellationToken cancellationToken)
	{
		await WriteMessageAsync(new JsonRpcResponse
		{
			Id = request.Id,
			Result = JsonSerializer.SerializeToNode(new ModelContextProtocolTypes.InitializeResult
			{
				ServerInfo = new() { Name = "TestServer", Version = "1.0.0" },
				ProtocolVersion = "2024-11-05",
				Capabilities = new() { },
			})
		}, cancellationToken);
	}

	protected async Task WriteMessageAsync(IJsonRpcMessage message, CancellationToken cancellationToken = default)
	{
		await _messageChannel.Writer.WriteAsync(message, cancellationToken);
	}
}