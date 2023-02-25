using System.Collections.ObjectModel;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Extensions;
using TF2PugBot.Helpers;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinMediCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task Perform (SocketSlashCommand command, SocketGuildUser caller)
    {

        if (caller.IsConnectedToVoice())
        {
            var connectedUsers = caller.VoiceChannel.ConnectedUsers;

            if (connectedUsers.Count > 6)
            {
                await command.RespondAsync("More than 6 players, ignoring.", ephemeral: true);
            }
            
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Spinning for Medic!");
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithFooter(DataManager.GetMedImmunePlayerString());

            List<SocketGuildUser> medSpinners = connectedUsers.Where(cu => DataManager.TrackedMedImmunities.Contains(cu)).ToList();
            if (medSpinners.Count == connectedUsers.Count)
            {
                DataManager.ClearListOfImmunePlayers(medSpinners);
            }
            
            List<SocketGuildUser>? winners = await Spin(command, medSpinners, embedBuilder, SpinMode.Solo);
            
            if (winners is null || winners.Count == 0)
            {
                await command.FollowupAsync("Something went wrong YELL AT BASS!");
            }
            else
            {
                DataManager.MakePlayerMedImmune(winners[0]);
                await command.FollowupAsync($"<@!{winners[0].Id}> is medic and will be granted med immunity after game end, unless re-spun!");

            }
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    
}