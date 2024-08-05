using System;

namespace OllamaSharp.Models.Exceptions;

public class ModelDoesNotSupportToolsException : OllamaException
{
	public ModelDoesNotSupportToolsException()
	{
	}

	public ModelDoesNotSupportToolsException(string message) : base(message)
	{
	}

	public ModelDoesNotSupportToolsException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
