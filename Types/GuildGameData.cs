namespace TF2PugBot.Types;

public class GuildGameData
{
    public List<ulong> Players { get; set; } = new List<ulong>();
    public DateTime StartDate { get; set; } = DateTime.Now;
}