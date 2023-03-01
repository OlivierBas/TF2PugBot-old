using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
{
    private static List<GuildSettingsData> _guildSettingsData = new List<GuildSettingsData>();
    private static Dictionary<ulong, DateTime> _lastGuildGame = new Dictionary<ulong, DateTime>();
    public static async Task InitializeGuildDataAsync (SocketGuild guild)
    {
        if (_guildSettingsData.Any(g => g.GuildId == guild.Id))
        {
            return;
        }
        _guildSettingsData.Add(new GuildSettingsData() {GuildId = guild.Id});
        await SaveDbAsync();
    }

    public static void StartGuildGame (ulong guildId)
    {
        if (!_lastGuildGame.ContainsKey(guildId))
        {
            _lastGuildGame.Add(guildId, DateTime.Now);
        }
    }

    public static bool PreviousGuildGameEnded (ulong guildId)
    {
        if (_lastGuildGame[guildId].MinutesFromNow() > 5)
        {
            _lastGuildGame.Remove(guildId);
            return true;
        }

        return false;
    }
    
    public static Team? GetGuildTeamChannel (ulong guildId, ulong channelId)
    {
        var guildData =_guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
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
        var guildData =_guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
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