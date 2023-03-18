using System.Text;
using System.Text.Json;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static class DataManager
{
    private static string _token = string.Empty;
    private static bool _instantSpin;
    private static ulong _devId;

    public static string Token
    {
        get => _token;
        set
        {
            if (string.IsNullOrEmpty(_token))
            {
                _token = value;
            }
        }
    }

    public static bool InstantSpin
    {
        get => _instantSpin;
        set => _instantSpin = value;
    }

    public static ulong DevId
    {
        get => _devId;
        set => _devId = value;
    }

    private static string MedImmunityDb
    {
        get => EasySetup.MedDbFileName + ".json";
    }

    private static string GuildDataDb
    {
        get => EasySetup.GuildDbFileName + ".json";
    }
    private static string MapDataDb
    {
        get => EasySetup.MapsDbFileName + ".json";
    }

    private static string PlayerStatsDb
    {
        get => EasySetup.StatsDbFileName + ".json";
    }


    static DataManager ()
    {
        RetrieveDb();
    }

    private static void RetrieveDb ()
    {
        using (FileStream fs = new FileStream(MedImmunityDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = sr.ReadToEnd();
                if (data.Length > 0)
                {
                    MedManager.MedImmunities = JsonSerializer.Deserialize<List<MedicImmunePlayer>>(data)!;
                }
            }
        }

        using (FileStream fs = new FileStream(GuildDataDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = sr.ReadToEnd();
                if (data.Length > 0)
                {
                    GuildManager.GuildSettings = JsonSerializer.Deserialize<List<GuildSettingsData>>(data)!;
                }
            }
        }

        using (FileStream fs = new FileStream(PlayerStatsDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = sr.ReadToEnd();
                if (data.Length > 0)
                {
                    StatsManager.PlayerStats = JsonSerializer.Deserialize<List<PlayerStats>>(data)!;
                }
            }
        }
        using (FileStream fs = new FileStream(MapDataDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = sr.ReadToEnd();
                if (data.Length > 0)
                {
                    MapManager.GuildMapData = JsonSerializer.Deserialize<List<GuildMapData>>(data)!;
                }
            }
        }
    }

    public static async Task SaveDbAsync (SaveType save)
    {
        switch (save)
        {
            case SaveType.MedImmunities:
                await using (FileStream fs = new FileStream(MedImmunityDb, FileMode.Truncate, FileAccess.Write))
                {
                    await using (StreamWriter sw = new StreamWriter(fs))
                    {
                        var storedImmunities = MedManager.MedImmunities.DistinctBy(mi => new { mi.Id, mi.GuildId })
                                                         .ToList();
                        await sw.WriteAsync(JsonSerializer.Serialize(storedImmunities));
                    }
                }
                break;
            
            case SaveType.PlayerStats:
                await using (FileStream fs = new FileStream(PlayerStatsDb, FileMode.Truncate, FileAccess.Write))
                {
                    await using (StreamWriter sw = new StreamWriter(fs))
                    {
                        await sw.WriteAsync(JsonSerializer.Serialize(StatsManager.PlayerStats));
                    }
                }
                break;
            
            case SaveType.GuildData:
                await using (FileStream fs = new FileStream(GuildDataDb, FileMode.Truncate, FileAccess.Write))
                {
                    await using (StreamWriter sw = new StreamWriter(fs))
                    {
                        await sw.WriteAsync(JsonSerializer.Serialize(GuildManager.GuildSettings));
                    }
                }
                break;
            case SaveType.GuildMaps:
                await using (FileStream fs = new FileStream(MapDataDb, FileMode.Truncate, FileAccess.Write))
                {
                    await using (StreamWriter sw = new StreamWriter(fs))
                    {
                        await sw.WriteAsync(JsonSerializer.Serialize(MapManager.GuildMapData));
                    }
                }
                break;
        }
    }
}