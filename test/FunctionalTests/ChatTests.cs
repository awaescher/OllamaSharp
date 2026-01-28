using NUnit.Framework;
using OllamaSharp;
using Shouldly;

namespace Tests.FunctionalTests;

/// <summary>
/// Contains functional tests for chat functionality.
/// </summary>
public class ChatTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";

	private OllamaApiClient _client = null!;
	private Chat _chat = null!;

	/// <summary>
	/// Initializes the Ollama client and ensures the test model is available.
	/// </summary>
	[SetUp]
	public async Task Setup()
	{
		_client = new OllamaApiClient(_baseUri);
		_chat = new Chat(_client);

		var modelExists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == _model);
		if (!modelExists)
			await _client.PullModelAsync(_model).ToListAsync();
	}

	/// <summary>
	/// Disposes the Ollama client after each test.
	/// </summary>
	[TearDown]
	public Task Teardown()
	{
		_client?.Dispose();
		return Task.CompletedTask;
	}


	/// <summary>
	/// Verifies that sending a message returns the expected response.
	/// </summary>
	[Test]
	public async Task SendAsync_ShouldSucceed()
	{
		_client.SelectedModel = _model;

		var response = await _chat
			.SendAsync("What is 1+1? Provide only the result number, nothing else.", CancellationToken.None)
			.StreamToEndAsync();

		response.ShouldBe("2");
	}
}