using System.Text;
using System.Text.Json;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static partial class DataManager
{
    private static List<MedicImmunePlayer> _medImmunities = new List<MedicImmunePlayer>();

    private static Dictionary<ulong, MedicImmunePlayer[]> _temporaryMedicImmunities
        = new Dictionary<ulong, MedicImmunePlayer[]>();

    public static IReadOnlyCollection<MedicImmunePlayer> TrackedMedImmunities => _medImmunities.AsReadOnly();

    public static async Task<List<MedicImmunePlayer>> GetMedImmunePlayersAsync (ulong? guildId)
    {
        RemoveOldImmunities(guildId.GetValueOrDefault());


        await SaveDbAsync();
        return _medImmunities;
    }

    public static MedicImmunePlayer[]? GetTemporaryMedImmunePlayers (ulong? guildId)
    {
        _temporaryMedicImmunities.TryGetValue(guildId.GetValueOrDefault(), out var result);
        return result;
    }

    public static void PrepareTempMedImmunity (SocketGuildUser player, Team team)
    {
        var medicImmunePlayer = new MedicImmunePlayer()
        {
            DisplayName = player.DisplayName,
            Id          = player.Id,
            GuildId     = player.Guild.Id,
            Added       = DateTime.Now
        };

        if (!_temporaryMedicImmunities.ContainsKey(player.Guild.Id))
        {
            _temporaryMedicImmunities.Add(player.Guild.Id, new MedicImmunePlayer[2]);
        }


        switch (team)
        {
            case Team.RED:
                _temporaryMedicImmunities[player.Guild.Id][(int)team] = medicImmunePlayer;
                break;
            case Team.BLU:
                _temporaryMedicImmunities[player.Guild.Id][(int)team] = medicImmunePlayer;
                break;
        }

    }

    public static async Task MakePermanentImmunitiesAsync (ulong guildId)
    {
        if (GuildGameHasEnded(guildId))
        {
            if (_temporaryMedicImmunities.ContainsKey(guildId))
            {
                foreach (var tempImmunePlayer in _temporaryMedicImmunities[guildId])
                {
                    if (tempImmunePlayer is not null)
                    {
                        if (_medImmunities.Contains(tempImmunePlayer))
                        {
                            _medImmunities.FirstOrDefault(p => p.Id == tempImmunePlayer.Id
                                                            && p.GuildId == tempImmunePlayer.GuildId)!.Added = DateTime.Now;
                            continue;
                        }
                        _medImmunities.Add(tempImmunePlayer);
                    }

                }

                _temporaryMedicImmunities[guildId] = new MedicImmunePlayer[2];
            }

            await SaveDbAsync();
        }

    }

    public static async Task<string> GetMedImmunePlayerStringAsync (ulong? guildId)
    {
        StringBuilder sb = new StringBuilder();
        RemoveOldImmunities(guildId.GetValueOrDefault());

        await SaveDbAsync();
        foreach (var player in _medImmunities.Where(p => p.GuildId == guildId))
        {
            sb.Append($"{player.DisplayName} has med immunity for {Constants.MedImmunityClearHours - player.Added.HoursFromNow()} hours\n");
        }

        return sb.ToString();
    }

    public static async Task<string> GetMedImmunePlayerStringAsync (ulong? guildId, List<SocketGuildUser> voiceUsers)
    {
        StringBuilder sb = new StringBuilder();
        RemoveOldImmunities(guildId.GetValueOrDefault());

        await SaveDbAsync();
        foreach (var player in _medImmunities.Where(p => p.GuildId == guildId))
        {
            if (voiceUsers.Exists(vu => vu.Id == player.Id))
            {
                sb.Append($"{player.DisplayName} has med immunity for {Constants.MedImmunityClearHours - player.Added.HoursFromNow()} hours\n");
            }
        }

        return sb.ToString();
    }

    private static void RemoveOldImmunities (ulong guildId)
    {
        List<MedicImmunePlayer> toBeRemoved
            = _medImmunities.Where(p => Constants.MedImmunityClearHours - p.Added.HoursFromNow() <= 0 && p.GuildId == guildId).ToList();
        if (toBeRemoved.Count > 0)
        {
            _medImmunities.RemoveAll(p => toBeRemoved.Contains(p));
        }
    }

    public static async Task ClearListOfImmunePlayersAsync (IEnumerable<SocketGuildUser> playersToBeRemoved)
    {
        _medImmunities.RemoveAll(m => playersToBeRemoved.Select(p => (MedicImmunePlayer)p).Contains(m));
        await SaveDbAsync();
    }

    public static async Task ForceAddMedImmunePlayerAsync (SocketGuildUser player)
    {
        _medImmunities.Add(player);
        await SaveDbAsync();
    }

    public static async Task ForceRemoveMedImmunePlayerAsync (SocketGuildUser player)
    {
        _medImmunities.RemoveAll(m => m.GuildId == player.Guild.Id && m.Id == player.Id);
        await SaveDbAsync();
    }
}