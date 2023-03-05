using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
{
    private static List<PlayerStats> _playerStats = new List<PlayerStats>();

    private static PlayerStats? GetPlayerStats (ulong userId)
    {
        var ps = _playerStats.FirstOrDefault(p => p.UserId == userId);
        if (ps is null)
        {
            TryGeneratePlayerStats(userId, null);
            return _playerStats.FirstOrDefault(p => p.UserId == userId)!;
        }

        return ps;
    }

    public static PlayerGuildStats? GetPlayerGuildStats (ulong userId, ulong guildId)
    {
        try
        {
            PlayerStats?      ps  = GetPlayerStats(userId);
            PlayerGuildStats? psg = ps?.GuildStats.FirstOrDefault(g => g.GuildId == guildId);
            if (psg is null)
            {
                TryGeneratePlayerStats(userId, guildId);
                return ps?.GuildStats.FirstOrDefault(g => g.GuildId == guildId);
            }

            return psg;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }

        return null;
    }

    private static void TryGeneratePlayerStats (ulong userId, ulong? guildId)
    {
        if (!_playerStats.Exists(p => p.UserId == userId))
        {
            var newPs = new PlayerStats()
            {
                UserId = userId,
            };
            if (guildId is not null)
            {
                newPs.GuildStats.Add(new PlayerGuildStats()
                {
                    GuildId    = guildId.GetValueOrDefault(),
                    LastPlayed = DateTime.Now
                });
            }

            _playerStats.Add(newPs);
        }
        else
        {
            var ps = _playerStats.FirstOrDefault(p => p.UserId == userId);
            ps!.GuildStats = new List<PlayerGuildStats>()
            {
                new PlayerGuildStats()
                {
                    GuildId    = guildId.GetValueOrDefault(),
                    LastPlayed = DateTime.Now
                },
            };
        }
    }

    public static async Task UpdatePlayerStatsAsync (ulong userId, ulong guildId, StatTypes stat)
    {
        var ps = GetPlayerGuildStats(userId, guildId);
        if (ps is not null)
        {
            switch (stat)
            {
                case StatTypes.GamesPlayed:
                    ps.GamesPlayed++;
                    ps.LastPlayed = DateTime.Now;
                    break;
                case StatTypes.CaptainSpinsWon:
                    ps.WonCaptainSpins++;
                    break;
                case StatTypes.MedicSpinsWon:
                    ps.WonMedicSpins++;
                    break;
            }

            await SaveDbAsync();
        }
    }
}