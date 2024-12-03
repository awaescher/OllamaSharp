using FluentAssertions;
using Microsoft.Extensions.AI;

namespace OllamaSharp.FunctionalTests;

public class ChatTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";

#pragma warning disable NUnit1032
	private OllamaApiClient _client = null!;
	private Chat _chat = null!;
#pragma warning restore NUnit1032

	[SetUp]
	public async Task Setup()
	{
		// Set up the test environment
		_client = new OllamaApiClient(_baseUri);
		_chat = new Chat(_client);

		await _client.PullIfNotExistsAsync(_model);
	}

	[TearDown]
	public Task Teardown()
	{
		// Clean up the test environment
		((IChatClient?)_client)?.Dispose();

		return Task.CompletedTask;
	}


	[Test]
	public async Task SendAsync_ShouldSucceed()
	{
		// Arrange
		_client.SelectedModel = _model;

		// Act
		var response = await _chat
			.SendAsync("What is the ultimate answer to " +
					   "life, the universe, and everything, as specified in " +
					   "a Hitchhikers Guide to the Galaxy. " +
					   "Provide only the answer.",
				CancellationToken.None)
			.StreamToEndAsync();

		// Assert
		response.Should().NotBeNullOrEmpty();
		response.Should().ContainAny("42", "forty-two", "forty two");
	}
}