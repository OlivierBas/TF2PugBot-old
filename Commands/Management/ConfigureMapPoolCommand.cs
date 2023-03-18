using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Extensions;

namespace TF2PugBot.Commands.Management;

public class ConfigureMapPoolCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (GuildManager.HasAccessToCommand(command.GuildId, caller))
        {
            string addOrRemove = command.Data.Options.First().Name;
            var argMap     = command.Data.Options.First().Options.First().Value; 

            try
            {
                string mapName = (string)argMap;
                bool   success = false;
                mapName = mapName.FirstLetterUppercase();
                if (addOrRemove == "add")
                {
                    success = await MapManager.TryAddMapToGuildMapsAsync(command.GuildId.GetValueOrDefault(), mapName);
                }

                if (addOrRemove == "remove")
                {
                    success = await MapManager.TryRemoveMapFromGuildMapsAsync(command.GuildId.GetValueOrDefault(), mapName);

                }

                if (success)
                {
                    string action = addOrRemove == "add" ? "added" : "removed";
                    await command.RespondAsync($"Successfully {action} `{mapName}` to the map pool");
                }
                else
                {
                    await command.RespondAsync("Something went wrong. minimum map name length is 3 and a total of 20 maps are allowed.", ephemeral: true);
                }

                return;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync("Something went wrong.", ephemeral: true);
            }
        }

        await command.RespondAsync("You do not have access to this command", ephemeral: true);

    }
}