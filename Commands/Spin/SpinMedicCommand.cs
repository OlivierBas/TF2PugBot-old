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
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        
        if (caller.IsConnectedToVoice())
        {
            var connectedUsers = caller.VoiceChannel.ConnectedUsers;
            int playersInVoice = connectedUsers.Count;
            if (connectedUsers.Count > 6)
            {
                await command.RespondAsync("More than 6 players, ignoring.", ephemeral: true);
            }

            Team? vcTeam = DataManager.GetGuildTeamChannel(command.GuildId.GetValueOrDefault(), caller.VoiceChannel.Id);

            if (vcTeam is not null)
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder.WithTitle($"Spinning for {vcTeam.ToString()} Medic!");
                embedBuilder.WithColor(Color.Red);
                embedBuilder.WithFooter(DataManager.GetMedImmunePlayerString(command.GuildId.GetValueOrDefault()));
            
                List<SocketGuildUser> medSpinners = connectedUsers.Where(cu => !DataManager.GetMedImmunePlayers(command.GuildId).Contains(cu)).ToList();
                if (connectedUsers.Count - medSpinners.Count == playersInVoice)
                {
                    await DataManager.ClearListOfImmunePlayersAsync(medSpinners);
                    medSpinners = connectedUsers.ToList();
                }
            
                List<SocketGuildUser>? winners = await Spin(command, medSpinners, embedBuilder, SpinMode.Solo);

                if (winners is not null)
                {
                    DataManager.MakePlayerMedImmune(winners[0], vcTeam.GetValueOrDefault());
                    await command.FollowupAsync($"<@!{winners[0].Id}> is {vcTeam.ToString()} medic and will be granted med immunity after game end, unless re-spun!");  
                    //await command.FollowupAsync($"winner!");  
                }

                return;
            }
            else
            {
                await command.RespondAsync(
                    $"{caller.VoiceChannel.Name} is not set as a team channel and cannot be used for medic spins.",
                    ephemeral: true);
                return;
                
            }
            

        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    
}