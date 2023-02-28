using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Helpers;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinCaptainsCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {

        if (caller.IsConnectedToVoice())
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Spinning for Team Captain!");
            embedBuilder.WithColor(Color.Teal);
            var newImmunities = DataManager.GetTemporaryMedImmunePlayers(command.GuildId);

            if (newImmunities is not null && newImmunities.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (MedicImmunePlayer player in newImmunities)
                {
                    if (player.Added.MinutesFromNow() > 4)
                    {
                        sb.AppendLine($"{player.DisplayName} will be granted med immunity for next medic spin");
                    }
                }

                embedBuilder.WithFooter(sb.ToString());

                await DataManager.MakePermanentImmunitiesAsync(command.GuildId.GetValueOrDefault());
            }
            
            List<SocketGuildUser>? winners = await Spin(command, caller.VoiceChannel.ConnectedUsers, embedBuilder, SpinMode.Duo, true);
            if(winners is not null)
            {
                 await command.FollowupAsync($"<@!{winners[0].Id}> and <@!{winners[1].Id}> are team captains!");
                // await command.FollowupAsync($"winner");
            }

            return;
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    
}