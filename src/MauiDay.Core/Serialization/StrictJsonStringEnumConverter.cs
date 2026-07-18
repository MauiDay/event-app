using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiDay.Core.Serialization;

/// <summary>
/// A string-only enum converter that rejects numeric enum values. This keeps
/// malformed configuration and Sessionize payloads (for example a raw integer or
/// an unknown status) as an explicit deserialization failure instead of silently
/// mapping to an undefined enum value.
/// </summary>
public sealed class StrictJsonStringEnumConverter<T> : JsonStringEnumConverter<T>
    where T : struct, Enum
{
    public StrictJsonStringEnumConverter()
        : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {
    }
}
