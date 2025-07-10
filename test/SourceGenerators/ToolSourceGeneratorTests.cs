using NUnit.Framework;
using Shouldly;

namespace Tests.SourceGenerators;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class ToolSourceGeneratorTests : SourceGeneratorTest
{
	public class MetaDataTests : ToolSourceGeneratorTests
	{
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

	public class StaticInvocationTests : ToolSourceGeneratorTests
	{
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