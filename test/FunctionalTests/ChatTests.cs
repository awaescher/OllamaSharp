using FluentAssertions;
using Microsoft.Extensions.AI;
using NUnit.Framework;
using OllamaSharp;

namespace Tests.FunctionalTests;

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
		_client = new OllamaApiClient(_baseUri);
		_chat = new Chat(_client);

		var modelExists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == _model);
		if (!modelExists)
			await _client.PullModelAsync(_model).ToListAsync();
	}

	[TearDown]
	public Task Teardown()
	{
		_client?.Dispose();
		return Task.CompletedTask;
	}


	[Test]
	public async Task SendAsync_ShouldSucceed()
	{
		_client.SelectedModel = _model;

		var response = await _chat
			.SendAsync("What is the ultimate answer to " +
					   "life, the universe, and everything, as specified in " +
					   "a Hitchhikers Guide to the Galaxy. " +
					   "Provide only the answer.",
				CancellationToken.None)
			.StreamToEndAsync();

		response.Should().NotBeNullOrEmpty();
		response.Should().ContainAny("42", "forty-two", "forty two");
	}
}