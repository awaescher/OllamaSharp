using System;

namespace OllamaSharp.Models.Exceptions;

/// <summary>
/// Represents an exception thrown when a model does not support the requested tools.
/// </summary>
public class ModelDoesNotSupportToolsException : OllamaException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ModelDoesNotSupportToolsException"/> class.
	/// </summary>
	public ModelDoesNotSupportToolsException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelDoesNotSupportToolsException"/> class with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public ModelDoesNotSupportToolsException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModelDoesNotSupportToolsException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
	public ModelDoesNotSupportToolsException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
