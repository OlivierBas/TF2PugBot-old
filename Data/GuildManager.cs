using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static class GuildManager
{
    private static List<GuildSettingsData> _guildSettingsData = new List<GuildSettingsData>();
    private static Dictionary<ulong, GuildGameData> _trackedGuildGame = new Dictionary<ulong, GuildGameData>();

    public static List<GuildSettingsData> GuildSettings
    {
        get => _guildSettingsData;
        set => _guildSettingsData = value;
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

            if (caller.Id == DataManager.DevId)
            {
                return true;
            }
        }

        return false;
    }

    public static async Task InitializeGuildDataAsync (SocketGuild guild)
    {
        if (_guildSettingsData.Any(g => g.GuildId == guild.Id))
        {
            return;
        }

        _trackedGuildGame.Add(guild.Id, new GuildGameData());
        
        _guildSettingsData.Add(new GuildSettingsData() { GuildId = guild.Id });
        await DataManager.SaveDbAsync(SaveType.GuildData);
    }

    public static void StartNewGuildGame (ulong guildId)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            _trackedGuildGame.Add(guildId, new GuildGameData());
        }
        else
        {
            _trackedGuildGame[guildId] = new GuildGameData();
        }

        Console.WriteLine($"Guild game started for guild {guildId}");
    }

    public static void StartNewGuildGame (ulong guildId, List<SocketGuildUser> players)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            _trackedGuildGame.Add(
                guildId, new GuildGameData() { Players = players.Select(p => p.Id).ToList(), LockPlayers = true });
        }
        else
        {
            _trackedGuildGame[guildId] = new GuildGameData()
            {
                Players     = players.Select(p => p.Id).ToList(),
                LockPlayers = true
            };
        }

        Console.WriteLine($"Guild game started for guild {guildId} with {players.Count} players");
    }

    public static bool GuildGameHasEnded (ulong guildId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            if (_trackedGuildGame[guildId].StartDate.MinutesFromNow() >= Constants.GuildGameMinDuration)
            {
                return true;
            }
        }

        return false;
    }

    public static async Task EnsureGuildGameEnded (ulong guildId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            var guildGame = _trackedGuildGame[guildId];
            if (guildGame.StartDate.HoursFromNow() >= 2) // If 2 hours have passed, just end the game.
            {
                Console.WriteLine($"Two hours passed, forcefully ending guild game for {guildId}.");
                await TryEndGuildGame(guildId);
            }
        }
    }

    public static async Task<bool> TryEndGuildGame (ulong guildId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            var guildGame = _trackedGuildGame[guildId];
            if (guildGame.StartDate.MinutesFromNow() >= Constants.GuildGameMinDuration)
            {
                if (guildGame.Players.Count == 0)
                {
                    Console.WriteLine($"Something went wrong, guild game {guildId} had 0 players");
                }

                foreach (var player in guildGame.Players)
                {
                    await StatsManager.UpdatePlayerStatsAsync(guildId, StatTypes.GamesPlayed, player);
                    Console.WriteLine($"user {player}, Increased Games Played");
                }

                _trackedGuildGame[guildId] = new GuildGameData(); // reset the tracked guild game.
                return true;
            }
        }

        return false;
    }


    /*public static void AddPlayersToGuildGame (ulong guildId, params ulong[] userIds)
    {
        if (_lastGuildGame.ContainsKey(guildId))
        {
            foreach (var userId in userIds)
            {
                if (_lastGuildGame[guildId].Players.Contains(userId))
                {
                    continue;
                }
                _lastGuildGame[guildId].Players.Add(userId);
            }
        }
    }*/

    public static void AddPlayerToGuildGame (ulong guildId, ulong userId)
    {
        if (_trackedGuildGame.ContainsKey(guildId))
        {
            Console.WriteLine($"Guild game has: {_trackedGuildGame[guildId].Players.Count} players.");
            if (!_trackedGuildGame[guildId].LockPlayers)
            {
                Console.WriteLine($"And now adding {userId} to count.");
                _trackedGuildGame[guildId].Players.Add(userId);
            }
        }
        else
        {
            // Otherwise create a new trackedGuildGame and add the first player.
            _trackedGuildGame.Add(guildId, new GuildGameData() { Players = new List<ulong>() { userId } });
        }
    }

    public static void RemovePlayerFromGuildGame (ulong guildId, ulong userId)
    {
        if (!_trackedGuildGame.ContainsKey(guildId))
        {
            Console.WriteLine($"Guild game has: {_trackedGuildGame[guildId].Players.Count} players.");
            if (!_trackedGuildGame[guildId].LockPlayers)
            {
                Console.WriteLine($"And now removing {userId} from count.");
                _trackedGuildGame[guildId].Players.Remove(userId);
            }
        }
    }

    public static Team? GetGuildTeamChannel (ulong guildId, ulong channelId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
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

    public static bool TryGetGuildTeamChannel (ulong guildId, ulong channelId, out Team? team)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            if (guildData.BluTeamVoiceChannelId == channelId)
            {
                team = Team.BLU;
                return true;
            }

            if (guildData.RedTeamVoiceChannelId == channelId)
            {
                team = Team.BLU;
                return true;
            }
        }

        team = null;
        return false;
    }

    public static bool GuildHasPingsEnabled (ulong guildId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            return guildData.PingOnSpin;
        }

        return false;
    }

    public static bool GuildIsHLMode (ulong guildId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            return guildData.HLMode;
        }

        return false;
    }

    public static async Task SetGuildAdminRoleAsync (ulong? guildId, SocketRole role)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.AdminRoleId = role.Id;
            await DataManager.SaveDbAsync(SaveType.GuildData);
            
        }
    }

    public static async Task SetGuildPingsAsync (ulong? guildId, bool value)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.PingOnSpin = value;
            await DataManager.SaveDbAsync(SaveType.GuildData);
        }
    }
    
    public static async Task SetGuildHLModeAsync (ulong? guildId, bool value)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            guildData.HLMode = value;
            await DataManager.SaveDbAsync(SaveType.GuildData);
        }
    }

    public static async Task<bool> UpdateGuildChannelDataAsync (ulong guildId, Team team, ulong channelId)
    {
        var guildData = _guildSettingsData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildData is not null)
        {
            bool success = guildData.TryUpdateValue(team, channelId);
            if (success)
            {
                await DataManager.SaveDbAsync(SaveType.GuildData);
                return success;
            }
        }

        return false;
    }
}