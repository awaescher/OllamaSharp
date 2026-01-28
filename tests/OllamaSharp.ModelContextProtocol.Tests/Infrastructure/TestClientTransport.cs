using System.Threading.Channels;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace OllamaSharp.ModelContextProtocol.Tests.Infrastructure;

internal class TestClientTransport : IClientTransport, IAsyncDisposable
{
	private readonly Channel<JsonRpcMessage> _messageChannel;

	public TestClientTransport(string name)
	{
		Name = name;

		_messageChannel = Channel.CreateUnbounded<JsonRpcMessage>(new UnboundedChannelOptions
		{
			SingleReader = true,
			SingleWriter = true,
		});
	}

	public string Name { get; }

	public Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult<ITransport>(new TestTransport(Name, _messageChannel));

	public ValueTask DisposeAsync()
		=> ValueTask.CompletedTask;
}