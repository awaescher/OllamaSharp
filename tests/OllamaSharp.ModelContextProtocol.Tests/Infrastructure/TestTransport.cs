using System.Text.Json;
using System.Threading.Channels;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

namespace OllamaSharp.ModelContextProtocol.Tests.Infrastructure;

internal class TestTransport : ITransport
{
	private readonly string _name;
	private readonly ChannelWriter<IJsonRpcMessage> _messageWriter;

	public TestTransport(string name, Channel<IJsonRpcMessage> channel)
	{
		_name = name;
		MessageReader = channel.Reader;
		_messageWriter = channel.Writer;

		IsConnected = true;
	}

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	public async Task SendMessageAsync(IJsonRpcMessage message, CancellationToken cancellationToken = default)
	{
		if (message is JsonRpcRequest request)
		{
			if (request.Method == "initialize")
			{
				await Initialize(request, cancellationToken);
			}
			else if (request.Method == "tools/list")
			{
				await ListTools(request, cancellationToken);
			}
		}
	}

	public bool IsConnected { get; }

	public ChannelReader<IJsonRpcMessage> MessageReader { get; }

	private async Task ListTools(JsonRpcRequest request, CancellationToken cancellationToken)
	{
		await _messageWriter.WriteAsync(new JsonRpcResponse
		{
			Id = request.Id,
			Result = JsonSerializer.SerializeToNode(new ListToolsResult
			{
				Tools =
				[
					new Tool
					{
						Name = $"test_for_{_name}",
						Description = $"This is a test tool for {_name} server"
					}
				]
			})
		}, cancellationToken);
	}

	private async Task Initialize(JsonRpcRequest request, CancellationToken cancellationToken)
	{
		await _messageWriter.WriteAsync(new JsonRpcResponse
		{
			Id = request.Id,
			Result = JsonSerializer.SerializeToNode(new InitializeResult
			{
				ServerInfo = new() { Name = "TestServer", Version = "1.0.0" },
				ProtocolVersion = "2024-11-05",
				Capabilities = new() { },
			})
		}, cancellationToken);
	}
}