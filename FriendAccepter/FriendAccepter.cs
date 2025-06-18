using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;

namespace FriendAccepter;

internal sealed class FriendAccepter : IGitHubPluginUpdates, IBotModules, IBotFriendRequest {
    public string Name => nameof(FriendAccepter);
    public string RepositoryName => "JackieWaltRyan/FriendAccepter";
    public Version Version => typeof(FriendAccepter).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
    public Dictionary<Bot, bool> EnabledBot = new();

    public Task OnLoaded() {
        ASF.ArchiLogger.LogGenericInfo($"{Name} Plugin is Loaded!");

        return Task.CompletedTask;
    }

    public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties == null) {
            return Task.CompletedTask;
        }

        foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
            switch (configProperty.Key) {
                case "EnableFriendAccepter" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                    bool enabled = configProperty.Value.GetBoolean();

                    bot.ArchiLogger.LogGenericInfo($"Enable Friend Accepter: {enabled}");

                    EnabledBot.Add(bot, enabled);

                    break;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) => Task.FromResult(EnabledBot[bot]);
}
