using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinMapCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        ulong? gid = command.GuildId;
        if (gid is not null)
        {
            ulong guildId = gid.GetValueOrDefault();
            if (!GuildManager.GuildGameHasEnded(guildId))
            {
                await MapManager.ClearIgnoredMapsAsync(guildId);
                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder.WithTitle("Spinning for map");
                embedBuilder.WithColor(Color.Red);
                var ignoredGuildMaps = await MapManager.GetIgnoredMapsAsync(guildId);
                var guildMaps        = await MapManager.GetGuildMapsAsync(guildId);

                embedBuilder.WithFooter(await MapManager.GetIgnoredMapsAsStringAsync(guildId));

                var rollingMaps = guildMaps.Where(g => !ignoredGuildMaps.Contains(g)).ToList();
                if (rollingMaps.Count == 0)
                {
                    embedBuilder.WithFooter("Can't ignore any maps, re-rolling with all allowed.");
                    rollingMaps = guildMaps;
                }

                var wonMap = await Spin<SixesMap>(command, rollingMaps, embedBuilder, SpinMode.Solo,
                                                  EasySetup.InstantSpins);
                
                MapManager.PrepareMapIgnore(guildId, wonMap[0]);
                await command.RespondAsync($"{wonMap[0].MapName} won the map spin.");
                return;
            }

            await command.RespondAsync(
                "A game has to be running for a map spin (Roll captains first, or medic if smix)", ephemeral: true);
            return;
        }

        await command.RespondAsync("Something went HORRIBLY wrong! :)", ephemeral: true);
    }
}