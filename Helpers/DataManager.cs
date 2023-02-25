using System.Text;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Helpers;

public static class DataManager
{
    private static string _token = string.Empty;
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
    

}