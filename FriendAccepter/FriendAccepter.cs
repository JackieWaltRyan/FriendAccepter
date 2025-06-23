using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;

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

internal sealed class FriendAccepter : IGitHubPluginUpdates, IBotModules, IBotFriendRequest {
    public string Name => nameof(FriendAccepter);
    public string RepositoryName => "JackieWaltRyan/FriendAccepter";
    public Version Version => typeof(FriendAccepter).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
    public Dictionary<string, bool> Bots = new();

    public Task OnLoaded() => Task.CompletedTask;

    public async Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties == null) {
            return;
        }

        bool autoPostEnabled = false;
        AutoPostConfig autoPostConfig = new();

        foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
            switch (configProperty.Key) {
                case "EnableFriendAccepter" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                    bool isEnabled = configProperty.Value.GetBoolean();

                    bot.ArchiLogger.LogGenericInfo($"Enable Friend Accepter: {isEnabled}");

                    Bots[bot.BotName] = isEnabled;

                    break;
                }

                case "FriendAccepterAutoPost": {
                    if (configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False) {
                        bool isEnabled = configProperty.Value.GetBoolean();

                        bot.ArchiLogger.LogGenericInfo($"Friend Auto Post: {isEnabled}");

                        autoPostEnabled = isEnabled;

                        bot.ArchiLogger.LogGenericInfo($"Friend Auto Post Config: {autoPostConfig.ToJsonText()}");
                    } else {
                        AutoPostConfig? filter = configProperty.Value.ToJsonObject<AutoPostConfig>();

                        if (filter != null) {
                            autoPostEnabled = true;

                            bot.ArchiLogger.LogGenericInfo($"Friend Auto Post: {autoPostEnabled}");

                            autoPostConfig = filter;

                            bot.ArchiLogger.LogGenericInfo($"Friend Auto Post Config: {filter.ToJsonText()}");
                        }
                    }

                    break;
                }
            }
        }

        if (autoPostEnabled) {
            await AutoPost(bot, autoPostConfig).ConfigureAwait(false);
        }
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) {
        if (!Bots[bot.BotName]) {
            return Task.FromResult(false);
        }

        bot.ArchiLogger.LogGenericInfo($"User {steamID} add to friend.");

        return Task.FromResult(true);
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    public static async Task AutoPost(Bot bot, AutoPostConfig config) {
        try {
            if (!bot.IsConnectedAndLoggedOn) {
                await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                await AutoPost(bot, config).ConfigureAwait(false);
            }

            bot.ArchiLogger.LogGenericInfo("AutoPost is run.");

            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await AutoPost(bot, config).ConfigureAwait(false);
        } catch {
            await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            await AutoPost(bot, config).ConfigureAwait(false);
        }
    }
}
