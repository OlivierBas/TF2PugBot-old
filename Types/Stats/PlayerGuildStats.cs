namespace TF2PugBot.Types;

public class PlayerGuildStats
{
    public ulong GuildId { get; set; }
    public int GamesPlayed { get; set; }
    public int WonMedicSpins { get; set; }
    public int WonCaptainSpins { get; set; }
    public DateTime LastPlayed { get; set; } = DateTime.Now;

    public float MedSpinsWonPercentage => MathF.Round((float)WonMedicSpins / GamesPlayed, 2);
    public float CaptainSpinsWonPercentage => MathF.Round((float)WonCaptainSpins / GamesPlayed, 2);
}