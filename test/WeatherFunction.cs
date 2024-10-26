using Microsoft.Extensions.AI;

namespace Tests;

internal class WeatherFunction : AIFunction
{
	public override AIFunctionMetadata Metadata => new("get_weather")
	{
		Description = "Gets the current weather for a current location",
		Parameters =
		[
			new AIFunctionParameterMetadata("city")
			{
				IsRequired = true,
				Description = "The city to get the weather for",
				ParameterType = typeof(string),
			},
			new AIFunctionParameterMetadata("unit")
			{
				IsRequired = false,
				Description = "The unit to calculate the current temperature to",
				ParameterType = typeof(string),
				DefaultValue = "celsius"
			}
		],
		ReturnParameter = new AIFunctionReturnParameterMetadata
		{
			Description = "The current weather in a given location",
			ParameterType = typeof(string),
			Schema = null
		}
	};

	protected override Task<object?> InvokeCoreAsync(IEnumerable<KeyValuePair<string, object?>> arguments, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}