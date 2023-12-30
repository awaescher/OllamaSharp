using System.Text.Json;

namespace OllamaSharp.Models.Chat.Options
{
    internal static class Optional
    {
        public static bool IsDefined<T>(T? value) where T: struct
        {
            return value.HasValue;
        }
        public static bool IsDefined(object value)
        {
            return value != null;
        }
        public static bool IsDefined(string value)
        {
            return value != null;
        }

        public static bool IsDefined(JsonElement value)
        {
            return value.ValueKind != JsonValueKind.Undefined;
        }

        public static T? ToNullable<T>(Optional<T> optional) where T: struct
        {
            if (optional.HasValue)
            {
                return optional.Value;
            }
            return default;
        }

        public static T? ToNullable<T>(Optional<T?> optional) where T: struct
        {
            return optional.Value;
        }
    }

    internal readonly partial struct Optional<T>
    {
        public Optional(T value) : this()
        {
            Value = value;
            HasValue = true;
        }

        public T Value { get; }
        public bool HasValue { get; }

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);
        public static implicit operator T(Optional<T> optional) => optional.Value;
    }
}