namespace OllamaSharp.Telemetry;

internal class OpenTelemetryConstants
{
	// follow OpenTelemetry GenAI semantic conventions:
	// https://github.com/open-telemetry/semantic-conventions/tree/v1.27.0/docs/gen-ai

	public const string ERROR_TYPE_KEY = "error.type";
	public const string SERVER_ADDRESS_KEY = "server.address";
	public const string SERVER_PORT_KEY = "server.port";

	public const string GEN_AI_CLIENT_OPERATION_DURATION_METRIC_NAME = "gen_ai.client.operation.duration";
	public const string GEN_AI_CLIENT_TOKEN_USAGE_METRIC_NAME = "gen_ai.client.token.usage";

	public const string GEN_AI_OPERATION_NAME_KEY = "gen_ai.operation.name";

	public const string GEN_AI_REQUEST_MAX_TOKENS_KEY = "gen_ai.request.max_tokens";
	public const string GEN_AI_REQUEST_MODEL_KEY = "gen_ai.request.model";
	public const string GEN_AI_REQUEST_TEMPERATURE_KEY = "gen_ai.request.temperature";
	public const string GEN_AI_REQUEST_TOP_P_KEY = "gen_ai.request.top_p";
	public const string GEN_AI_REQUEST_TOP_K_KEY = "gen_ai.request.top_k";
	public const string GEN_AI_REQUEST_PRESENCE_PENALTY_KEY = "gen_ai.request.presence_penalty";
	public const string GEN_AI_REQUEST_STOP_SEQUENCES_KEY = "gen_ai.request.stop_sequences";

	public const string GEN_AI_PROMPT_KEY = "gen_ai.prompt";

	public const string GEN_AI_RESPONSE_ID_KEY = "gen_ai.response.id";
	public const string GEN_AI_RESPONSE_FINISH_REASON_KEY = "gen_ai.response.finish_reasons";
	public const string GEN_AI_RESPONSE_MODEL_KEY = "gen_ai.response.model";

	public const string GEN_AI_SYSTEM_KEY = "gen_ai.system";
	public const string GEN_AI_SYSTEM_VALUE = "ollamasharp";

	public const string GEN_AI_TOKEN_TYPE_KEY = "gen_ai.token.type";

	public const string GEN_AI_USAGE_INPUT_TOKENS_KEY = "gen_ai.usage.input_tokens";
	public const string GEN_AI_USAGE_OUTPUT_TOKENS_KEY = "gen_ai.usage.output_tokens";
}
