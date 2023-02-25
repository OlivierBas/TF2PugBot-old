using Discord;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinCaptainsCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task Perform (SocketSlashCommand command, SocketGuildUser caller)
    {

        if (caller.IsConnectedToVoice())
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Spinning for Team Captain!");
            embedBuilder.WithColor(Color.Teal);
            List<SocketGuildUser>? winners = await Spin(command, caller.VoiceChannel.ConnectedUsers, embedBuilder, SpinMode.Duo);
            if (winners is null || winners.Count < 2)
            {
                await command.FollowupAsync("Something went wrong YELL AT BASS!");
            }
            else
            {
                await command.FollowupAsync($"<@!{winners[0].Id}> and <@!{winners[1].Id}> are captains!");

            }
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    
}