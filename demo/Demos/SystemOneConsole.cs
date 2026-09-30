using OllamaSharp;
using OllamaSharp.Models;
using Spectre.Console;

namespace OllamaApiConsole.Demos;

/// <summary>
/// Demonstrates the System One endpoint by triaging support tickets.
/// The user types a ticket and the model answers a choice, a yes/no (noul) and a score question about it.
/// </summary>
/// <param name="ollama">The <see cref="IOllamaApiClient"/> used to communicate with the Ollama service.</param>
public class SystemOneConsole(IOllamaApiClient ollama) : OllamaConsole(ollama)
{
	private const string SYSTEMONE_MODEL = "nimble";
	private const string MINIMUM_OLLAMA_VERSION = "0.35.0";

	private const string LABEL_QUESTION = "label";
	private const string REFUND_QUESTION = "refund";
	private const string URGENCY_QUESTION = "urgency";

	/// <inheritdoc/>
	public override async Task Run()
	{
		AnsiConsole.Write(new Rule("System One").LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("System One does not write text. It answers predefined questions about your input with probabilities, ready to use in code.");
		AnsiConsole.MarkupLine($"Type a [{AccentTextColor}]support ticket[/] and the model will tell you:");
		AnsiConsole.MarkupLine($"  [{AiTextColor}]•[/] [{AccentTextColor}]label[/]    which category fits [{HintTextColor}](choice: billing, bug, account, feedback)[/]");
		AnsiConsole.MarkupLine($"  [{AiTextColor}]•[/] [{AccentTextColor}]refund[/]   whether the customer wants money back [{HintTextColor}](yes/no)[/]");
		AnsiConsole.MarkupLine($"  [{AiTextColor}]•[/] [{AccentTextColor}]urgency[/]  how urgent it is [{HintTextColor}](score: 0 = routine … 2 = immediate)[/]");
		AnsiConsole.MarkupLine($"[{WarningTextColor}]Requires Ollama v{MINIMUM_OLLAMA_VERSION} or later and a local model trained for System One, like [{AccentTextColor}]{SYSTEMONE_MODEL}[/] (ollama pull {SYSTEMONE_MODEL}).[/]");
		AnsiConsole.WriteLine();

		if (!await EnsureOllamaVersion())
			return;

		var defaultModel = await EnsureSystemOneModel();

		Ollama.SelectedModel = await SelectModel("Select a System One model:", defaultModel: defaultModel);

		if (string.IsNullOrEmpty(Ollama.SelectedModel))
			return;

		AnsiConsole.MarkupLineInterpolated($"You are using [{AccentTextColor}]{Ollama.SelectedModel}[/] for System One questions.");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Enter a support ticket to triage. Type [{AccentTextColor}]{EXIT_COMMAND}[/] to leave.[/]");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Begin with [{AccentTextColor}][[[/] to start multiline input. Submit it by ending with [{AccentTextColor}]]][/].[/]");

		string message;

		do
		{
			AnsiConsole.WriteLine();
			message = ReadInput($"Enter a [{AccentTextColor}]support ticket[/] [{HintTextColor}](e.g. \"Our checkout has returned 500 errors since 9am.\" or \"I was charged twice. Please refund the extra payment.\")[/]");

			if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
				break;

			if (string.IsNullOrWhiteSpace(message))
				continue;

			var request = CreateRequest(message);

			var response = await AnsiConsole.Status()
				.StartAsync("Asking System One ...", _ => Ollama.SystemOneAsync(request));

			AnsiConsole.WriteLine();
			RenderResponse(response);
		} while (!string.IsNullOrEmpty(message));
	}

	private async Task<bool> EnsureOllamaVersion()
	{
		var versionText = await Ollama.GetVersionAsync();
		var minimumVersion = Version.Parse(MINIMUM_OLLAMA_VERSION);

		if (!TryParseVersion(versionText, out var version))
		{
			AnsiConsole.MarkupLineInterpolated($"[{WarningTextColor}]Could not determine the Ollama version \"{versionText}\". Make sure it is v{MINIMUM_OLLAMA_VERSION} or later.[/]");
			AnsiConsole.WriteLine();
			return true;
		}

		if (version >= minimumVersion)
			return true;

		AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]Your Ollama instance runs v{versionText} but System One requires v{MINIMUM_OLLAMA_VERSION} or later.[/]");
		AnsiConsole.MarkupLine($"[{HintTextColor}]Update Ollama from [/][{AccentTextColor}][link]https://ollama.com/download[/][/][{HintTextColor}] and try again.[/]");
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine($"Press [{AccentTextColor}]Return[/] to go back.");
		Console.ReadLine();
		return false;
	}

	private static bool TryParseVersion(string versionText, out Version version)
	{
		// Ollama versions may contain suffixes like "0.35.0-rc1"
		var numericPart = new string(versionText.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
		return Version.TryParse(numericPart, out version!);
	}

	private async Task<string> EnsureSystemOneModel()
	{
		var model = await FindSystemOneModel();
		if (!string.IsNullOrEmpty(model))
			return model;

		AnsiConsole.MarkupLine($"[{WarningTextColor}]The System One model [{AccentTextColor}]{SYSTEMONE_MODEL}[/] is not available on your Ollama instance.[/]");

		if (!AnsiConsole.Confirm($"Do you want to pull [{AccentTextColor}]{SYSTEMONE_MODEL}[/] now?"))
		{
			AnsiConsole.MarkupLine($"[{HintTextColor}]Make sure to select a model trained for System One, other models will be rejected by Ollama.[/]");
			AnsiConsole.WriteLine();
			return "";
		}

		await AnsiConsole.Progress().StartAsync(async context =>
		{
			ProgressTask? task = null;

			await foreach (var status in Ollama.PullModelAsync(SYSTEMONE_MODEL))
			{
				if (status is null)
					continue;

				if (status.Status != task?.Description)
				{
					task?.StopTask();
					task = context.AddTask(status.Status);
				}

				task.Increment(status.Percent - task.Value);
			}

			task?.StopTask();
		});

		AnsiConsole.WriteLine();
		return await FindSystemOneModel();
	}

	private async Task<string> FindSystemOneModel()
	{
		var models = await Ollama.ListLocalModelsAsync();

		return models
			.Select(m => m.Name)
			.FirstOrDefault(name => name.Equals(SYSTEMONE_MODEL, StringComparison.OrdinalIgnoreCase) || name.StartsWith(SYSTEMONE_MODEL + ":", StringComparison.OrdinalIgnoreCase)) ?? "";
	}

	private SystemOneRequest CreateRequest(string ticket)
	{
		return new SystemOneRequest
		{
			Model = Ollama.SelectedModel,
			State = new Dictionary<string, string> { ["ticket"] = ticket },
			Questions = new Dictionary<string, SystemOneQuestion>
			{
				[LABEL_QUESTION] = new SystemOneChoiceQuestion
				{
					Instructions = "Which label fits this ticket?",
					Criteria = new Dictionary<string, string?>
					{
						["billing"] = "Payments and refunds",
						["bug"] = "Software errors",
						["account"] = "Login and account access",
						["feedback"] = "Feature requests and general feedback"
					}
				},
				[REFUND_QUESTION] = new SystemOneNoulQuestion
				{
					Instructions = "Is the customer requesting a refund?",
					Criteria = new SystemOneNoulCriteria
					{
						No = "No refund is requested",
						Yes = "The customer requests a refund"
					}
				},
				[URGENCY_QUESTION] = new SystemOneScoreQuestion
				{
					Instructions = "How urgently does this ticket need a response?",
					Criteria =
					[
						"Routine: no time pressure",
						"Soon: a customer is inconvenienced",
						"Immediate: a critical service is unavailable"
					]
				}
			}
		};
	}

	private static void RenderResponse(SystemOneResponse response)
	{
		foreach (var (name, answer) in response.Answers)
		{
			AnsiConsole.Write(new Rule($"[{HintTextColor}]{Markup.Escape(name)} ({answer.Type})[/]").LeftJustified());
			AnsiConsole.WriteLine();

			switch (answer)
			{
				case SystemOneChoiceAnswer choice:
					RenderChoiceAnswer(choice);
					break;

				case SystemOneNoulAnswer noul:
					RenderNoulAnswer(noul);
					break;

				case SystemOneScoreAnswer score:
					RenderScoreAnswer(score);
					break;
			}

			AnsiConsole.WriteLine();
		}

		AnsiConsole.MarkupLine($"[{HintTextColor}]Usage: {response.Usage?.InputTokens ?? 0} input tokens, {response.Usage?.OutputTokens ?? 0} output tokens[/]");
	}

	private static void RenderChoiceAnswer(SystemOneChoiceAnswer answer)
	{
		AnsiConsole.MarkupLineInterpolated($"Choice: [bold {AiTextColor}]{answer.Choice}[/] [{HintTextColor}](confidence {answer.Confidence:P1})[/]");
		AnsiConsole.WriteLine();

		var chart = new BarChart().Width(60).WithMaxValue(100);

		foreach (var (option, probability) in answer.Probabilities)
		{
			var color = option == answer.Choice ? Color.Aqua : Color.Grey;
			chart.AddItem(Markup.Escape(option), Math.Round(probability * 100, 1), color);
		}

		AnsiConsole.Write(chart);
	}

	private static void RenderNoulAnswer(SystemOneNoulAnswer answer)
	{
		var isTrue = answer.Noul >= 0.5;
		AnsiConsole.MarkupLineInterpolated($"Answer: [bold {AiTextColor}]{(isTrue ? "Yes" : "No")}[/] [{HintTextColor}](probability of yes {answer.Noul:P1})[/]");
		AnsiConsole.WriteLine();

		var chart = new BarChart()
			.Width(60)
			.WithMaxValue(100)
			.AddItem("No", Math.Round((1 - answer.Noul) * 100, 1), isTrue ? Color.Grey : Color.Aqua)
			.AddItem("Yes", Math.Round(answer.Noul * 100, 1), isTrue ? Color.Aqua : Color.Grey);

		AnsiConsole.Write(chart);
	}

	private static void RenderScoreAnswer(SystemOneScoreAnswer answer)
	{
		var maxScore = Math.Max(1, answer.Legend.Count - 1);
		var level = (int)Math.Round(answer.Score);
		var levelDescription = answer.Legend.TryGetValue(level.ToString(), out var description) ? description : level.ToString();

		AnsiConsole.MarkupLineInterpolated($"Score: [bold {AiTextColor}]{answer.Score:0.00}[/] of {maxScore} — closest to [{AiTextColor}]{levelDescription}[/] [{HintTextColor}](confidence {answer.Confidence:P1})[/]");
		AnsiConsole.WriteLine();

		var chart = new BarChart().Width(60).WithMaxValue(100);

		foreach (var (index, text) in answer.Legend.OrderBy(l => int.TryParse(l.Key, out var i) ? i : int.MaxValue))
		{
			var probability = answer.Probabilities.TryGetValue(index, out var p) ? p : 0;
			var color = index == level.ToString() ? Color.Aqua : Color.Grey;
			chart.AddItem(Markup.Escape($"{index}: {text}"), Math.Round(probability * 100, 1), color);
		}

		AnsiConsole.Write(chart);
	}
}
