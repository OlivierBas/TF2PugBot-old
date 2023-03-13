using Discord.WebSocket;
using TF2PugBot.Data;

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

                if (addOrRemove == "add")
                {
                    await MapManager.AddMapToGuildMapsAsync(command.GuildId.GetValueOrDefault(), mapName);
                }

                if (addOrRemove == "remove")
                {
                    await MapManager.RemoveMapFromGuildMapsAsync(command.GuildId.GetValueOrDefault(), mapName);

                }
                
                string action = addOrRemove == "add" ? "added" : "removed";
                await command.RespondAsync($"Successfully `{action}` {mapName} to the map pool", ephemeral: true);
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