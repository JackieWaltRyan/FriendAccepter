using System;
using System.Collections.Generic;
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

    public Dictionary<string, bool> AcceptFriendsEnable = new();
    public Dictionary<string, bool> GroupAutoPostEnable = new();

    public Dictionary<string, GroupAutoPostConfig> GroupAutoPostConfig = new();
    public Dictionary<string, Timer> GroupAutoPostTimers = new();

    public Task OnLoaded() => Task.CompletedTask;

    public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties != null) {
            AcceptFriendsEnable[bot.BotName] = false;
            GroupAutoPostEnable[bot.BotName] = false;

            GroupAutoPostConfig[bot.BotName] = new GroupAutoPostConfig();
            GroupAutoPostTimers[bot.BotName] = new Timer(async e => await GroupAutoPost(bot).ConfigureAwait(false), null, Timeout.Infinite, Timeout.Infinite);

            foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
                switch (configProperty.Key) {
                    case "AcceptFriendsEnable" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                        bool isEnabled = configProperty.Value.GetBoolean();

                        bot.ArchiLogger.LogGenericInfo($"AcceptFriendsEnable: {isEnabled}");

                        AcceptFriendsEnable[bot.BotName] = isEnabled;

                        break;
                    }

                    case "GroupAutoPostEnable" when configProperty.Value.ValueKind is JsonValueKind.True or JsonValueKind.False: {
                        bool isEnabled = configProperty.Value.GetBoolean();

                        bot.ArchiLogger.LogGenericInfo($"GroupAutoPostEnable: {isEnabled}");

                        GroupAutoPostEnable[bot.BotName] = isEnabled;

                        break;
                    }

                    case "GroupAutoPostConfig": {
                        GroupAutoPostConfig? config = configProperty.Value.ToJsonObject<GroupAutoPostConfig>();

                        if (config != null) {
                            GroupAutoPostConfig[bot.BotName] = config;
                        }

                        bot.ArchiLogger.LogGenericInfo($"GroupAutoPostConfig: {GroupAutoPostConfig[bot.BotName].ToJsonText()}");

                        break;
                    }
                }
            }

            if (GroupAutoPostEnable[bot.BotName]) {
                GroupAutoPostTimers[bot.BotName].Change(1, -1);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) {
        if (AcceptFriendsEnable[bot.BotName]) {
            bot.ArchiLogger.LogGenericInfo($"User: {steamID} | Status: OK");

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task GroupAutoPost(Bot bot) {
        if (bot.IsConnectedAndLoggedOn) {
            ObjectResponse<AddGroupCommentResponse>? rawResponse = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AddGroupCommentResponse>(
                new Uri(ArchiWebHandler.SteamCommunityURL, $"/comment/Clan/post/{GroupAutoPostConfig[bot.BotName].GroupID}/-1/"), data: new Dictionary<string, string>(4) {
                    { "comment", GroupAutoPostConfig[bot.BotName].Comment },
                    { "count", "10" },
                    { "feature2", "-1" }
                }
            ).ConfigureAwait(false);

            AddGroupCommentResponse? response = rawResponse?.Content;

            uint timeout = 1;

            if (response != null) {
                if (response.Success) {
                    timeout = GroupAutoPostConfig[bot.BotName].Timeout;
                }

                bot.ArchiLogger.LogGenericInfo($"Group: {GroupAutoPostConfig[bot.BotName].GroupID} | Comment: {GroupAutoPostConfig[bot.BotName].Comment} | Status: {response.Success} | Next send: {DateTime.Now.AddMinutes(timeout):T}");
            } else {
                bot.ArchiLogger.LogGenericInfo($"Group: {GroupAutoPostConfig[bot.BotName].GroupID} | Comment: {GroupAutoPostConfig[bot.BotName].Comment} | Status: Error | Next send: {DateTime.Now.AddMinutes(timeout):T}");
            }

            GroupAutoPostTimers[bot.BotName].Change(TimeSpan.FromMinutes(timeout), TimeSpan.FromMilliseconds(-1));
        } else {
            bot.ArchiLogger.LogGenericInfo($"Group: {GroupAutoPostConfig[bot.BotName].GroupID} | Comment: {GroupAutoPostConfig[bot.BotName].Comment} | Status: BotNotConnected | Next send: {DateTime.Now.AddSeconds(10):T}");

            GroupAutoPostTimers[bot.BotName].Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
        }
    }
}
