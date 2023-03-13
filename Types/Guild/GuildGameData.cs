namespace TF2PugBot.Types;

public class GuildGameData
{
    public List<ulong> Players { get; set; } = new List<ulong>();
    public bool LockPlayers { get; set; } = false;
    public DateTime StartDate { get; set; } = DateTime.Now;
}