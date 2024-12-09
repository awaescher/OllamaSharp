using FluentAssertions;
using Microsoft.Extensions.AI;
using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using ChatRole = OllamaSharp.Models.Chat.ChatRole;

namespace Tests.FunctionalTests;

public class OllamaApiClientTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";
	private readonly string _localModel = "OllamaSharpTest";
	private readonly string _embeddingModel = "all-minilm:22m";

#pragma warning disable NUnit1032
	private OllamaApiClient _client = null!;
#pragma warning restore NUnit1032

	[SetUp]
	public async Task Setup()
	{
		_client = new OllamaApiClient(_baseUri);
		await CleanupModel(_localModel);
	}

	[TearDown]
	public async Task Teardown()
	{
		await CleanupModel(_localModel);
		((IChatClient?)_client)?.Dispose();
	}

	private async Task CleanupModel(string? model = null)
	{
		var modelExists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == (model ?? _model));

		if (modelExists)
			await _client.DeleteModelAsync(new DeleteModelRequest { Model = model ?? _model });
	}

	private async Task PullIfNotExists(string model)
	{
		var modelExists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == model);

		if (!modelExists)
			await _client.PullModelAsync(new PullModelRequest { Model = model }).ToListAsync();
	}


	[Test, Order(1), Ignore("Prevent the model from being downloaded each test run")]
	public async Task PullModel()
	{
		var response = await _client
			.PullModelAsync(new PullModelRequest { Model = _model })
			.ToListAsync();

		var models = await _client.ListLocalModelsAsync();
		models.Should().Contain(m => m.Name == _model);

		response.Should().NotBeEmpty();
		response.Should().Contain(r => r!.Status == "pulling manifest");
		response.Should().Contain(r => r!.Status == "success");
	}

	[Test, Order(2)]
	public async Task CreateModel()
	{
		await PullIfNotExists(_localModel);

		var model = new CreateModelRequest
		{
			Model = _localModel,
			ModelFileContent =
				"""
				FROM llama3.2
				PARAMETER temperature 0.3
				PARAMETER num_ctx 100

				# sets a custom system message to specify the behavior of the chat assistant
				SYSTEM You are a concise model that tries to return yes or no answers.
				"""
		};

		var response = await _client
			.CreateModelAsync(model)
			.ToListAsync();

		var models = await _client.ListLocalModelsAsync();
		models.Should().Contain(m => m.Name.StartsWith(_localModel));

		response.Should().NotBeEmpty();
		response.Should().Contain(r => r!.Status == "success");
	}

	[Test, Order(3)]
	public async Task CopyModel()
	{
		await PullIfNotExists(_localModel);

		var model = new CopyModelRequest { Source = _localModel, Destination = $"{_localModel}-copy" };

		await _client.CopyModelAsync(model);

		var models = await _client.ListLocalModelsAsync();
		models.Should().Contain(m => m.Name == $"{_localModel}-copy:latest");

		await _client.DeleteModelAsync(new DeleteModelRequest { Model = $"{_localModel}-copy:latest" });
	}

	[Test]
	public async Task Embed()
	{
		await PullIfNotExists(_embeddingModel);

		var request = new EmbedRequest { Model = _embeddingModel, Input = ["Hello, world!"] };

		var response = await _client.EmbedAsync(request);

		response.Should().NotBeNull();
		response.Embeddings.Should().NotBeEmpty();
		response.LoadDuration.Should().BeGreaterThan(100, "Because loading the model should take some time");
		response.TotalDuration.Should().BeGreaterThan(100, "Because generating embeddings should take some time");
	}

	[Test]
	public async Task ListLocalModels()
	{
		var models = (await _client.ListLocalModelsAsync()).ToList();

		models.Should().NotBeEmpty();
		models.Should().Contain(m => m.Name == _model);
	}

	[Test]
	public async Task ListRunningModels()
	{
		await PullIfNotExists(_model);
		var backgroundTask = Task.Run(async () =>
		{
			var generate = _client
				.GenerateAsync(new GenerateRequest { Model = _model, Prompt = "Write a long song." })
				.ToListAsync();

			await Task.Yield();

			await generate;
		});

		var modelsTask = _client.ListRunningModelsAsync();
		await Task.WhenAll(backgroundTask, modelsTask);

		var models = modelsTask.Result.ToList();
		models.Should().NotBeEmpty();
		models.Should().Contain(m => m.Name == _model);
	}

	[Test]
	public async Task ShowModel()
	{
		await PullIfNotExists(_model);

		var response = await _client.ShowModelAsync(new ShowModelRequest { Model = _model });

		response.Should().NotBeNull();
		response.Info.Should().NotBeNull();
		response.Info.Architecture.Should().Be("llama");
		response.Details.Should().NotBeNull();
		response.Details.Format.Should().NotBeNullOrEmpty();
		response.Details.Family.Should().Be("llama");
	}

	[Test]
	public async Task DeleteModel()
	{
		await PullIfNotExists(_localModel);
		await _client.CopyModelAsync(new CopyModelRequest
		{
			Source = _localModel,
			Destination = $"{_localModel}-copy"
		});

		var exists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == $"{_localModel}-copy:latest");

		exists.Should().BeTrue();

		await _client.DeleteModelAsync(new DeleteModelRequest { Model = $"{_localModel}-copy:latest" });

		var models = await _client.ListLocalModelsAsync();
		models.Should().NotContain(m => m.Name == $"{_localModel}-copy:latest");
	}

	[Test]
	public async Task GenerateAsync()
	{
		await PullIfNotExists(_model);

		var response = await _client.GenerateAsync(new GenerateRequest
		{
			Model = _model,
			Prompt =
				"What is the meaning to life, the universe, and everything according to the Hitchhikers Guide to the Galaxy?"
		})
		.ToListAsync();

		var joined = string.Join("", response.Select(r => r.Response));

		response.Should().NotBeEmpty();
		joined.Should().Contain("42");
	}

	[Test]
	public async Task ChatAsync()
	{
		await PullIfNotExists(_model);

		var response = await _client.ChatAsync(new ChatRequest
		{
			Model = _model,
			Messages = new[]
			{
				new Message
				{
					Role = ChatRole.User,
					Content = "What is the meaning to life, the universe, and everything according to the Hitchhikers Guide to the Galaxy?"
				},
				new Message
				{
					Role = ChatRole.System,
					Content = "According to the Hitchhikers Guide to the Galaxy, the meaning to life, the universe, and everything is 42."
				},
				new Message
				{
					Role = ChatRole.User,
					Content = "Who is the author of the Hitchhikers Guide to the Galaxy?"
				}
			}
		})
		.ToListAsync();

		var joined = string.Join("", response.Select(r => r.Message.Content));

		response.Should().NotBeEmpty();
		joined.Should().Contain("Douglas Adams");
	}

	[Test]
	public async Task IsRunningAsync()
	{
		var response = await _client.IsRunningAsync();
		response.Should().BeTrue();
	}

	[Test]
	public async Task GetVersionAsync()
	{
		var response = await _client.GetVersionAsync();
		response.Should().NotBeNull();
	}
}