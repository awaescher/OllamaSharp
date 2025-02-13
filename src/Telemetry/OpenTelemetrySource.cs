using OllamaSharp.Models.Chat;

namespace OllamaSharp.Telemetry;

internal class OpenTelemetrySource
{
	private const string CHAT_OPERATION_NAME = "chat";
	private readonly bool _isOTelEnabled = AppContextSwitchHelper
		.GetConfigValue("OllamaSharp.Experimental.EnableOpenTelemetry", "OLLAMASHARP_EXPERIMENTAL_ENABLE_OPEN_TELEMETRY");

	private readonly string _serverAddress;
	private readonly int _serverPort;

	public OpenTelemetrySource(Uri endpoint)
	{
		_serverAddress = endpoint.Host;
		_serverPort = endpoint.Port;
	}

	public OpenTelemetryScope? StartChatScope(ChatRequest completionsOptions)
	{
		return _isOTelEnabled
			? OpenTelemetryScope.StartChat(completionsOptions.Model, CHAT_OPERATION_NAME, _serverAddress, _serverPort, completionsOptions)
			: null;
	}

}
