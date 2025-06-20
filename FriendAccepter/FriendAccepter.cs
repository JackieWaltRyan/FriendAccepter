using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;

namespace FriendAccepter;

internal sealed class FriendAccepter : IGitHubPluginUpdates, IBotModules, IBotFriendRequest {
    public string Name => nameof(FriendAccepter);
    public string RepositoryName => "JackieWaltRyan/FriendAccepter";
    public Version Version => typeof(FriendAccepter).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
    public Dictionary<string, bool> Bots = new();

    public Task OnLoaded() => Task.CompletedTask;

    public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties == null) {
            return Task.CompletedTask;
        }

        foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
            switch (configProperty.Key) {
                case "EnableFriendAccepter" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                    bool isEnabled = configProperty.Value.GetBoolean();

                    bot.ArchiLogger.LogGenericInfo($"Enable Friend Accepter: {isEnabled}");

                    Bots.Add(bot.BotName, isEnabled);

                    break;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) {
        if (!Bots[bot.BotName]) {
            return Task.FromResult(false);
        }

        bot.ArchiLogger.LogGenericInfo($"User {steamID} add to friend.");

        return Task.FromResult(true);
    }
}
