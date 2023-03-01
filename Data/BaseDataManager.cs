using System.Text;
using System.Text.Json;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
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
    
    private static string PlayerStatsDb
    {
        get => EasySetup.StatsDbFileName + ".json";
    }

    static DataManager ()
    {
        RetrieveDb();
    }

    public static bool HasAccessToCommand (ulong? guildId, SocketGuildUser caller)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            if (caller.Roles.Any(r => r.Id == guildData.AdminRoleId))
            {
                return true;
            }

            if (caller.Roles.Any(r => r.Permissions.Administrator))
            {
                return true;
            }

            if (caller.Guild.OwnerId == caller.Id)
            {
                return true;
            }

            if (caller.Id == _devId)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task RetrieveDbAsync ()
    {

        await using (FileStream fs = new FileStream(MedImmunityDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = await sr.ReadToEndAsync();
                if (data.Length > 0)
                {
                    _medImmunities = JsonSerializer.Deserialize<List<MedicImmunePlayer>>(data)!;
                }
            }
        }
        
        await using (FileStream fs = new FileStream(GuildDataDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = await sr.ReadToEndAsync();
                if (data.Length > 0)
                {
                    _guildSettingsData = JsonSerializer.Deserialize<List<GuildSettingsData>>(data)!;
                }
            }
        }
        
        await using (FileStream fs = new FileStream(PlayerStatsDb, FileMode.OpenOrCreate, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string data = await sr.ReadToEndAsync();
                if (data.Length > 0)
                {
                    _playerStats = JsonSerializer.Deserialize<List<PlayerStats>>(data)!;
                }
            }
        }
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
                    _medImmunities = JsonSerializer.Deserialize<List<MedicImmunePlayer>>(data)!;
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
                    _guildSettingsData = JsonSerializer.Deserialize<List<GuildSettingsData>>(data)!;
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
                    _playerStats = JsonSerializer.Deserialize<List<PlayerStats>>(data)!;
                }
            }
        }
    }

    private static async Task SaveDbAsync ()
    {

        
        await using (FileStream fs = new FileStream(MedImmunityDb, FileMode.Truncate, FileAccess.Write))
        {
            await using (StreamWriter sw = new StreamWriter(fs))
            {
                var storedImmunities =_medImmunities.DistinctBy(mi => new { mi.Id, mi.GuildId }).ToList();
                await sw.WriteAsync(JsonSerializer.Serialize(storedImmunities));
            }
        }
        
        await using (FileStream fs = new FileStream(GuildDataDb, FileMode.Truncate, FileAccess.Write))
        {
            await using (StreamWriter sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(JsonSerializer.Serialize(_guildSettingsData));
            }
        }
        
        await using (FileStream fs = new FileStream(PlayerStatsDb, FileMode.Truncate, FileAccess.Write))
        {
            await using (StreamWriter sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(JsonSerializer.Serialize(_playerStats));
            }
        }
    }

    private static void SaveDb ()
    {

        using (FileStream fs = new FileStream(MedImmunityDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                var storedImmunities =_medImmunities.DistinctBy(mi => new { mi.Id, mi.GuildId }).ToList();
                sw.Write(JsonSerializer.Serialize(storedImmunities));
            }
        }
        
        using (FileStream fs = new FileStream(GuildDataDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(JsonSerializer.Serialize(_guildSettingsData));
            }
        }
        
        using (FileStream fs = new FileStream(PlayerStatsDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(JsonSerializer.Serialize(_playerStats));
            }
        }
    }


}