using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Stats;

public class GetLeaderboardCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        try
        {
            var argsMode = command.Data.Options.First().Name;
            if (argsMode is not null)
            {
                ulong        guildId    = command.GuildId.GetValueOrDefault();
                var          playerStats =  StatsManager.GetPlayerStatsOfGuild(guildId);
                StatTypes?   chosenStat = null;
                EmbedBuilder embedBuilder = new EmbedBuilder();

                switch (argsMode)
                {
                    case "captains":
                        playerStats = playerStats.OrderByDescending(gs => gs.GetGuildStat(guildId)?.WonCaptainSpins).ToList();
                        chosenStat = StatTypes.CaptainSpinsWon;
                        break;
                    case "medics":
                        playerStats = playerStats.OrderByDescending(gs => gs.GetGuildStat(guildId)?.WonMedicSpins).ToList();
                        chosenStat = StatTypes.MedicSpinsWon;
                        break;
                    case "played":
                        playerStats = playerStats.OrderByDescending(gs => gs.GetGuildStat(guildId)?.GamesPlayed).ToList();
                        chosenStat = StatTypes.GamesPlayed;
                        break;
                }

                if (chosenStat is not null)
                {
                    embedBuilder.WithTitle($"Leaderboard for {argsMode}");
                    embedBuilder.WithDescription("");
                    await WriteTopPlayers(embedBuilder, playerStats, chosenStat.GetValueOrDefault(), guildId, 10);
                    await command.RespondAsync(embed: embedBuilder.Build());
                }
                else
                {
                    await command.RespondAsync("Something went wrong :)", ephemeral: true);
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await command.RespondAsync("Something went HORRIBLY wrong :)", ephemeral: true);
        }
    }

    async Task WriteTopPlayers (EmbedBuilder embedBuilder, List<PlayerStats> guildStats, StatTypes chosenStat, ulong guildId, int maxPlayers)
    {
        for (int i = 0; i < guildStats.Take(maxPlayers).Count(); i++)
        {
            switch (chosenStat)
            {
                case StatTypes.GamesPlayed:
                    embedBuilder.AddField($"**{i + 1}. {await Program.GetNameByUserIdAsync(guildStats[i].UserId)}**", guildStats[i].GetGuildStat(guildId)?.GamesPlayed + " games played");
                    break;
                case StatTypes.CaptainSpinsWon:
                    embedBuilder.AddField($"**{i + 1}. {await Program.GetNameByUserIdAsync(guildStats[i].UserId)}**", guildStats[i].GetGuildStat(guildId)?.WonCaptainSpins + " captain spins won");
                    break;
                case StatTypes.MedicSpinsWon:
                    embedBuilder.AddField($"**{i + 1}. {await Program.GetNameByUserIdAsync(guildStats[i].UserId)}**", guildStats[i].GetGuildStat(guildId)?.WonMedicSpins + " medic spins won");
                    break;
            }
        }

    }
}