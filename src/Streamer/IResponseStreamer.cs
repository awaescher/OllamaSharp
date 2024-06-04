namespace OllamaSharp.Streamer;

public interface IResponseStreamer<T>
{
	void Stream(T stream);
}