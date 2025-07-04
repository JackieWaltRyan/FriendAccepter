using System.Text.Json.Serialization;

namespace FriendAccepter;

internal sealed record FriendAccepterConfig {
    [JsonInclude]
    public bool AcceptFriends { get; set; }

    [JsonInclude]
    public bool GroupAutoPost { get; set; }

    [JsonInclude]
    public AutoPostConfig GroupAutoPostConfig { get; set; } = new();

    internal sealed record AutoPostConfig {
        [JsonInclude]
        public ulong GroupID { get; set; } = 103582791432987389;

        [JsonInclude]
        public string Comment { get; set; } = "Feel free to add me! Accept everyone!";

        [JsonInclude]
        public uint Timeout { get; set; } = 60;

        [JsonConstructor]
        public AutoPostConfig() { }
    }

    [JsonConstructor]
    public FriendAccepterConfig() { }
}

internal sealed record AddGroupCommentResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
