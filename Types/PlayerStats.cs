namespace TF2PugBot.Types;

public class PlayerStats
{
    public ulong UserId { get; set; }
    public List<PlayerGuildStats> GuildStats { get; set; } = new List<PlayerGuildStats>();
}