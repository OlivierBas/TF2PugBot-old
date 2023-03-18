using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Extensions;

namespace TF2PugBot.Commands.Maps;

public class GetMapPoolCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        EmbedBuilder  embedBuilder  = new EmbedBuilder();
        StringBuilder stringBuilder = new StringBuilder();

        var maps        = await MapManager.GetGuildMapsAsync(command.GuildId.GetValueOrDefault());
        var ignoredMaps = await MapManager.GetIgnoredMapsAsync(command.GuildId.GetValueOrDefault());

        embedBuilder.WithTitle("Current Map Pool");
        for (int i = 0; i < maps.Count; i++)
        {
            stringBuilder.AppendLine($"{maps[i]}");
        }

        embedBuilder.WithDescription(stringBuilder.ToString());

        stringBuilder.Clear();
        if (ignoredMaps.Count > 0)
        {
            stringBuilder.AppendLine("**Already Played Maps**");
            for (int i = 0; i < ignoredMaps.Count; i++)
            {
                stringBuilder.AppendLine(
                    $"{ignoredMaps[i]}");
            }
        }

        embedBuilder.WithFooter(stringBuilder.ToString());

        await command.RespondAsync(embed: embedBuilder.Build());
    }
}