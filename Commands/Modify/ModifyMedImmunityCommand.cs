using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;

namespace TF2PugBot.Commands.Modify;

public class ModifyMedImmunityCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        
        string option = command.Data.Options.First().Name;

        
        if (option == "get")
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Current Medic Immune Players");
            embedBuilder.WithDescription(DataManager.GetMedImmunePlayerString(command.GuildId));

            await command.RespondAsync(embed: embedBuilder.Build());
            return;
        }
        
        if (DataManager.HasAccessToCommand(command.GuildId, caller))
        {
            var    argUser       = command.Data.Options.First().Options.First().Value;

            try
            {
                SocketGuildUser user = (SocketGuildUser)argUser;
                if (option == "grant")
                {
                    await DataManager.ForceAddMedImmunePlayerAsync(user);
                    await command.RespondAsync($"<@!{user.Id}> has been granted medic immunity for 12 hours");
                    return;
                }

                if (option == "revoke")
                {
                    await DataManager.ForceRemoveMedImmunePlayerAsync(user);
                    await command.RespondAsync($"<@!{user.Id} has been revoked medic immunity");
                    return;
                }



                await command.RespondAsync("Invalid User");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync("Something went wrong, make sure to specify the user", ephemeral: true);
            }
        }
        await command.RespondAsync("You do not have access to this command", ephemeral: true);
        
    }
}