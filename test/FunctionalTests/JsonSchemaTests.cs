using System.Text.Json;
using System.Text.Json.Schema;
using NUnit.Framework;
using OllamaSharp;
using Shouldly;

namespace Tests.FunctionalTests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class JsonSchemaTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";

	private OllamaApiClient _client = null!;
	private Chat _chat = null!;

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
