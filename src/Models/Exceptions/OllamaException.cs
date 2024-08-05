using System;

namespace OllamaSharp.Models.Exceptions;

public class OllamaException : Exception
{
	public OllamaException()
	{
	}

	public OllamaException(string message) : base(message)
	{
	}

	public OllamaException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
