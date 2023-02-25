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
    
    private static List<MedicImmunePlayer> _medImmunities = new List<MedicImmunePlayer> ();
    private static MedicImmunePlayer[] _temporaryMedicImmunities = new MedicImmunePlayer[2];

    private static List<GuildTeamChannelData> _guildTeamChannels = new List<GuildTeamChannelData>();

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
        SaveDb();
    }

    public static IReadOnlyCollection<MedicImmunePlayer> GetMedImmunePlayers ()
    {
        List<MedicImmunePlayer> toBeRemoved =_medImmunities.Where(p => p.Added.HoursFromNow() > 12).ToList();
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

    public static string GetMedImmunePlayerString ()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var player in _medImmunities)
        {
            sb.Append($"{player.DisplayName} has med immunity for {12 - player.Added.HoursFromNow()} hours");
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
        if (_guildTeamChannels.Any(g => g.GuildId == guild.Id))
        {
            return;
        }
        _guildTeamChannels.Add(new GuildTeamChannelData() {GuildId = guild.Id});
    }
    
    public static Team? GetTeamChannelTeam (ulong guildId, ulong channelId)
    {
        var teamData =_guildTeamChannels.FirstOrDefault(g => g.GuildId == guildId);
        if (teamData is not null)
        {
            if (teamData.BluTeamVoiceChannelId == channelId)
            {
                return Team.BLU;
            }
            
            if (teamData.RedTeamVoiceChannelId == channelId)
            {
                return Team.RED;
            }
        }

        return null;
    }

    public static async Task<bool> UpdateGuildChannelData (ulong guildId, Team team, ulong channelId)
    {
        var teamData = _guildTeamChannels.FirstOrDefault(g => g.GuildId == guildId);
        if (teamData is not null)
        {
            bool success = teamData.TryUpdateValue(team, channelId);
            if (success)
            {
                SaveDb();
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
                    _guildTeamChannels = JsonSerializer.Deserialize<List<GuildTeamChannelData>>(data)!;
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
                    _guildTeamChannels = JsonSerializer.Deserialize<List<GuildTeamChannelData>>(data)!;
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
                await sw.WriteAsync(JsonSerializer.Serialize(_medImmunities));
            }
        }
        
        using (FileStream fs = new FileStream(guildDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(JsonSerializer.Serialize(_guildTeamChannels));
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
                sw.Write(JsonSerializer.Serialize(_medImmunities));
            }
        }
        
        using (FileStream fs = new FileStream(guildDb, FileMode.Truncate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(JsonSerializer.Serialize(_guildTeamChannels));
            }
        }
    }


}