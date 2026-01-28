using NUnit.Framework;
using Shouldly;

namespace Tests.SourceGenerators;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

/// <summary>
/// Base test class for the tool source generator tests.
/// </summary>
public class ToolSourceGeneratorTests : SourceGeneratorTest
{
	/// <summary>
	/// Contains tests that verify metadata generation from XML documentation comments.
	/// </summary>
	public class MetaDataTests : ToolSourceGeneratorTests
	{
		/// <summary>
		/// Verifies that the generator extracts function name, description and parameter information from a method's summary and param tags.
		/// </summary>
		[Test]
		public void Generates_Meta_Data_From_Summary()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° C in {location}.";
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Name.ShouldBe("GetWeather");
			result.GeneratedTool.Function.Description.ShouldBe("Gets the current weather for a given location.");
			result.GeneratedTool.Function.Parameters.Type.ShouldBe("object");
			result.GeneratedTool.Function.Parameters.Required.ShouldBe(["location"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties.Count.ShouldBe(1);
			result.GeneratedTool.Function.Parameters.Properties["location"].Description.ShouldBe("The location or city to get the weather for");
			result.GeneratedTool.Function.Parameters.Properties["location"].Enum.ShouldBeNull();
			result.GeneratedTool.Function.Parameters.Properties["location"].Type.ShouldBe("string");
			result.GeneratedTool.Type.ShouldBe("function");

			result.GeneratedTool.GetType().Name.ShouldBe("GetWeatherTool");
		}

		/// <summary>
		/// Ensures that a method without a summary still generates a tool with an empty description.
		/// </summary>
		[Test]
		public void Allows_No_Summary()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° C in {location}.";
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Name.ShouldBe("GetWeather");
			result.GeneratedTool.Function.Description.ShouldBeEmpty();
			result.GeneratedTool.Function.Parameters.Properties["location"].Description.ShouldBeEmpty();
		}

		/// <summary>
		/// Checks that line breaks inside a summary are normalized into a single description string.
		/// </summary>
		[Test]
		public void Allows_Line_Breaks_In_Summary()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	/// <summary>
	/// Gets the current weather
	/// for a given
	/// 
	/// location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° C in {location}.";
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Description.ShouldBe("Gets the current weather for a given location.");
		}

		/// <summary>
		/// Validates that enum parameters are represented with their possible values and string type.
		/// </summary>
		[Test]
		public void Supports_Enums()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public static string GetWeather(string location, Unit unit) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Parameters.Required.ShouldBe(["location", "unit"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Description.ShouldBe("The unit to measure the temperature in");
			result.GeneratedTool.Function.Parameters.Properties["unit"].Enum.ShouldBe(["Celsius", "Fahrenheit"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Type.ShouldBe("string");
		}

		/// <summary>
		/// Ensures that boolean parameters, including nullable and optional variants, are handled correctly.
		/// </summary>
		[Test]
		public void Supports_Booleans()
		{
			var code = """
namespace TestNamespace;
public class Test
{
    /// <summary>
    /// Tests boolean parameter handling.
    /// </summary>
    /// <param name="required">A required boolean parameter</param>
    /// <param name="nullableRequired">A required nullable boolean</param>
    /// <param name="optionalTrue">An optional boolean with true default</param>
    /// <param name="nullableOptionalNull">An optional nullable boolean with null default</param>
    /// <param name="nullableOptionalTrue">An optional nullable boolean with true default</param>
    [OllamaTool]
    public static string TestBooleans(
        bool required, 
        bool? nullableRequired,
        bool optionalTrue = true, 
        bool? nullableOptionalNull = null,
        bool? nullableOptionalTrue = true) => "test";
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Parameters.Required.ShouldBe(["required", "nullableRequired"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties["required"].Description.ShouldBe("A required boolean parameter");
			result.GeneratedTool.Function.Parameters.Properties["optionalTrue"].Description.ShouldBe("An optional boolean with true default");
			result.GeneratedTool.Function.Parameters.Properties["nullableRequired"].Description.ShouldBe("A required nullable boolean");
			result.GeneratedTool.Function.Parameters.Properties["nullableOptionalNull"].Description.ShouldBe("An optional nullable boolean with null default");
			result.GeneratedTool.Function.Parameters.Properties["nullableOptionalTrue"].Description.ShouldBe("An optional nullable boolean with true default");

			result.GeneratedCode.ShouldContain("bool required = (bool)args[\"required\"];");
			result.GeneratedCode.ShouldContain("bool? nullableRequired = (bool?)args[\"nullableRequired\"];");
			result.GeneratedCode.ShouldContain("bool optionalTrue = args.ContainsKey(\"optionalTrue\") ? (bool)args[\"optionalTrue\"] : true;");
			result.GeneratedCode.ShouldContain("bool? nullableOptionalNull = args.ContainsKey(\"nullableOptionalNull\") ? (bool?)args[\"nullableOptionalNull\"] : null;");
			result.GeneratedCode.ShouldContain("bool? nullableOptionalTrue = args.ContainsKey(\"nullableOptionalTrue\") ? (bool?)args[\"nullableOptionalTrue\"] : true;");
		}

		/// <summary>
		/// Checks that string and numeric arguments are mapped to the correct JSON schema types.
		/// </summary>
		[Test]
		public void Supports_String_And_Number_Arguments()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static object Foo(string a, int b, short c, long d, float e, double f) => null;
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Parameters.Properties["a"].Type.ShouldBe("string");
			result.GeneratedTool.Function.Parameters.Properties["b"].Type.ShouldBe("number");
			result.GeneratedTool.Function.Parameters.Properties["c"].Type.ShouldBe("number");
			result.GeneratedTool.Function.Parameters.Properties["d"].Type.ShouldBe("number");
			result.GeneratedTool.Function.Parameters.Properties["e"].Type.ShouldBe("number");
			result.GeneratedTool.Function.Parameters.Properties["f"].Type.ShouldBe("number");
		}

		/// <summary>
		/// Verifies that optional arguments are not marked as required and retain their metadata.
		/// </summary>
		[Test]
		public void Supports_Optional_Arguments()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	/// <summary>
	/// Gets the current weather for a given location.
	/// </summary>
	/// <param name="location">The location or city to get the weather for</param>
	/// <param name="unit">The unit to measure the temperature in</param>
	/// <returns>The weather for the given location</returns>
	[OllamaTool]
	public static string GetWeather(string location, Unit unit = Unit.Celsius) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";

			var result = RunGenerator(code);

			result.GeneratedTool.Function.Parameters.Required.ShouldBe(["location"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Description.ShouldBe("The unit to measure the temperature in");
			result.GeneratedTool.Function.Parameters.Properties["unit"].Enum.ShouldBe(["Celsius", "Fahrenheit"], ignoreOrder: true);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Type.ShouldBe("string");
		}

		/// <summary>
		/// Confirms that a tool can be generated when the source file has no explicit namespace.
		/// </summary>
		[Test]
		public void Allows_Default_Namespace()
		{
			var code = """
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° Celsius in {location}.";
}
""";

			var result = RunGenerator(code);
			result.GeneratedTool.ShouldNotBeNull();
		}

		/// <summary>
		/// Ensures that a tool can be generated from a static class definition.
		/// </summary>
		[Test]
		public void Requires_Class_Definition()
		{
			var code = """
namespace TestNamespace;
public static class Test
{
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° Celsius in {location}.";
}
""";

			var result = RunGenerator(code, allowErrors: true);
			result.GeneratedTool.Function.Name.ShouldBe("GetWeather");
		}
	}

	/// <summary>
	/// Contains tests that verify invocation of generated tools using static methods.
	/// </summary>
	public class StaticInvocationTests : ToolSourceGeneratorTests
	{
		/// <summary>
		/// Checks that a simple static method can be invoked through the generated tool.
		/// </summary>
		[Test]
		public async Task Can_Be_Invoked()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static short GetRandomNumber() => 42;
}
""";
			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result);
			toolResult.ShouldBe(42);
		}

		/// <summary>
		/// Verifies that tools inside static classes are invocable.
		/// </summary>
		[Test]
		public async Task Can_Be_Invoked_Within_Static_Classes()
		{
			var code = """
namespace TestNamespace;
public static class Test
{
	[OllamaTool]
	public static short GetRandomNumber() => 42;
}
""";
			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result);
			toolResult.ShouldBe(42);
		}

		/// <summary>
		/// Ensures that asynchronous static methods returning a Task are handled correctly.
		/// </summary>
		[Test]
		public async Task Can_Be_Invoked_As_Async_Task()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static async Task<short> GetRandomNumber()
	{
		await Task.Delay(100);
		return 42;
	}
}
""";
			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result);
			toolResult.ShouldBe(42);
		}

		/// <summary>
		/// Tests that arguments are passed correctly to the generated tool.
		/// </summary>
		[Test]
		public async Task Uses_Arguments()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location, Unit unit) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";

			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result, new() { ["location"] = "Berlin", ["unit"] = "Fahrenheit" });
			toolResult.ShouldBe("It's cold at only 6° Fahrenheit in Berlin.");
		}

		/// <summary>
		/// Confirms that optional arguments can be omitted when invoking the tool.
		/// </summary>
		[Test]
		public async Task Allows_Skipping_Optional_Arguments()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location, Unit unit = Unit.Celsius) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";

			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result, new() { ["location"] = "Berlin" });
			toolResult.ShouldBe("It's cold at only 6° Celsius in Berlin.");
		}

		/// <summary>
		/// Verifies that an invalid enum value for a required argument throws an exception.
		/// </summary>
		[Test]
		public async Task Throws_On_Invalid_Enums_If_Argument_Is_Required()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location, Unit unit) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";
			var result = RunGenerator(code);

			Func<Task> act = async () => await InvokeTool(result, new() { ["location"] = "Berlin", ["unit"] = "Kelvin" }); // <- Kelvin is not part of the enum
			await act.ShouldThrowAsync<ArgumentException>();
		}

		/// <summary>
		/// Ensures that an invalid enum value for an optional argument falls back to the default.
		/// </summary>
		[Test]
		public async Task Allows_Invalid_Enums_For_Optional_Arguments()
		{
			var code = """
namespace TestNamespace;
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location, Unit unit = Unit.Celsius) => $"It's cold at only 6° {unit} in {location}.";

	public enum Unit
	{
		Celsius,
		Fahrenheit
	}
}
""";
			var result = RunGenerator(code);
			var toolResult = await InvokeTool(result, new() { ["location"] = "Berlin", ["unit"] = "Kelvin" }); // <- Kelvin is not part of the enum
			toolResult.ShouldBe("It's cold at only 6° Celsius in Berlin."); // <-- Celsius is the fallback value
		}
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.