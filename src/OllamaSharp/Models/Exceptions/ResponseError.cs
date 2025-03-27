namespace OllamaSharp.Models.Exceptions;

/// <summary>
/// Represents an exception thrown when a response is a ErrorResponse.
/// </summary>
public class ResponseError : OllamaException
{
	/// <summary>
	/// </summary>
	/// <param name="message"></param>
	public ResponseError(string message) : base(message)
	{ }
}
