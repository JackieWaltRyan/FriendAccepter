using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;

namespace FriendAccepter;

internal sealed class FriendAccepter : IGitHubPluginUpdates, IBotModules, IBotFriendRequest {
    public string Name => nameof(FriendAccepter);
    public string RepositoryName => "JackieWaltRyan/FriendAccepter";
    public Version Version => typeof(FriendAccepter).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

    public Dictionary<string, bool> BotsEnable = new();
    public Dictionary<string, Timer> BotsTimers = new();

    public Task OnLoaded() => Task.CompletedTask;

    public async Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties == null) {
            return;
        }

        foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
            switch (configProperty.Key) {
                case "EnableFriendAccepter" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                    bool isEnabled = configProperty.Value.GetBoolean();

                    bot.ArchiLogger.LogGenericInfo($"Enable Friend Accepter: {isEnabled}");

                    BotsEnable[bot.BotName] = isEnabled;

                    break;
                }

                case "FriendAccepterAutoPost": {
                    bool autoPostEnabled = false;
                    AutoPostConfig autoPostConfig = new();

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

                    if (autoPostEnabled) {
                        if (BotsTimers.TryGetValue(bot.BotName, out Timer? value)) {
                            await value.DisposeAsync().ConfigureAwait(false);
                        }

                        // ReSharper disable once AsyncVoidLambda
                        // ReSharper disable once UnusedParameter.Local
                        BotsTimers[bot.BotName] = new Timer(async e => await AutoPost(bot, autoPostConfig).ConfigureAwait(false), null, TimeSpan.FromSeconds(0), TimeSpan.FromMicroseconds(-1));
                    }

                    break;
                }
            }
        }
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) {
        if (!BotsEnable.TryGetValue(bot.BotName, out bool value) || (BotsEnable[bot.BotName] = !value)) {
            return Task.FromResult(false);
        }

        bot.ArchiLogger.LogGenericInfo($"User {steamID} add to friend.");

        return Task.FromResult(true);
    }

    public async Task AutoPost(Bot bot, AutoPostConfig config) {
        uint timeout = 1;

        if (bot.IsConnectedAndLoggedOn) {
            ObjectResponse<JsonElement>? rawResponse = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<JsonElement>(
                new Uri(ArchiWebHandler.SteamCommunityURL, $"/comment/Clan/post/{config.GroupID}/-1/"), data: new Dictionary<string, string>(4) {
                    { "comment", config.Comment },
                    { "count", "10" },
                    { "feature2", "-1" }
                }, referer: new Uri(ArchiWebHandler.SteamCommunityURL, $"/groups/{config.GroupID}/comments"), session: ArchiWebHandler.ESession.Lowercase, headers: new ReadOnlyCollection<KeyValuePair<string, string>>([new KeyValuePair<string, string>("Cookie", bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookieHeader(ArchiWebHandler.SteamCommunityURL))])
            ).ConfigureAwait(false);

            JsonElement? response = rawResponse?.Content;

            if (response != null) {
                bot.ArchiLogger.LogGenericInfo(response.ToJsonText());

                // bot.ArchiLogger.LogGenericInfo($"Add comment \"{config.Comment}\" to group {config.GroupID} return status {response.Success}.");

                // timeout = response.Success ? config.Timeout : 1;

                bot.ArchiLogger.LogGenericInfo($"Next send comment: {DateTime.Now.AddMinutes(timeout):T}");
            }
        }

        if (BotsTimers.TryGetValue(bot.BotName, out Timer? value)) {
            await value.DisposeAsync().ConfigureAwait(false);
        }

        // ReSharper disable once AsyncVoidLambda
        // ReSharper disable once UnusedParameter.Local
        BotsTimers[bot.BotName] = new Timer(async e => await AutoPost(bot, config).ConfigureAwait(false), null, TimeSpan.FromMinutes(timeout), TimeSpan.FromMicroseconds(-1));
    }
}
