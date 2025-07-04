using System.Text.Json.Serialization;

namespace FriendAccepter;

internal sealed record FriendAccepterConfig {
    [JsonInclude]
    public bool AcceptFriends { get; set; }

    [JsonInclude]
    public GroupAutoPostConfig GroupAutoPost { get; set; } = new();

    internal sealed record GroupAutoPostConfig {
        [JsonInclude]
        public bool Enable { get; set; }

        [JsonInclude]
        public ulong GroupID { get; set; } = 103582791432987389;

        [JsonInclude]
        public string Comment { get; set; } = "Feel free to add me! Accept everyone!";

        [JsonInclude]
        public uint Timeout { get; set; } = 60;

        [JsonConstructor]
        public GroupAutoPostConfig() { }
    }

    [JsonConstructor]
    public FriendAccepterConfig() { }
}

internal sealed record AddGroupCommentResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
