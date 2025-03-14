using NUnit.Framework;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Shouldly;
using ChatRole = OllamaSharp.Models.Chat.ChatRole;

namespace Tests.FunctionalTests;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class OllamaApiClientTests
{
	private readonly Uri _baseUri = new("http://localhost:11434");
	private readonly string _model = "llama3.2:1b";
	private readonly string _localModel = "OllamaSharpTest";
	private readonly string _embeddingModel = "all-minilm:22m";

	private OllamaApiClient _client = null!;

	[OneTimeSetUp]
	public async Task Setup()
	{
		_client = new OllamaApiClient(_baseUri);
		await CleanupModel(_localModel);
	}

	[OneTimeTearDown]
	public async Task Teardown()
	{
		await CleanupModel(_localModel + ":latest");
		_client?.Dispose();
	}

	private async Task CleanupModel(string model)
	{
		var modelExists = (await _client.ListLocalModelsAsync()).Any(m => m.Name == model);

		if (modelExists)
			await _client.DeleteModelAsync(new DeleteModelRequest { Model = model });
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
		models.ShouldContain(m => m.Name == _model);

		response.ShouldNotBeEmpty();
		response.ShouldContain(r => r!.Status == "pulling manifest");
		response.ShouldContain(r => r!.Status == "success");
	}

	[Test, Order(2)]
	public async Task CreateModel()
	{
		await PullIfNotExists(_model);

		var model = new CreateModelRequest
		{
			Model = _localModel,
			From = _model,
			Parameters = new Dictionary<string, object>()
			{
				 { "num_ctx", 4096 },
				 {"temperature", 0.31 }
			 }
		};

		var response = await _client
			.CreateModelAsync(model)
			.ToListAsync();

		response.ShouldNotBeEmpty();
		response.ShouldContain(r => r!.Status == "success");

		var models = await _client.ListLocalModelsAsync();
		models.ShouldContain(m => m.Name.StartsWith(_localModel));
	}

	[Test, Order(3)]
	public async Task CopyModel()
	{
		await PullIfNotExists(_localModel);

		var model = new CopyModelRequest { Source = _localModel, Destination = $"{_localModel}-copy" };

		await _client.CopyModelAsync(model);

		var models = await _client.ListLocalModelsAsync();
		models.ShouldContain(m => m.Name == $"{_localModel}-copy:latest");

		await _client.DeleteModelAsync(new DeleteModelRequest { Model = $"{_localModel}-copy:latest" });
	}

	[Test]
	public async Task Embed()
	{
		await PullIfNotExists(_embeddingModel);

		var request = new EmbedRequest { Model = _embeddingModel, Input = ["Hello, world!"] };

		var response = await _client.EmbedAsync(request);

		response.ShouldNotBeNull();
		response.Embeddings.ShouldNotBeEmpty();
		response.LoadDuration!.Value.ShouldBeGreaterThan(100, "Because loading the model should take some time");
		response.TotalDuration!.Value.ShouldBeGreaterThan(100, "Because generating embeddings should take some time");
	}

	[Test]
	public async Task ListLocalModels()
	{
		var models = (await _client.ListLocalModelsAsync()).ToList();

		models.ShouldNotBeEmpty();
		models.ShouldContain(m => m.Name == _model);
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
		models.ShouldNotBeEmpty();
		models.ShouldContain(m => m.Name == _model);
	}

	[Test]
	public async Task ShowModel()
	{
		await PullIfNotExists(_model);

		var response = await _client.ShowModelAsync(new ShowModelRequest { Model = _model });

		response.ShouldNotBeNull();
		response.Info.ShouldNotBeNull();
		response.Info.Architecture.ShouldBe("llama");
		response.Details.ShouldNotBeNull();
		response.Details.Format.ShouldNotBeNullOrEmpty();
		response.Details.Family.ShouldBe("llama");
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

		exists.ShouldBeTrue();

		await _client.DeleteModelAsync(new DeleteModelRequest { Model = $"{_localModel}-copy:latest" });

		var models = await _client.ListLocalModelsAsync();
		models.ShouldNotContain(m => m.Name == $"{_localModel}-copy:latest");
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

		response.ShouldNotBeEmpty();
		joined.ShouldContain("42");
	}

	[Test]
	public async Task ChatAsync()
	{
		await PullIfNotExists(_model);

		var response = await _client.ChatAsync(new ChatRequest
		{
			Model = _model,
			Messages =
			[
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
			]
		})
		.ToListAsync();

		var joined = string.Join("", response.Select(r => r.Message.Content));

		response.ShouldNotBeEmpty();
		joined.ShouldContain("Douglas Adams");
	}

	[Test]
	public async Task IsRunningAsync()
	{
		var response = await _client.IsRunningAsync();
		response.ShouldBeTrue();
	}

	[Test]
	public async Task GetVersionAsync()
	{
		var response = await _client.GetVersionAsync();
		response.ShouldNotBeNull();
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.