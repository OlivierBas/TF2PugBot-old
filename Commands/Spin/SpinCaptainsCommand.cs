using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinCaptainsCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (caller.IsConnectedToVoice())
        {
            var connectedUsers = caller.VoiceChannel.ConnectedUsers;
            int playersInVoice = connectedUsers.Count;
            if (connectedUsers.Count < 11)
            {
                await command.RespondAsync("Spin requires 12 players, ignoring.", ephemeral: true);
                return;
            }
            
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Spinning for Team Captain!");
            embedBuilder.WithColor(Color.Teal);

            if (await DataManager.PreviousGuildGameEndedAsync(command.GuildId.GetValueOrDefault(), true))
            {
                var newImmunities = DataManager.GetTemporaryMedImmunePlayers(command.GuildId);
                
                StringBuilder sb = new StringBuilder();
                foreach (MedicImmunePlayer player in newImmunities)
                {
                    sb.AppendLine($"{player.DisplayName} will be granted med immunity for next medic spin");
                }

                embedBuilder.WithFooter(sb.ToString());

                await DataManager.MakePermanentImmunitiesAsync(command.GuildId.GetValueOrDefault());
                
            }


            List<SocketGuildUser>? winners = await Spin(command, caller.VoiceChannel.ConnectedUsers, embedBuilder,
                                                        SpinMode.Duo, DataManager.InstantSpin);
            if (winners is not null)
            {

                if (DataManager.GuildHasPingsEnabled(command.GuildId.GetValueOrDefault()))
                {
                    await command.FollowupAsync($"<@!{winners[0].Id}> and <@!{winners[1].Id}> are team captains!");
                }
                else
                {
                    await command.FollowupAsync(
                        $"{winners[0].DisplayName} and {winners[1].DisplayName} are team captains!");
                }
                DataManager.StartGuildGame(command.GuildId.GetValueOrDefault());
                await DataManager.UpdatePlayerStatsAsync(winners[0].Id, command.GuildId.GetValueOrDefault(),
                                                         StatTypes.CaptainSpinsWon);
                await DataManager.UpdatePlayerStatsAsync(winners[1].Id, command.GuildId.GetValueOrDefault(),
                                                         StatTypes.CaptainSpinsWon);
                // await command.FollowupAsync($"winner");
            }

            return;
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }
}