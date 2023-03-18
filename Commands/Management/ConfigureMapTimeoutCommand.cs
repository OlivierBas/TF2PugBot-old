using Discord.WebSocket;
using TF2PugBot.Data;

namespace TF2PugBot.Commands.Management;

public class ConfigureMapTimeoutCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (GuildManager.HasAccessToCommand(command.GuildId, caller))
        {
            var argValue = command.Data.Options.First().Value;

            try
            {
                int hours = (int )argValue;
                await MapManager.UpdateMapTimeout(command.GuildId.GetValueOrDefault(), hours);
                
                await command.RespondAsync($"Successfully set map timeout to `{hours}` hours", ephemeral: true);
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