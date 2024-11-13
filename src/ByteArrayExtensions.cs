using System;
using System.Collections.Generic;
using System.Linq;

namespace OllamaSharp;

/// <summary>
/// Extensions for byte arrays
/// </summary>
internal static class ByteArrayExtensions
{
	/// <summary>
	/// Converts a sequence of bytes to its equivalent string representation encoded in base-64.
	/// </summary>
	/// <param name="bytes">The sequence of bytes to convert to a base-64 string.</param>
	/// <returns>A base-64 encoded string representation of the input byte sequence.</returns>
	public static string ToBase64(this IEnumerable<byte> bytes) => Convert.ToBase64String(bytes.ToArray());


	/// <summary>
	/// Converts a collection of byte arrays to a collection of base64 strings.
	/// </summary>
	/// <param name="byteArrays">The collection of byte arrays to convert to base64 strings.</param>
	/// <returns>A collection of base64 strings, or null if the input is null.</returns>
	public static IEnumerable<string>? ToBase64(this IEnumerable<IEnumerable<byte>>? byteArrays) => byteArrays?.Select(bytes => bytes.ToBase64());
}