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
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<string>? Required { get; set; }

	/// <summary>
	/// Get the JsonSchema from Type, use typeof(Class) to get the Type of Class.
	/// </summary>
	public static JsonSchema ToJsonSchema(Type type)
	{
		var properties = type.GetProperties().ToDictionary(
			prop => prop.Name,
			prop => new Property
			{
				Type = GetTypeName(prop.PropertyType),
				Items = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string)
					? new Item
					{
						Type = IsPrimitiveType(prop.PropertyType.IsArray
							? prop.PropertyType.GetElementType()
							: prop.PropertyType.GetGenericArguments().First())
							? GetTypeName(prop.PropertyType.IsArray
								? prop.PropertyType.GetElementType()
								: prop.PropertyType.GetGenericArguments().First())
							: "object",
						Properties = IsPrimitiveType(prop.PropertyType.IsArray
							? prop.PropertyType.GetElementType()
							: prop.PropertyType.GetGenericArguments().First())
							? null
							: ToJsonSchema(prop.PropertyType.IsArray
								? prop.PropertyType.GetElementType()
								: prop.PropertyType.GetGenericArguments().First()).Properties,
						Required = IsPrimitiveType(prop.PropertyType.IsArray
							? prop.PropertyType.GetElementType()
							: prop.PropertyType.GetGenericArguments().First())
							? null
							: (prop.PropertyType.IsArray
								? prop.PropertyType.GetElementType()
								: prop.PropertyType.GetGenericArguments().First()).GetProperties()
							.Where(info =>
								!info.PropertyType.IsGenericType ||
								info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>) ||
								Nullable.GetUnderlyingType(info.PropertyType) != null)
							.Select(info => info.Name)
							.ToList()
					}
					: null
			}
		);

		var required = type.GetProperties()
			.Where(prop =>
				!prop.PropertyType.IsGenericType ||
				prop.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>) ||
				Nullable.GetUnderlyingType(prop.PropertyType) != null)
			.Select(prop => prop.Name)
			.ToList();

		return new JsonSchema { Properties = properties, Required = required };
	}

	private static bool IsPrimitiveType(Type type)
	{
		return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
	}

	private static string GetTypeName(Type type)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			var underlyingType = type.GetGenericArguments().First();
			return GetTypeName(underlyingType);
		}

		var typeCode = System.Type.GetTypeCode(type);

		switch (typeCode)
		{
			case TypeCode.Int32:
			case TypeCode.Int16:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int64:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return "integer";
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return "number";
			case TypeCode.Boolean:
				return "boolean";
			case TypeCode.DateTime:
			case TypeCode.String:
				return "string";
			case TypeCode.Object:
				if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
					return "array";
				return "object";
			default:
				return "object";
		}
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

	/// <summary>
	/// Gets or sets the description of the property.
	/// </summary>
	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the Enum of the property.
	/// </summary>
	[JsonPropertyName("enum")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<string>? Enum { get; set; }
}

public class Item
{
	/// <summary>
	/// Gets or sets the type of the item.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// Gets or sets the properties of the item.
	/// </summary>
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("properties")]
	public Dictionary<string, Property>? Properties { get; set; }

	/// <summary>
	/// Gets or sets a list of required fields within the item.
	///	</summary>
	[JsonPropertyName("required")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IEnumerable<string>? Required { get; set; }
}
