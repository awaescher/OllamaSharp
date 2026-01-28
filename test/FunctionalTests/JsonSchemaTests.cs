using System.Text.Json;
using System.Text.Json.Schema;
using NUnit.Framework;
using OllamaSharp;
using Shouldly;

namespace Tests.FunctionalTests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

/// <summary>
/// Contains functional tests that verify JSON schema generation and deserialization using the Ollama API.
/// </summary>
public class JsonSchemaTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";

	private OllamaApiClient _client = null!;
	private Chat _chat = null!;

	/// <summary>
	/// Initializes the test fixture by creating an <see cref="OllamaApiClient"/> and a <see cref="Chat"/>
	/// instance, and ensures that the required model is available locally.
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
	/// Cleans up resources after each test execution.
	/// </summary>
	[TearDown]
	public Task Teardown()
	{
		_client?.Dispose();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Generates a sword description via the Ollama chat API and verifies that the response conforms to the expected JSON schema.
	/// </summary>
	[Test]
	public async Task GenerateSword_ShouldSucceed()
	{
		var responseSchema = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(Sword));

		_client.SelectedModel = _model;

		var response = await _chat
			.SendAsync("""
				Generate a sword with the name 'Excalibur'.

				Return a valid JSON object like:
				{
					"Name": "",
					"Damage": 0
				}
				""", tools: null, format: responseSchema)
			.StreamToEndAsync();
		response.ShouldNotBeNullOrEmpty();

		var responseSword = JsonSerializer.Deserialize<Sword>(response);
		responseSword.ShouldNotBeNull();
		responseSword.Name.ToLowerInvariant().ShouldContain("excalibur");
		responseSword.Damage.ShouldBeOfType(typeof(int));
	}

	private class Sword
	{
		public required string Name { get; set; }
		public required int Damage { get; set; }
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.