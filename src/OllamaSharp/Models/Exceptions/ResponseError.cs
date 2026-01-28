namespace OllamaSharp.Models.Exceptions;

/// <summary>
/// Represents an exception thrown when a response is an <see cref="ErrorResponse"/>.
/// </summary>
public class ResponseError : OllamaException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ResponseError"/> class with the specified error message.
	/// </summary>
	/// <param name="message">The error message that describes the exception.</param>
	public ResponseError(string message) : base(message)
	{ }
}