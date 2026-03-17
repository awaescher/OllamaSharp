using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Demonstrates structured outputs by extracting a recipe in a strongly-typed JSON schema.
/// The user types a dish name and the model returns structured data that is rendered as a formatted table.
/// </summary>
public class StructuredOutputConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new()
	{
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <inheritdoc/>
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("Structured outputs").LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("This demo asks the model to return data that exactly matches a predefined JSON schema.");
		AnsiConsole.MarkupLine($"Type the name of any dish and get back a structured [{AccentTextColor}]recipe[/] — no free-form text, only typed data.");
		AnsiConsole.WriteLine();

		Ollama.SelectedModel = await SelectModel("Select a model you want to use:");
		SetThink(new ThinkValue(false));

		if (string.IsNullOrEmpty(Ollama.SelectedModel))
			return;

		var schema = JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(RecipeSchema));

		AnsiConsole.Write(new Rule($"[{HintTextColor}]Expected JSON schema[/]").LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLineInterpolated($"[{HintTextColor}]{Markup.Escape(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }))}[/]");
		AnsiConsole.WriteLine();

		WriteChatInstructionHint();

		var keepChatting = true;

		do
		{
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLineInterpolated($"You are talking to [{AccentTextColor}]{Ollama.SelectedModel}[/] now.");

			string message;

			do
			{
				AnsiConsole.WriteLine();
				message = ReadInput($"Enter a [{AccentTextColor}]dish name[/] [{HintTextColor}](e.g. \"Pasta Carbonara\" or \"Vegan Chocolate Cake\")[/]");

				if (string.IsNullOrWhiteSpace(message))
					continue;

				if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
				{
					keepChatting = false;
					break;
				}

				if (message.Equals(TOGGLETHINK_COMMAND, StringComparison.OrdinalIgnoreCase))
				{
					ToggleThink();
					keepChatting = true;
					continue;
				}

				if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
				{
					keepChatting = true;
					break;
				}

				var prompt = $"Create a detailed recipe for: {message}. Respond only with the JSON object, no markdown, no explanation.";
				var json = new System.Text.StringBuilder();

				// Step 1: stream the raw model output live
				AnsiConsole.Write(new Rule($"[{HintTextColor}]Raw model output[/]").LeftJustified());
				AnsiConsole.WriteLine();
				AnsiConsole.WriteLine();

				var chat = new Chat(Ollama) { Think = Think };
				chat.OnThink += (sender, thoughts) => AnsiConsole.MarkupInterpolated($"[{AiThinkTextColor}]{thoughts}[/]");

				await foreach (var token in chat.SendAsync(prompt, tools: null, format: schema))
				{
					AnsiConsole.MarkupInterpolated($"[{HintTextColor}]{Markup.Escape(token)}[/]");
					json.Append(token);
				}

				AnsiConsole.WriteLine();
				AnsiConsole.WriteLine();

				// Step 2: pretty-print the JSON
				AnsiConsole.Write(new Rule($"[{HintTextColor}]Parsed JSON[/]").LeftJustified());
				AnsiConsole.WriteLine();
				RenderPrettyJson(json.ToString());
				AnsiConsole.WriteLine();

				// Step 3: the final recipe card
				AnsiConsole.Write(new Rule($"[{HintTextColor}]Recipe[/]").LeftJustified());
				AnsiConsole.WriteLine();
				RenderRecipe(json.ToString(), message);
			}
			while (!string.IsNullOrEmpty(message));
		}
		while (keepChatting);
	}

	private static void RenderPrettyJson(string json)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			var pretty = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
			AnsiConsole.MarkupLineInterpolated($"[{AiTextColor}]{Markup.Escape(pretty)}[/]");
		}
		catch (JsonException)
		{
			// Not yet valid JSON — just print as-is
			AnsiConsole.MarkupLineInterpolated($"[{HintTextColor}]{Markup.Escape(json)}[/]");
		}
	}

	private static void RenderRecipe(string json, string userInput)
	{
		RecipeSchema? recipe = null;

		try
		{
			recipe = JsonSerializer.Deserialize<RecipeSchema>(json, SERIALIZER_OPTIONS);
		}
		catch (JsonException)
		{
			AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]Could not parse the model's response as a recipe. Raw output:[/]");
			AnsiConsole.WriteLine(json);
			return;
		}

		if (recipe is null)
		{
			AnsiConsole.MarkupLineInterpolated($"[{WarningTextColor}]The model returned an empty response.[/]");
			return;
		}

		AnsiConsole.MarkupLineInterpolated($"[bold {AccentTextColor}]{Markup.Escape(recipe.Name ?? userInput)}[/]");

		if (!string.IsNullOrWhiteSpace(recipe.Description))
			AnsiConsole.MarkupLineInterpolated($"[italic]{Markup.Escape(recipe.Description)}[/]");

		AnsiConsole.WriteLine();

		// Meta row: timing, servings, difficulty
		var metaTable = new Table().NoBorder().HideHeaders();
		metaTable.AddColumn(new TableColumn("").Width(22));
		metaTable.AddColumn(new TableColumn(""));

		if (recipe.PrepTimeMinutes > 0)
			metaTable.AddRow($"[{HintTextColor}]Prep time[/]", $"{recipe.PrepTimeMinutes} min");

		if (recipe.CookTimeMinutes > 0)
			metaTable.AddRow($"[{HintTextColor}]Cook time[/]", $"{recipe.CookTimeMinutes} min");

		if (recipe.PrepTimeMinutes > 0 && recipe.CookTimeMinutes > 0)
			metaTable.AddRow($"[{HintTextColor}]Total time[/]", $"{recipe.PrepTimeMinutes + recipe.CookTimeMinutes} min");

		if (recipe.Servings > 0)
			metaTable.AddRow($"[{HintTextColor}]Servings[/]", $"{recipe.Servings}");

		if (!string.IsNullOrWhiteSpace(recipe.Difficulty))
			metaTable.AddRow($"[{HintTextColor}]Difficulty[/]", recipe.Difficulty);

		AnsiConsole.Write(metaTable);
		AnsiConsole.WriteLine();

		// Ingredients
		if (recipe.Ingredients?.Length > 0)
		{
			AnsiConsole.MarkupLine($"[bold]Ingredients[/]");
			foreach (var ingredient in recipe.Ingredients)
				AnsiConsole.MarkupLineInterpolated($"  [{AiTextColor}]•[/] {Markup.Escape(ingredient)}");

			AnsiConsole.WriteLine();
		}

		// Steps
		if (recipe.Steps?.Length > 0)
		{
			AnsiConsole.MarkupLine($"[bold]Steps[/]");
			for (var i = 0; i < recipe.Steps.Length; i++)
				AnsiConsole.MarkupLineInterpolated($"  [{AiTextColor}]{i + 1,2}.[/] {Markup.Escape(recipe.Steps[i])}");

			AnsiConsole.WriteLine();
		}
	}

	/// <summary>
	/// Defines the JSON schema the model must respond with.
	/// </summary>
	private sealed class RecipeSchema
	{
		/// <summary>The official name of the dish.</summary>
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		/// <summary>A short, appetising description of the dish.</summary>
		[JsonPropertyName("description")]
		public string? Description { get; set; }

		/// <summary>List of ingredients with quantities.</summary>
		[JsonPropertyName("ingredients")]
		public string[]? Ingredients { get; set; }

		/// <summary>Ordered list of preparation steps.</summary>
		[JsonPropertyName("steps")]
		public string[]? Steps { get; set; }

		/// <summary>Preparation time in minutes, before any cooking starts.</summary>
		[JsonPropertyName("prepTimeMinutes")]
		public int PrepTimeMinutes { get; set; }

		/// <summary>Cooking or baking time in minutes.</summary>
		[JsonPropertyName("cookTimeMinutes")]
		public int CookTimeMinutes { get; set; }

		/// <summary>Number of portions the recipe yields.</summary>
		[JsonPropertyName("servings")]
		public int Servings { get; set; }

		/// <summary>Subjective difficulty, e.g. "Easy", "Medium", "Hard".</summary>
		[JsonPropertyName("difficulty")]
		public string? Difficulty { get; set; }
	}
}
