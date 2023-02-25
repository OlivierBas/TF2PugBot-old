using System.Text;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Helpers;

public static class DataManager
{
    private static string _token = string.Empty;
    
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

    private static List<MedicImmunePlayer> _medImmunities = new List<MedicImmunePlayer> ();
    public static IReadOnlyCollection<MedicImmunePlayer> TrackedMedImmunities
    {
        get => _medImmunities.AsReadOnly();
    }

    public static void MakePlayerMedImmune (SocketGuildUser player)
    {
        List<MedicImmunePlayer> toBeRemoved =_medImmunities.Where(p => p.Added.HoursFromNow() > 12).ToList();
        if (toBeRemoved.Count > 0)
        {
            _medImmunities.RemoveAll(p => toBeRemoved.Contains(p));

        }
        
        _medImmunities.Add(new MedicImmunePlayer()
        {
            DisplayName = player.DisplayName,
            Id = player.Id,
            Added = DateTime.Now
        });
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

    public static void ClearListOfImmunePlayers (List<MedicImmunePlayer> playersToBeRemoved)
    {
        _medImmunities.RemoveAll(m => playersToBeRemoved.Contains(m));
    }
    
    public static void ClearListOfImmunePlayers (List<SocketGuildUser> playersToBeRemoved)
    {
        _medImmunities.RemoveAll(m => playersToBeRemoved.Select(p => (MedicImmunePlayer)p).Contains(m));
    }

    public static Team GetTeamChannelTeam (ulong guildId, ulong channelId)
    {
        var teamData =_guildTeamChannels.FirstOrDefault(g => g.GuildId == guildId);
        if (teamData is not null)
        {
            if (teamData.BluTeamVoiceChannelId == channelId)
            {
                return Team.BLU;
            }
            else if (teamData.RedTeamVoiceChannelId == channelId)
            {
                return Team.RED;
            }
        }

        return Team.RED;
    }

    public static ulong? GetTeamChannelId (ulong guildId, Team team)
    {
        var teamData = _guildTeamChannels.FirstOrDefault(g => g.GuildId == guildId);

        if (teamData is not null)
        {
            switch (team)
            {
                case Team.RED:
                    return teamData.RedTeamVoiceChannelId;
                case Team.BLU:
                    return teamData.BluTeamVoiceChannelId;
            }
        }

        return null;
    }
    
    public static bool UpdateGuildTeamChannelData (ulong guildId, Team team, ulong channelId)
    {
        var teamData = _guildTeamChannels.FirstOrDefault(g => g.GuildId == guildId);
        if (teamData is not null)
        {
            return teamData.TryUpdateValue(team, channelId);
        }

        return false;

    }

    public static void InitializeGuildData (SocketGuild guild)
    {
        if (_guildTeamChannels.Any(g => g.GuildId == guild.Id))
        {
            return;
        }
        _guildTeamChannels.Add(new GuildTeamChannelData() {GuildId = guild.Id});
    }

}