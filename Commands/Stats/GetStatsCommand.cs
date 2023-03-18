using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Stats;

public class GetStatsCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        try
        {
            var           argsUser      = command.Data.Options;
            EmbedBuilder  embedBuilder  = new EmbedBuilder();
            StringBuilder stringBuilder = new StringBuilder();
            if (argsUser is null
             || argsUser.Count == 0)
            {
                var psg = StatsManager.GetPlayerGuildStats(caller.Id, command.GuildId.GetValueOrDefault());
                BuildTexts(embedBuilder, stringBuilder, psg, caller);

                await command.RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                try
                {
                    SocketGuildUser user = (SocketGuildUser)argsUser.First().Value;
                    var             psg = StatsManager.GetPlayerGuildStats(user.Id, command.GuildId.GetValueOrDefault());
                    BuildTexts(embedBuilder, stringBuilder, psg, user);

                    await command.RespondAsync(embed: embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await command.RespondAsync("Something went wrong. The user specified is invalid", ephemeral: true);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
    }

    private void BuildTexts (EmbedBuilder embedBuilder, StringBuilder stringBuilder, PlayerGuildStats psg,
                             SocketGuildUser user)
    {
        embedBuilder.WithTitle($"{user.DisplayName}'s stats");
        embedBuilder.WithThumbnailUrl(user.GetDisplayAvatarUrl());
        stringBuilder.AppendLine($"**Games played**: {psg.GamesPlayed}");
        stringBuilder.AppendLine($"**Captain Spins Won**: {psg.WonCaptainSpins}");
        stringBuilder.AppendLine($"**Medic Spins Won**: {psg.WonMedicSpins}");
        embedBuilder.WithFooter($"Last Played: {psg.LastPlayed.ToString("yyyy-MM-dd")}");
        embedBuilder.WithDescription(stringBuilder.ToString());
    }
}