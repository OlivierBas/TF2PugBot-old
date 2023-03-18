using TF2PugBot.Data;

namespace TF2PugBot.Types;

public class PlayerStats
{
    public ulong UserId { get; set; }
    public List<PlayerGuildStats> GuildStats { get; set; } = new List<PlayerGuildStats>();

    public PlayerGuildStats? GetGuildStat (ulong guildId)
    {
        return StatsManager.GetPlayerGuildStats(UserId, guildId);
    }


}