using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using OllamaSharp.Models.Chat.Converter;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// A description of the intended purpose of a message within a chat completions interaction.
/// </summary>
[JsonConverter(typeof(ChatRoleConverter))]
public readonly struct ChatRole : IEquatable<ChatRole>
{
	private readonly string _value;

	/// <summary> Initializes a new instance of <see cref="ChatRole"/>. </summary>
	/// <exception cref="ArgumentNullException"> <paramref name="role"/> is null. </exception>
	public ChatRole(string? role)
	{
		_value = role ?? throw new ArgumentNullException(nameof(role));
	}

	[JsonConstructor]
	public ChatRole(object _)
	{
		_value = null!;
	}

	private const string SYSTEM_VALUE = "system";
	private const string ASSISTANT_VALUE = "assistant";
	private const string USER_VALUE = "user";
	private const string TOOL_VALUE = "tool";

	/// <summary>
	/// The role that instructs or sets the behavior of the assistant.
	/// </summary>
	public static ChatRole System { get; } = new(SYSTEM_VALUE);

	/// <summary>
	/// The role that provides responses to system-instructed, user-prompted input.
	/// </summary>
	public static ChatRole Assistant { get; } = new(ASSISTANT_VALUE);

	/// <summary>
	/// The role that provides input for chat completions.
	/// </summary>
	public static ChatRole User { get; } = new(USER_VALUE);

	/// <summary>
	/// The role that is used to input the result from an external tool.
	/// </summary>
	public static ChatRole Tool { get; } = new(TOOL_VALUE);

	/// <summary>
	/// Determines if two <see cref="ChatRole"/> values are the same.
	/// </summary>
	public static bool operator ==(ChatRole left, ChatRole right) => left.Equals(right);

	/// <summary>
	/// Determines if two <see cref="ChatRole"/> values are not the same.
	/// </summary>
	public static bool operator !=(ChatRole left, ChatRole right) => !left.Equals(right);

	/// <summary>
	/// Converts a string to a <see cref="ChatRole"/>.
	/// </summary>
	public static implicit operator ChatRole(string value) => new(value);

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj) => obj is ChatRole other && Equals(other);

	/// <inheritdoc />
	public bool Equals(ChatRole other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode() => _value?.GetHashCode() ?? 0;

	/// <inheritdoc />
	public override string ToString() => _value;
}