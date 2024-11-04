using System;
using System.Collections.Generic;
using System.Linq;

namespace OllamaSharp;

/// <summary>
/// Extensions for byte arrays
/// </summary>
public static class ByteArrayExtensions
{
	/// <summary>
	/// Converts a series of bytes to a base64 string
	/// </summary>
	/// <param name="bytes">The bytes to convert to base64</param>
	public static string ToBase64(this IEnumerable<byte>? bytes) => Convert.ToBase64String(bytes.ToArray());

	/// <summary>
	/// Converts multiple series of bytes to multiple base64 strings, one for each.
	/// </summary>
	/// <param name="byteArrays">The series of bytes to convert to base64</param>
	public static IEnumerable<string>? ToBase64(this IEnumerable<IEnumerable<byte>>? byteArrays) => byteArrays?.Select(ToBase64);
}