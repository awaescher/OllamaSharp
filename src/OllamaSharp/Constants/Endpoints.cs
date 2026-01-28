namespace OllamaSharp.Constants;

/// <summary>
/// Provides a collection of constant endpoint URLs used by the API in the OllamaSharp library.
/// </summary>
/// <remarks>
/// <p>
/// This static class contains various string constants that represent API endpoints. These constants are used primarily
/// in API client implementations for making requests to specific functionality provided by the backend API.
/// </p>
/// </remarks>
internal static class Endpoints
{
	public const string CreateModel = "api/create";
	public const string DeleteModel = "api/delete";
	public const string ListLocalModels = "api/tags";
	public const string ListRunningModels = "api/ps";
	public const string ShowModel = "api/show";
	public const string CopyModel = "api/copy";
	public const string PullModel = "api/pull";
	public const string PushModel = "api/push";
	public const string Embed = "api/embed";
	public const string Chat = "api/chat";
	public const string Version = "api/version";
	public const string Generate = "api/generate";
}