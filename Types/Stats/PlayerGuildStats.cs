namespace TF2PugBot.Types;

public class PlayerGuildStats
{
    public ulong GuildId { get; set; }
    public int GamesPlayed { get; set; }
    public int WonMedicSpins { get; set; }
    public int WonCaptainSpins { get; set; }
    public DateTime LastPlayed { get; set; } = DateTime.Now;

        public float MedSpinsWonPercentage => (GamesPlayed > 0 && WonMedicSpins > 0) ?
                                                (float)WonMedicSpins / GamesPlayed :
                                                0.0f; 
        public float CaptainSpinsWonPercentage => (GamesPlayed > 0 && WonCaptainSpins > 0) ?
                                                    (float)WonCaptainSpins / GamesPlayed :
                                                    0.0f;
}
