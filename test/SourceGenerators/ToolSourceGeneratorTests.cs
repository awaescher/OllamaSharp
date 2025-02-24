using FluentAssertions;
using NUnit.Framework;

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

			result.GeneratedTool.Function.Name.Should().Be("GetWeather");
			result.GeneratedTool.Function.Description.Should().Be("Gets the current weather for a given location.");
			result.GeneratedTool.Function.Parameters.Type.Should().Be("object");
			result.GeneratedTool.Function.Parameters.Required.Should().BeEquivalentTo(["location"]);
			result.GeneratedTool.Function.Parameters.Properties.Count.Should().Be(1);
			result.GeneratedTool.Function.Parameters.Properties["location"].Description.Should().Be("The location or city to get the weather for");
			result.GeneratedTool.Function.Parameters.Properties["location"].Enum.Should().BeNull();
			result.GeneratedTool.Function.Parameters.Properties["location"].Type.Should().Be("string");
			result.GeneratedTool.Type.Should().Be("function");

			result.GeneratedTool.GetType().Name.Should().Be("GetWeatherTool");
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

			result.GeneratedTool.Function.Name.Should().Be("GetWeather");
			result.GeneratedTool.Function.Description.Should().BeEmpty();
			result.GeneratedTool.Function.Parameters.Properties["location"].Description.Should().BeEmpty();
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

			result.GeneratedTool.Function.Parameters.Required.Should().BeEquivalentTo(["location", "unit"]);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Description.Should().Be("The unit to measure the temperature in");
			result.GeneratedTool.Function.Parameters.Properties["unit"].Enum.Should().BeEquivalentTo(["Celsius", "Fahrenheit"]);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Type.Should().Be("string");
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

			result.GeneratedTool.Function.Parameters.Properties["a"].Type.Should().Be("string");
			result.GeneratedTool.Function.Parameters.Properties["b"].Type.Should().Be("number");
			result.GeneratedTool.Function.Parameters.Properties["c"].Type.Should().Be("number");
			result.GeneratedTool.Function.Parameters.Properties["d"].Type.Should().Be("number");
			result.GeneratedTool.Function.Parameters.Properties["e"].Type.Should().Be("number");
			result.GeneratedTool.Function.Parameters.Properties["f"].Type.Should().Be("number");
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

			result.GeneratedTool.Function.Parameters.Required.Should().BeEquivalentTo(["location"]);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Description.Should().Be("The unit to measure the temperature in");
			result.GeneratedTool.Function.Parameters.Properties["unit"].Enum.Should().BeEquivalentTo(["Celsius", "Fahrenheit"]);
			result.GeneratedTool.Function.Parameters.Properties["unit"].Type.Should().Be("string");
		}

		[Test]
		public void Requires_Namespace()
		{
			var code = """
public class Test
{
	[OllamaTool]
	public static string GetWeather(string location) => $"It's cold at only 6° Celsius in {location}.";
}
""";

			var result = RunGenerator(code, allowErrors: true);

			result.Diagnostics.Should().ContainSingle(d => d.ToString().Contains("A namespace is required!"));
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
			result.GeneratedTool.Function.Name.Should().Be("GetWeather");
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
			toolResult.Should().Be(42);
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
			toolResult.Should().Be(42);
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
			toolResult.Should().Be(42);
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
			toolResult.Should().Be("It's cold at only 6° Fahrenheit in Berlin.");
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
			toolResult.Should().Be("It's cold at only 6° Celsius in Berlin.");
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
			await act.Should().ThrowAsync<ArgumentException>();
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
			toolResult.Should().Be("It's cold at only 6° Celsius in Berlin."); // <-- Celsius is the fallback value
		}
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.