#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OllamaSharp.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class ListRunningModelsResponse
{
    [JsonPropertyName("models")] public RunningModel[] RunningModels { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
[DebuggerDisplay("{Name}")]
public class RunningModel
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("modified_at")] public DateTime? ModifiedAt { get; set; }

    [JsonPropertyName("size")] public long? Size { get; set; }

    [JsonPropertyName("size_vram")] public long? SizeVRAM { get; set; }

    [JsonPropertyName("digest")] public string? Digest { get; set; }

    [JsonPropertyName("details")] public Details? Details { get; set; }

    [JsonPropertyName("expires_at")] public DateTime? ExpiresAt { get; set; }
}