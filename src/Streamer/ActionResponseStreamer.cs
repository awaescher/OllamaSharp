using System;

namespace OllamaSharp.Streamer;

/// <summary>
/// A class that implements the IResponseStreamer interface and handles 
/// streaming responses using a provided action.
/// </summary>
/// <typeparam name="T">The type of the response.</typeparam>
public class ActionResponseStreamer<T> : IResponseStreamer<T>
{
	/// <summary>
	/// Gets the action that handles the streamed response.
	/// </summary>
	public Action<T> ResponseHandler { get; }

	/// <summary>
	/// Initializes a new instance of the 
	/// <see cref="ActionResponseStreamer{T}"/> class.
	/// </summary>
	/// <param name="responseHandler">
	/// The action that handles the streamed response.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when the responseHandler is null.
	/// </exception>
	public ActionResponseStreamer(Action<T> responseHandler)
	{
		ResponseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
	}

	/// <summary>
	/// Streams the response by invoking the response handler action.
	/// </summary>
	/// <param name="stream">The response to be streamed.</param>
	public void Stream(T stream) => ResponseHandler(stream);
}