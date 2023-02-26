using System.Text;
using System.Text.Json;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Helpers;

public static class DataManager
{
    private static string _token = string.Empty;
    private const string MEDIMMUNITIES_DB = "medimmunities.json";
    private const string GUILDTEAMCHANNELS_DB = "guildteamchannels.json";
    private const ulong DEV_ID = 624280077982629888;
    
    private static List<MedicImmunePlayer> _medImmunities = new List<MedicImmunePlayer> ();
    private static MedicImmunePlayer[] _temporaryMedicImmunities = new MedicImmunePlayer[2];

    private static List<GuildSettingsData> _guildSettingsData = new List<GuildSettingsData>();

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
    
    public static IReadOnlyCollection<MedicImmunePlayer> TrackedMedImmunities => _medImmunities.AsReadOnly();
    
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

            if (caller.Roles.Any(r => r.Permissions.Administrator == true))
            {
                return true;
            }

            if (caller.Guild.OwnerId == caller.Id)
            {
                return true;
            }

            if (caller.Id == DEV_ID)
            {
                return true;
            }
        }

        return false;
    }

    public static async Task SetAdminRole (ulong? guildId, SocketRole role)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.AdminRoleId = role.Id;
            await SaveDbAsync();
        }
    }

    public static IReadOnlyCollection<MedicImmunePlayer> GetMedImmunePlayers (ulong? guildId)
    {
        List<MedicImmunePlayer> toBeRemoved =_medImmunities.Where(p => p.Added.HoursFromNow() < 0 && p.GuildId == guildId).ToList();
        if (toBeRemoved.Count > 0)
        {
            _medImmunities.RemoveAll(p => toBeRemoved.Contains(p));

        }
        SaveDb();
        return _medImmunities;
    }

    public static void MakePlayerMedImmune (SocketGuildUser player, Team team)
    {
        var medicImmunePlayer = new MedicImmunePlayer()
        {
            DisplayName = player.DisplayName,
            Id          = player.Id,
            GuildId     = player.Guild.Id,
            Added       = DateTime.Now
        };
        
        switch (team)
        {
            case Team.RED:
                _temporaryMedicImmunities[(int)team] = medicImmunePlayer;
                break;
            case Team.BLU:
                _temporaryMedicImmunities[(int)team] = medicImmunePlayer;
                break;
        }
        
    }

    public static async Task MakePermanentImmunitiesAsync ()
    {
        foreach (var medicImmunePlayer in _medImmunities)
        {
            if (_medImmunities.Contains(medicImmunePlayer))
            {
                _medImmunities.FirstOrDefault(p => p.Id == medicImmunePlayer.Id && p.GuildId == medicImmunePlayer.GuildId)!.Added = DateTime.Now;
                continue;
            }
            _medImmunities.Add(medicImmunePlayer);
        }

        await SaveDbAsync();
    }

    public static string GetMedImmunePlayerString (ulong? guildId)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var player in _medImmunities.Where(p => p.GuildId == guildId))
        {
            sb.Append($"{player.DisplayName} has med immunity for {12 - player.Added.HoursFromNow()} hours\n");
        }
        return sb.ToString();
    }

    public static async Task ClearListOfImmunePlayersAsync (List<SocketGuildUser> playersToBeRemoved)
    {
        _medImmunities.RemoveAll(m => playersToBeRemoved.Select(p => (MedicImmunePlayer)p).Contains(m));
        await SaveDbAsync();
    }

    public static void InitializeGuildData (SocketGuild guild)
    {
        if (_guildSettingsData.Any(g => g.GuildId == guild.Id))
        {
            return;
        }
        _guildSettingsData.Add(new GuildSettingsData() {GuildId = guild.Id});
    }
    
    public static Team? GetGuildTeamChannel (ulong guildId, ulong channelId)
    {
        var guildData =_guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            if (guildData.BluTeamVoiceChannelId == channelId)
            {
                return Team.BLU;
            }
            
            if (guildData.RedTeamVoiceChannelId == channelId)
            {
                return Team.RED;
            }
        }

        return null;
    }

    public static async Task<bool> UpdateGuildChannelData (ulong guildId, Team team, ulong channelId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            bool success = guildData.TryUpdateValue(team, channelId);
            if (success)
            {
                await SaveDbAsync();
                return success;
            }
        }

        return false;

    }

    private static async Task RetrieveDbAsync ()
    {
        string medDb   = MEDIMMUNITIES_DB;
        string guildDb = GUILDTEAMCHANNELS_DB;
        
        using (FileStream fs = new FileStream(medDb, FileMode.OpenOrCreate, FileAccess.Read))
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
        
        using (FileStream fs = new FileStream(guildDb, FileMode.OpenOrCreate, FileAccess.Read))
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
    }

    private static void RetrieveDb ()
    {
        string medDb   = MEDIMMUNITIES_DB;
        string guildDb = GUILDTEAMCHANNELS_DB;
        
        using (FileStream fs = new FileStream(medDb, FileMode.OpenOrCreate, FileAccess.Read))
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
        
        using (FileStream fs = new FileStream(guildDb, FileMode.OpenOrCreate, FileAccess.Read))
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
    }

    private static async Task SaveDbAsync ()
    {
        string medDb   = MEDIMMUNITIES_DB;
        string guildDb = GUILDTEAMCHANNELS_DB;
        using (FileStream fs = new FileStream(medDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                var storedImmunities =_medImmunities.DistinctBy(mi => new { mi.Id, mi.GuildId }).ToList();
                await sw.WriteAsync(JsonSerializer.Serialize(storedImmunities));
            }
        }
        
        using (FileStream fs = new FileStream(guildDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(JsonSerializer.Serialize(_guildSettingsData));
            }
        }
    }

    private static void SaveDb ()
    {
        string medDb   = MEDIMMUNITIES_DB;
        string guildDb = GUILDTEAMCHANNELS_DB;
        using (FileStream fs = new FileStream(medDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                var storedImmunities =_medImmunities.DistinctBy(mi => new { mi.Id, mi.GuildId }).ToList();
                sw.Write(JsonSerializer.Serialize(storedImmunities));
            }
        }
        
        using (FileStream fs = new FileStream(guildDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(JsonSerializer.Serialize(_guildSettingsData));
            }
        }
    }


}