using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;

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

internal sealed class AddGroupCommentResponse {
    [JsonInclude]
    [JsonPropertyName("success")]
    internal bool Success { get; }

    [JsonConstructor]
    internal AddGroupCommentResponse() { }
}

internal sealed class FriendAccepter : IGitHubPluginUpdates, IBotModules, IBotFriendRequest {
    public string Name => nameof(FriendAccepter);
    public string RepositoryName => "JackieWaltRyan/FriendAccepter";
    public Version Version => typeof(FriendAccepter).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
    public Dictionary<string, bool> Bots = new();
    public Dictionary<string, Timer> BotsTimers = new();

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
    public async Task AutoPost(Bot bot, AutoPostConfig config) {
        if (!bot.IsConnectedAndLoggedOn) {
            // ReSharper disable once AsyncVoidLambda
            // ReSharper disable once UnusedParameter.Local
            BotsTimers[bot.BotName] = new Timer(async e => await AutoPost(bot, config).ConfigureAwait(false), null, TimeSpan.FromMinutes(1), TimeSpan.FromMicroseconds(-1));
        }

        ObjectResponse<AddGroupCommentResponse>? rawResponse = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AddGroupCommentResponse>(
            new Uri(ArchiWebHandler.SteamCommunityURL, $"/comment/Clan/post/{config.GroupID}/-1/"), data: new Dictionary<string, string>(4) {
                { "comment", config.Comment },
                { "count", "10" },
                { "feature2", "-1" }
            }
        ).ConfigureAwait(false);

        AddGroupCommentResponse? response = rawResponse?.Content;

        if (response == null) {
            // ReSharper disable once AsyncVoidLambda
            // ReSharper disable once UnusedParameter.Local
            BotsTimers[bot.BotName] = new Timer(async e => await AutoPost(bot, config).ConfigureAwait(false), null, TimeSpan.FromMinutes(1), TimeSpan.FromMicroseconds(-1));
        } else {
            bot.ArchiLogger.LogGenericInfo($"Add comment \"{config.Comment}\" to group {config.GroupID} return status {response.Success}.");

            uint timeout = response.Success ? config.Timeout : 1;

            bot.ArchiLogger.LogGenericInfo($"Next send comment: {DateTime.Now.AddMinutes(timeout):T}");

            // ReSharper disable once AsyncVoidLambda
            // ReSharper disable once UnusedParameter.Local
            BotsTimers[bot.BotName] = new Timer(async e => await AutoPost(bot, config).ConfigureAwait(false), null, TimeSpan.FromMinutes(timeout), TimeSpan.FromMicroseconds(-1));
        }
    }
}
