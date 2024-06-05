namespace OllamaSharp.Streamer;

public interface IResponseStreamer<in T>
{
	void Stream(T stream);
}