namespace OllamaSharp;

/// <summary>
/// Specifies that the class or method is a tool for Ollama.
/// OllamaSharp will generate an implementation of this class or method with the name suffix -Tool.
/// If your method is named "GetWeather", the generated class will be named "GetWeatherTool".
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OllamaToolAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="OllamaToolAttribute"/> class.
	/// </summary>
	public OllamaToolAttribute() { }
}
