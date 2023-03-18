namespace TF2PugBot.Types;

public class GuildMapData
{
    public ulong GuildId { get; set; }
    public int HoursBeforeMapClear { get; set; } = 2;
    public List<SixesMap> Maps { get; set; } = new List<SixesMap>();
    public List<IgnoredSixesMap> IgnoredMaps { get; set; } = new List<IgnoredSixesMap>();
}