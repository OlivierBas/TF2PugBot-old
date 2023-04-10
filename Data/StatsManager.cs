using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static class StatsManager
{
    private static List<PlayerStats> _playerStats = new List<PlayerStats>();

    public static List<PlayerStats> PlayerStats
    {
        get => _playerStats;
        set => _playerStats = value;
    }

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

    public static List<PlayerStats> GetPlayerStatsOfGuild (ulong guildId)
    {
        return _playerStats.Where(ps => ps?.GuildStats.FirstOrDefault(psg => psg.GuildId == guildId) is not null)
                           .ToList();
    }

    public static PlayerGuildStats? GetPlayerGuildStats (ulong userId, ulong guildId)
    {
        PlayerStats?      ps  = GetPlayerStats(userId);
        PlayerGuildStats? psg = ps?.GuildStats.FirstOrDefault(g => g.GuildId == guildId);
        if (psg is null)
        {
            TryGeneratePlayerStats(userId, guildId);
            return ps?.GuildStats.FirstOrDefault(g => g.GuildId == guildId);
        }

        return psg;


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
            if (ps.GuildStats.Count == 0)
            {
                ps.GuildStats = new List<PlayerGuildStats>();
            }
            else
            {
                ps.GuildStats.Add(new PlayerGuildStats()
                {
                    GuildId    = guildId.GetValueOrDefault(),
                    LastPlayed = DateTime.Now
                });
            }
        }
    }

    public static async Task UpdatePlayerStatsAsync (ulong guildId, StatTypes stat, params ulong[] userIds)
    {
        foreach (var userId in userIds)
        {
            var psg = GetPlayerGuildStats(userId, guildId);
            Console.WriteLine($"({guildId}) Increasing {userId}'s {stat}");
            if (psg is not null)
            {
                switch (stat)
                {
                    case StatTypes.GamesPlayed:
                        psg.GamesPlayed++;
                        psg.LastPlayed = DateTime.Now;
                        break;
                    case StatTypes.CaptainSpinsWon:
                        psg.WonCaptainSpins++;
                        break;
                    case StatTypes.MedicSpinsWon:
                        psg.WonMedicSpins++;
                        break;
                }
            }

            await DataManager.SaveDbAsync(SaveType.PlayerStats);
        }
    }
}