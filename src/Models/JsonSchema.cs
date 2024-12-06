using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

public class JsonSchema
{
	/// <summary>
	/// Gets or sets the type of the schema, default is "object".
	/// </summary>
	[JsonPropertyName("type")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? Type { get; set; } = "object";

	/// <summary>
	/// Gets or sets the properties of the schema.
	/// </summary>
	[JsonPropertyName("properties")]
	public Dictionary<string, Property>? Properties { get; set; }

	/// <summary>
	/// Gets or sets a list of required fields within the schema.
	///	</summary>
	[JsonPropertyName("required")]
	public IEnumerable<string>? Required { get; set; }

	/// <summary>
	/// Get the JsonSchema from Type, use typeof(Class) to get the Type of Class.
	/// </summary>

	public static JsonSchema ToJsonSchema(Type type)
	{
		var required = new List<string>();
		var properties = new Dictionary<string, Property>();
		foreach (var property in type.GetProperties())
		{
			var propertyName = property.Name;
			var propertyType = property.PropertyType;

			var isEnumerable = typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string);
			var isNullable = Nullable.GetUnderlyingType(propertyType) != null;
			if (isNullable)
				propertyType = Nullable.GetUnderlyingType(propertyType);

			properties.Add(propertyName,
				new Property
				{
					Type = isEnumerable ? "array" : propertyType.Name.ToLower(),
					Items = isEnumerable
						? new Item
						{
							Type = (propertyType.IsArray
								? propertyType.GetElementType()
								: propertyType.GetGenericArguments().First()).Name.ToLower()
						}
						: null
				});

			if (!isNullable)
				required.Add(propertyName);
		}

		return new JsonSchema { Properties = properties, Required = required };
	}
}

public class Property
{
	/// <summary>
	/// Gets or sets the type of the property.
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// Gets or sets the items of the property.
	/// </summary>
	[JsonPropertyName("items")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public Item? Items { get; set; }
}

public class Item
{
	/// <summary>
	/// Gets or sets the type of the item.
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }
}
