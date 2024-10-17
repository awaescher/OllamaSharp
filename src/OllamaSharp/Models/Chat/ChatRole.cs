using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using OllamaSharp.Models.Chat.Converter;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// Represents a role within a chat completions interaction, describing the intended purpose of a message.
/// </summary>
[JsonConverter(typeof(ChatRoleConverter))]
public readonly struct ChatRole : IEquatable<ChatRole>
{
	private readonly string _value;

	/// <summary>
	/// Initializes a new instance of <see cref="ChatRole"/> with the specified role.
	/// </summary>
	/// <param name="role">The role to initialize with.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is null.</exception>
	public ChatRole(string? role)
	{
		_value = role ?? throw new ArgumentNullException(nameof(role));
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ChatRole"/> using a JSON constructor.
	/// </summary>
	/// <param name="_">The placeholder parameter for JSON constructor.</param>
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
	/// Gets the role that instructs or sets the behavior of the assistant.
	/// </summary>
	public static ChatRole System { get; } = new(SYSTEM_VALUE);

	/// <summary>
	/// Gets the role that provides responses to system-instructed, user-prompted input.
	/// </summary>
	public static ChatRole Assistant { get; } = new(ASSISTANT_VALUE);

	/// <summary>
	/// Gets the role that provides input for chat completions.
	/// </summary>
	public static ChatRole User { get; } = new(USER_VALUE);

	/// <summary>
	/// Gets the role that is used to input the result from an external tool.
	/// </summary>
	public static ChatRole Tool { get; } = new(TOOL_VALUE);

	/// <summary>
	/// Determines if two <see cref="ChatRole"/> instances are equal.
	/// </summary>
	/// <param name="left">The first <see cref="ChatRole"/> to compare.</param>
	/// <param name="right">The second <see cref="ChatRole"/> to compare.</param>
	/// <returns><c>true</c> if both instances are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(ChatRole left, ChatRole right) => left.Equals(right);

	/// <summary>
	/// Determines if two <see cref="ChatRole"/> instances are not equal.
	/// </summary>
	/// <param name="left">The first <see cref="ChatRole"/> to compare.</param>
	/// <param name="right">The second <see cref="ChatRole"/> to compare.</param>
	/// <returns><c>true</c> if both instances are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(ChatRole left, ChatRole right) => !left.Equals(right);

	/// <summary>
	/// Implicitly converts a string to a <see cref="ChatRole"/>.
	/// </summary>
	/// <param name="value">The string value to convert.</param>
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
