using System.Text.Json.Serialization;

namespace FriendAccepter;

public class GroupAutoPostConfig {
    [JsonInclude]
    public ulong GroupID { get; set; } = 103582791432987389;

    [JsonInclude]
    public string Comment { get; set; } = "Feel free to add me! Accept everyone!";

    [JsonInclude]
    public uint Timeout { get; set; } = 60;

    [JsonConstructor]
    public GroupAutoPostConfig() { }
}

internal sealed record AddGroupCommentResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
