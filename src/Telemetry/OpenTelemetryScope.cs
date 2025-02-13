using System.ClientModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OllamaSharp.Models.Chat;

using static OllamaSharp.Telemetry.OpenTelemetryConstants;

namespace OllamaSharp.Telemetry;

internal class OpenTelemetryScope : IDisposable
{
	private static readonly ActivitySource _chatSource = new("OllamaSharp.ChatClient");
	private static readonly Meter _chatMeter = new("OllamaSharp.ChatClient");

	// TODO: add explicit histogram buckets once System.Diagnostics.DiagnosticSource 9.0 is used
	private static readonly Histogram<double> _duration = _chatMeter.CreateHistogram<double>(GEN_AI_CLIENT_OPERATION_DURATION_METRIC_NAME, "s", "Measures GenAI operation duration.");
	private static readonly Histogram<long> _tokens = _chatMeter.CreateHistogram<long>(GEN_AI_CLIENT_TOKEN_USAGE_METRIC_NAME, "{token}", "Measures the number of input and output token used.");

	private readonly string _operationName;
	private readonly string _serverAddress;
	private readonly int _serverPort;
	private readonly string _requestModel;

	private Stopwatch? _durationTime;
	private Activity? _activity;
	private TagList? _commonTags;

	private OpenTelemetryScope(
		string model, string operationName,
		string serverAddress, int serverPort)
	{
		_requestModel = model;
		_operationName = operationName;
		_serverAddress = serverAddress;
		_serverPort = serverPort;
	}

	private static bool IsChatEnabled => _chatSource.HasListeners() || _duration.Enabled;

	public static OpenTelemetryScope? StartChat(string model, string operationName,
		string serverAddress, int serverPort, ChatRequest options)
	{
		if (IsChatEnabled)
		{
			var scope = new OpenTelemetryScope(model, operationName, serverAddress, serverPort);
			scope.StartChat(options);
			return scope;
		}

		return null;
	}

	private void StartChat(ChatRequest options)
	{
		_durationTime = Stopwatch.StartNew();
		_commonTags = new TagList
		{
			{ GEN_AI_SYSTEM_KEY, GEN_AI_SYSTEM_VALUE },
			{ GEN_AI_REQUEST_MODEL_KEY, _requestModel },
			{ SERVER_ADDRESS_KEY, _serverAddress },
			{ SERVER_PORT_KEY, _serverPort },
			{ GEN_AI_OPERATION_NAME_KEY, _operationName },
		};

		_activity = _chatSource.StartActivity(string.Concat(_operationName, " ", _requestModel), ActivityKind.Client);
		if (_activity?.IsAllDataRequested == true)
		{
			RecordCommonAttributes();
			SetActivityTagIfNotNull(GEN_AI_REQUEST_MAX_TOKENS_KEY, options?.Options?.NumPredict);
			SetActivityTagIfNotNull(GEN_AI_REQUEST_TEMPERATURE_KEY, options?.Options?.Temperature);
			SetActivityTagIfNotNull(GEN_AI_REQUEST_TOP_P_KEY, options?.Options?.TopP);
			SetActivityTagIfNotNull(GEN_AI_REQUEST_TOP_K_KEY, options?.Options?.TopK);
			SetActivityTagIfNotNull(GEN_AI_REQUEST_PRESENCE_PENALTY_KEY, options?.Options?.PresencePenalty);
			SetActivityTagIfNotNull(GEN_AI_REQUEST_STOP_SEQUENCES_KEY, options?.Options?.Stop);
		}

		return;
	}

	public void RecordChatCompletion(ChatResponseStream? completion)
	{
		if (completion == null)
		{
			return;
		}

		var doneCompletion = completion as ChatDoneResponseStream;

		RecordMetrics(completion.Model, null, doneCompletion?.PromptEvalCount, doneCompletion?.EvalCount);

		if (_activity?.IsAllDataRequested == true)
		{
			SetActivityTagIfNotNull(GEN_AI_RESPONSE_MODEL_KEY, completion.Model);
			SetActivityTagIfNotNull(GEN_AI_USAGE_INPUT_TOKENS_KEY, doneCompletion?.PromptEvalCount);
			SetActivityTagIfNotNull(GEN_AI_USAGE_OUTPUT_TOKENS_KEY, doneCompletion?.EvalCount);
		}
	}

	public void RecordException(Exception ex)
	{
		var errorType = GetErrorType(ex);
		RecordMetrics(null, errorType, null, null);
		if (_activity?.IsAllDataRequested == true)
		{
			_activity?.SetTag(ERROR_TYPE_KEY, errorType);
			_activity?.SetStatus(ActivityStatusCode.Error, ex?.Message ?? errorType);
		}
	}

	public void Dispose()
	{
		_activity?.Stop();
	}

	private void RecordCommonAttributes()
	{
		_activity?.SetTag(GEN_AI_SYSTEM_KEY, GEN_AI_SYSTEM_VALUE);
		_activity?.SetTag(GEN_AI_REQUEST_MODEL_KEY, _requestModel);
		_activity?.SetTag(SERVER_ADDRESS_KEY, _serverAddress);
		_activity?.SetTag(SERVER_PORT_KEY, _serverPort);
		_activity?.SetTag(GEN_AI_OPERATION_NAME_KEY, _operationName);
	}

	private void RecordMetrics(string? responseModel, string? errorType, int? inputTokensUsage, int? outputTokensUsage)
	{
		// tags is a struct, let's copy and modify them
		var tags = _commonTags ?? new();

		if (responseModel != null)
		{
			tags.Add(GEN_AI_RESPONSE_MODEL_KEY, responseModel);
		}

		if (inputTokensUsage != null)
		{
			var inputUsageTags = tags;
			inputUsageTags.Add(GEN_AI_TOKEN_TYPE_KEY, "input");
			_tokens.Record(inputTokensUsage.Value, inputUsageTags);
		}

		if (outputTokensUsage != null)
		{
			var outputUsageTags = tags;
			outputUsageTags.Add(GEN_AI_TOKEN_TYPE_KEY, "output");
			_tokens.Record(outputTokensUsage.Value, outputUsageTags);
		}

		if (errorType != null)
		{
			tags.Add(ERROR_TYPE_KEY, errorType);
		}

		_duration.Record(_durationTime!.Elapsed.TotalSeconds, tags);
	}

	private string? GetErrorType(Exception exception)
	{
		if (exception is ClientResultException requestFailedException)
		{
			// TODO when we start targeting .NET 8+ we should put
			// requestFailedException.InnerException.HttpRequestError into error.type
			return requestFailedException.Status.ToString();
		}

		return exception?.GetType()?.FullName;
	}

	private void SetActivityTagIfNotNull(string name, object? value)
	{
		if (value != null)
		{
			_activity?.SetTag(name, value);
		}
	}

	private void SetActivityTagIfNotNull(string name, int? value)
	{
		if (value.HasValue)
		{
			_activity?.SetTag(name, value.Value);
		}
	}

	private void SetActivityTagIfNotNull(string name, float? value)
	{
		if (value.HasValue)
		{
			_activity?.SetTag(name, value.Value);
		}
	}
}
