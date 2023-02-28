using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Management;

public class ConfigureTeamChannelCommand : ICommand
{


    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (DataManager.HasAccessToCommand(command.GuildId, caller))
        {
            string bluOrRed   = command.Data.Options.First().Options.First().Name;
            var    argChannel = command.Data.Options.First().Options.First().Options.First().Value;

            try
            {
                IVoiceChannel channel = (IVoiceChannel )argChannel;
                bool          success = false;
            
                if (bluOrRed == "blu")
                {
                    success = await DataManager.UpdateGuildChannelData(command.GuildId.GetValueOrDefault(), Team.BLU, channel.Id);

                }
                else if (bluOrRed == "red")
                {
                    success = await DataManager.UpdateGuildChannelData(command.GuildId.GetValueOrDefault(), Team.RED, channel.Id);
                
                }
            
                if (success)
                {
                    await command.RespondAsync($"Successfully set `{bluOrRed.ToUpper()}`'s team voice channel to `{channel.Name}`", ephemeral: true);
                    return;
                }
            
                await command.RespondAsync("Failed to update the specified team's voice channel", ephemeral: true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync("Something went wrong, make sure to specify a voice channel.", ephemeral: true);
            }
        }

        await command.RespondAsync("You do not have access to this command", ephemeral: true);


    }
}