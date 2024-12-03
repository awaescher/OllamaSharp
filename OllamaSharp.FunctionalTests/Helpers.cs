using OllamaSharp.Models;

namespace OllamaSharp.FunctionalTests;

public static class Helpers
{
	public static async Task PullIfNotExistsAsync(
		this IOllamaApiClient client,
		string model)
	{
		var modelExists = (await client.ListLocalModelsAsync())
			.Any(m => m.Name == model);

		if (!modelExists)
			await client.PullModelAsync(new PullModelRequest { Model = model })
				.ToListAsync();
	}
}