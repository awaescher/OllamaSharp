# Tool support

Many [AI models support tools](https://ollama.com/search?c=tools), also known as "function calling". This enables an AI model to answer a given prompt using tools it knows about, making it possible for models to perform more complex tasks or interact with the outside world. [Please visit the official Ollama blog to learn more about it](https://ollama.com/blog/tool-support).

[OllamaSharp](https://github.com/awaescher/OllamaSharp) had very early support for tools. However it forced developers to build complex json structures to define the tools' meta data. In addition, the developer had to figure out which tools the model wanted to call with which arguments and do this in the models behalf.

Starting with [version 5.1.1](https://github.com/awaescher/OllamaSharp/releases/tag/5.1.1) defining and implementing tools has been dramatically simplified.

## What is a tool?

For Ollama, a tool is just the definition of a function that an AI model can potentially call. The AI model will decide by the given context whether or not it wants to make use of the tools provided. The following example is taken from the official Ollama blog:

``` json
tools=[
    {
      'type': 'function',
      'function': {
        'name': 'get_current_weather',
        'description': 'Get the current weather for a city',
        'parameters': {
          'type': 'object',
          'properties': {
            'city': {
              'type': 'string',
              'description': 'The name of the city',
            },
          },
          'required': ['city'],
        },
      },
    },
  ]
```

By passing this information to an AI model in addition to the prompt(s), the model knows that there's a way to query real weather data including the arguments it needs to provide to get the weather for a given city.

## Defining tools with OllamaSharp

Tools are available for the `/api/chat` endpoint in Ollama. The simplest way to build a chat is by using the `Chat` class provided by OllamaSharp. This class will automatically handle the whole interaction between the human and the AI model. Its `SendAsync()` method provides an overload that allows the developer to include images and tool definitions in addition to prompts.

These tool definitions are of type `object` to support any form of tool definition like shown below or modeled with JsonSchema, etc.

### Manual tool definition

In earlier versions, tools had to be defined as `OllamaSharp.Tool` which is a data transfer object that matches the required json from the Ollama API.

This is how the prior example would look like with an early version of OllamaSharp:

``` csharp
public class WeatherTool : Tool
{
    public WeatherTool()
    {
        Function = new Function
        {
            Description = "Get the current weather for a city",
            Name = "get_current_weather",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["city"] = new() { Type = "string", Description = "Name of the city" }
                },
                Required = ["city"],
            }
        };
        Type = "function";
    }
}
```

**It's still possible to define tools this way** but there are three major drawbacks:
1. it's pretty verbose to define tools in C# that way
2. it introduces a dependency to OllamaSharp which can be an issue for tools that are defined in independent projects
3. tool calls have to be discovered, executed and fed back to the chat messages manually

### Automatic tool generation and execution ([v5.1.1+](https://github.com/awaescher/OllamaSharp/pull/171))

OllamaSharp ships with a [source generator](https://learn.microsoft.com/en-us/shows/on-dotnet/c-source-generators) that will find tool definitions automatically and generate the required source code that bridges tool calls made by the AI model to the corresponding code that needs to be executed. It's as simple as writing a method and decorating it with the `[OllamaTool]` attribute.

For the example above, the only code that's required would be:

``` csharp
public class SampleTools
{
	/// <summary>
	/// Get the current weather for a city
	/// </summary>
	/// <param name="city">Name of the city</param>
	[OllamaTool]
	public static string GetWeather(string city) => ...;
}
```

To bring some more details in, let's extend the example with an optional argument with fixed values and a very simple implementation:

``` csharp
public class SampleTools
{
	/// <summary>
	/// Get the current weather for a city
	/// </summary>
	/// <param name="city">Name of the city</param>
	/// <param name="unit">Temperature unit for the weather</param>
	[OllamaTool]
	public static string GetWeather(string city, Unit unit = Unit.Celsius) => $"It's cold at only 6° {unit} in {city}.";

    public enum Unit
    {
        Celsius,
        Fahrenheit
    }
}
```

This way, OllamaSharp will automatically create the source code for the tool with the same name of the method + "Tool" appendix. In this case `GetWeatherTool` in the same namespace as the class `SampleTools` is located.

#### Usage

Pass instances of the desired tools with your message like this:

``` csharp
var chat = new Chat(...);

await foreach (var answerToken in chat.SendAsync("How's the weather in Stuttgart?", [new GetWeatherTool()]))
    Console.WriteLine(answerToken);
```

OllamaSharp will automatically match tool calls from the AI model with the provided tools, call the tools and return results back into the chat so that the AI model can continue.

#### Important details

 - the entire meta data definition from the json structure above will automatically be taken from the **method signature and its summary**, this includes
   - function name
   - function description
   - arguments
     - description
     - return type
     - enum (values to chose from)
     - required/optional
 - generated tools will get the **name of the method with the appendix "Tool"**: `GetWeather()` → `GetWeatherTool`
 - generated tools will be located in the **same namespace** as their definitions
 - **enum arguments are parsed** from the AI model's response
   - If the AI model provides an invalid enum value, like `"kelvin"` in the example above, an `ArgumentException` will occur. To prevent this, you can provide a default value like shown in the example above that is used if the AI model provides no or invalid enum values.
  - tools are **automatically invoked** if the AI model requests to do so
    - the tool implementation itself is not duplicated but gets executed from the generated tool. This allows easy debugging.
    - the tool's result value will automatically be back-propagated to the chat so that the AI model can continue working.
    - the entire tool invocation behavior can be modified by changing the `Chat.ToolInvoker` instance.
 - the project containing the tools **must generate a documentation file**, otherwise the tools' summaries are lost after compilation. Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to the corresponding project file.

 #### Limitations

 Automatic tool generation and execution has the following limitations:

  - cannot be used with non-static instance methods <sup>**_planned_**</sup>
  - cannot be used with interfaces to only define the meta data without providing an implementation <sup>**_planned_**</sup>
  - only available for C#. Visual Basic and F# are not supported <sup>**_not planned_**</sup>

 > The project containing the Ollama tools must generate a documentation file, see "Important details".
 
## Model context protocol (MCP) servers

OllamaSharp also supports the [model context protocol](https://modelcontextprotocol.io/introduction). In the past, we shipped a small package `OllamaSharp.ModelContextProtocol` for this but we discontinued it because of the quickly evolving nature of the MCP standard. 

Instead, we highly recommend using the [official C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) in combination with OllamaSharp or libraries that build upon OllamaSharp such as [Semantic Kernel or the Microsoft Agent Framework](https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-and-microsoft-agent-framework/).

The community made some samples how to combine the MCP SDK with OllamaSharp
- [Invoke MCP tool from local LLM using OllamaSharp](https://www.youtube.com/watch?v=NBlIZ2TlHsU)
- [Tiny code sample by @strabu](https://github.com/strabu/ollama-mcp-csharp/blob/3ed3f587e15dec94a67fa2bceea191e3a6da5e73/src/OllamaPlaywrightMCPExample/Program.cs#L1)

For larger projects, we recommend using frameworks like Semantic Kernel instead of OllamaSharp directly. Semantic Kernel is a huge library maintained by Microsoft and makes use of OllamaSharp behind the scenes when talking to Ollama endpoints. Once you moved to Semantic Kernel (+OllamaSharp), you can find a lot of resources and further extensions. A broad support for MCP is one of the benefits as well as countless resources like the following, for example: [Building a Model Context Protocol Server with Semantic Kernel](https://devblogs.microsoft.com/semantic-kernel/building-a-model-context-protocol-server-with-semantic-kernel/).
