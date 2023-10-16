using System;

public class ActionResponseStreamer<T> : IResponseStreamer<T>
{
	public Action<T> ResponseHandler { get; }

	public ActionResponseStreamer(Action<T> responseHandler)
    {
		ResponseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
	}

	public void Stream(T stream)
	{
		ResponseHandler(stream);
	}
}
