using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
{
    private static List<GuildSettingsData> _guildSettingsData = new List<GuildSettingsData>();
    private static Dictionary<ulong, GuildGameData> _trackedGuildGame = new Dictionary<ulong, GuildGameData>();

    public static async Task InitializeGuildDataAsync (SocketGuild guild)
    {
        if (_guildSettingsData.Any(g => g.GuildId == guild.Id))
        {
            return;
        }

        _guildSettingsData.Add(new GuildSettingsData() { GuildId = guild.Id });
        await SaveDbAsync();
    }

    public static void StartNewGuildGame (ulong guildId)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            _trackedGuildGame.Add(guildId, new GuildGameData());
        }
        else
        {
            _trackedGuildGame[guildId] = new GuildGameData();
        }
    }
    public static void StartNewGuildGame (ulong guildId, List<SocketGuildUser> players)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            _trackedGuildGame.Add(guildId, new GuildGameData() {Players = players.Select(p => p.Id).ToList(), LockPlayers = true});
        }
        else
        {
            _trackedGuildGame[guildId] = new GuildGameData()
            {
                Players = players.Select(p => p.Id).ToList(),
                LockPlayers = true
            };
        }
    }

    public static bool GuildGameHasEnded (ulong guildId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            if (_trackedGuildGame[guildId].StartDate.MinutesFromNow() >= Constants.GuildGameMinDuration)
            {
                return true;
            }
        }

        return false;
    }

    public static async Task<bool> TryEndGuildGame (ulong guildId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            var guildGame = _trackedGuildGame[guildId];
            if (guildGame.StartDate.MinutesFromNow() >= Constants.GuildGameMinDuration)
            {
                foreach (var player in guildGame.Players)
                {
                    await UpdatePlayerStatsAsync(player, guildId, StatTypes.GamesPlayed);
                }

                _trackedGuildGame[guildId] = new GuildGameData(); // reset the tracked guild game.
                return true;
            }   
        }
        return false;
    }
    

    /*public static void AddPlayersToGuildGame (ulong guildId, params ulong[] userIds)
    {
        if (_lastGuildGame.ContainsKey(guildId))
        {
            foreach (var userId in userIds)
            {
                if (_lastGuildGame[guildId].Players.Contains(userId))
                {
                    continue;
                }
                _lastGuildGame[guildId].Players.Add(userId);
            }
        }
    }*/

    public static void AddPlayerToGuildGame (ulong guildId, ulong userId)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            if (!_trackedGuildGame[guildId].LockPlayers)
            {
                _trackedGuildGame[guildId].Players.Add(userId);
            }
        }
        else
        {

            // Otherwise create a new trackedGuildGame and add the first player.
            _trackedGuildGame.Add(guildId, new GuildGameData() {Players = new List<ulong>() {userId}});
        }
    }

    public static void RemovePlayerFromGuildGame (ulong guildId, ulong userId)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            if (!_trackedGuildGame[guildId].LockPlayers)
            {
                _trackedGuildGame[guildId].Players.Remove(userId);
            }        }
    }

    public static Team? GetGuildTeamChannel (ulong guildId, ulong channelId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            if (guildData.BluTeamVoiceChannelId == channelId)
            {
                return Team.BLU;
            }

            if (guildData.RedTeamVoiceChannelId == channelId)
            {
                return Team.RED;
            }
        }

        return null;
    }

    public static bool GuildHasPingsEnabled (ulong guildId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            return guildData.PingOnSpin;
        }

        return false;
    }

    public static async Task SetGuildAdminRoleAsync (ulong? guildId, SocketRole role)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.AdminRoleId = role.Id;
            await SaveDbAsync();
        }
    }

    public static async Task SetGuildPingsAsync (ulong? guildId, bool value)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.PingOnSpin = value;
            await SaveDbAsync();
        }
    }

    public static async Task<bool> UpdateGuildChannelDataAsync (ulong guildId, Team team, ulong channelId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            bool success = guildData.TryUpdateValue(team, channelId);
            if (success)
            {
                await SaveDbAsync();
                return success;
            }
        }

        return false;
    }
}