﻿using System;
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

    public Dictionary<string, FriendAccepterConfig> FriendAccepterConfig = new();
    public Dictionary<string, Dictionary<string, Timer>> FriendAccepterTimers = new();

    public Task OnLoaded() => Task.CompletedTask;

    public async Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null) {
        if (additionalConfigProperties != null) {
            if (FriendAccepterTimers.TryGetValue(bot.BotName, out Dictionary<string, Timer>? dict)) {
                foreach (KeyValuePair<string, Timer> timers in dict) {
                    await timers.Value.DisposeAsync().ConfigureAwait(false);

                    bot.ArchiLogger.LogGenericInfo($"{timers.Key} Dispose.");
                }
            }

            FriendAccepterTimers[bot.BotName] = new Dictionary<string, Timer> {
                { "GroupAutoPost", new Timer(async e => await GroupAutoPost(bot).ConfigureAwait(false), null, Timeout.Infinite, Timeout.Infinite) }
            };

            FriendAccepterConfig[bot.BotName] = new FriendAccepterConfig();

            foreach (KeyValuePair<string, JsonElement> configProperty in additionalConfigProperties) {
                switch (configProperty.Key) {
                    case "FriendAccepterConfig": {
                        FriendAccepterConfig? config = configProperty.Value.ToJsonObject<FriendAccepterConfig>();

                        if (config != null) {
                            FriendAccepterConfig[bot.BotName] = config;
                        }

                        break;
                    }
                }
            }

            if (FriendAccepterConfig[bot.BotName].AcceptFriends || FriendAccepterConfig[bot.BotName].GroupAutoPost) {
                bot.ArchiLogger.LogGenericInfo($"FriendAccepterConfig: {FriendAccepterConfig[bot.BotName].ToJsonText()}");

                if (FriendAccepterConfig[bot.BotName].GroupAutoPost) {
                    FriendAccepterTimers[bot.BotName]["GroupAutoPost"].Change(1, -1);
                }
            }
        }
    }

    public Task<bool> OnBotFriendRequest(Bot bot, ulong steamID) {
        if (FriendAccepterConfig.TryGetValue(bot.BotName, out FriendAccepterConfig? value) && value.AcceptFriends) {
            bot.ArchiLogger.LogGenericInfo($"User: {steamID} | Status: OK");

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task GroupAutoPost(Bot bot) {
        uint timeout = 1;

        if (bot.IsConnectedAndLoggedOn) {
            FriendAccepterConfig.AutoPostConfig config = FriendAccepterConfig[bot.BotName].GroupAutoPostConfig;

            ObjectResponse<AddGroupCommentResponse>? rawResponse = await bot.ArchiWebHandler.UrlPostToJsonObjectWithSession<AddGroupCommentResponse>(
                new Uri($"{ArchiWebHandler.SteamCommunityURL}/comment/Clan/post/{config.GroupID}/-1/"), data: new Dictionary<string, string>(4) {
                    { "comment", config.Comment },
                    { "count", "10" },
                    { "feature2", "-1" }
                }, referer: new Uri($"{ArchiWebHandler.SteamCommunityURL}/gid/{config.GroupID}/comments")
            ).ConfigureAwait(false);

            AddGroupCommentResponse? response = rawResponse?.Content;

            if (response != null) {
                if (response.Success) {
                    timeout = config.Timeout;
                }

                bot.ArchiLogger.LogGenericInfo($"Group: {config.GroupID} | Comment: {config.Comment} | Status: {response.Success} | Next send: {DateTime.Now.AddMinutes(timeout):T}");
            } else {
                bot.ArchiLogger.LogGenericInfo($"Group: {config.GroupID} | Comment: {config.Comment} | Status: Error | Next send: {DateTime.Now.AddMinutes(timeout):T}");
            }
        } else {
            bot.ArchiLogger.LogGenericInfo($"Status: BotNotConnected | Next send: {DateTime.Now.AddMinutes(timeout):T}");
        }

        FriendAccepterTimers[bot.BotName]["GroupAutoPost"].Change(TimeSpan.FromMinutes(timeout), TimeSpan.FromMilliseconds(-1));
    }
}
