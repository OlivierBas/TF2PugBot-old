using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
{
    private static List<GuildSettingsData> _guildSettingsData = new List<GuildSettingsData>();
    private static Dictionary<ulong, GuildGameData> _lastGuildGame = new Dictionary<ulong, GuildGameData>();
    private static Dictionary<ulong, List<ulong>> _guildGamePlayers = new Dictionary<ulong, List<ulong>>();

    public static async Task InitializeGuildDataAsync (SocketGuild guild)
    {
        if (_guildSettingsData.Any(g => g.GuildId == guild.Id))
        {
            return;
        }

        _guildSettingsData.Add(new GuildSettingsData() { GuildId = guild.Id });
        await SaveDbAsync();
    }

    public static void StartGuildGame (ulong guildId)
    {
        if (!_lastGuildGame.ContainsKey(guildId))
        {
            _lastGuildGame.Add(guildId, new GuildGameData());
        }
        else
        {
            _lastGuildGame[guildId].StartDate = DateTime.Now;
        }
    }


    public static async Task<bool> PreviousGuildGameEndedAsync (ulong guildId, bool clear)
    {
        if (_lastGuildGame.ContainsKey(guildId))
        {
            if (_lastGuildGame[guildId].StartDate.MinutesFromNow() > 5)
            {
                if (clear)
                {
                    foreach (var player in _lastGuildGame[guildId].Players)
                    {
                        await UpdatePlayerStatsAsync(player, guildId, StatTypes.GamesPlayed);
                    }

                    _lastGuildGame.Remove(guildId);
                }

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
        if (!_guildGamePlayers.ContainsKey(guildId))
        {
            _guildGamePlayers.Add(guildId, new List<ulong>());
        }

        _guildGamePlayers[guildId].Add(userId);
    }

    public static void RemovePlayerFromGuildGame (ulong guildId, ulong userId)
    {
        if (_guildGamePlayers.ContainsKey(guildId))
        {
            _guildGamePlayers[guildId].Remove(userId);
        }
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