using System.Text.Json.Serialization;

namespace FriendAccepter;

internal sealed class AutoPostConfig {
    [JsonInclude]
    internal ulong GroupID { get; set; } = 103582791432987389;

    [JsonInclude]
    internal string Comment { get; set; } = "Feel free to add me! Accept everyone!";

    [JsonInclude]
    internal uint Timeout { get; set; } = 60;

    [JsonConstructor]
    internal AutoPostConfig() { }
}

public class AddGroupCommentResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
