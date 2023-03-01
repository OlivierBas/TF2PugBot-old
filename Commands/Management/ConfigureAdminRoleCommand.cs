using Discord.WebSocket;
using TF2PugBot.Data;


namespace TF2PugBot.Commands.Management;

public class ConfigureAdminRoleCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (DataManager.HasAccessToCommand(command.GuildId, caller))
        {
            var argRole = command.Data.Options.First().Value;

            try
            {
                SocketRole role    = (SocketRole )argRole;
                await DataManager.SetGuildAdminRoleAsync(command.GuildId.GetValueOrDefault(), role);
                
                await command.RespondAsync($"Successfully set `{role.Name}` to the bot management role.", ephemeral: true);
                return;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync("Something went wrong, make sure to specify the role.", ephemeral: true);
            }
        }

        await command.RespondAsync("You do not have access to this command", ephemeral: true);
        
    }
}