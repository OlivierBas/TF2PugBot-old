using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class SpinMedicCommand : BaseSpinCommand, ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (caller.IsConnectedToVoice())
        {
            var connectedUsers = caller.VoiceChannel.ConnectedUsers;
            int playersInVoice = connectedUsers.Count;
            if (connectedUsers.Count < 6)
            {
                await command.RespondAsync("Spin requires 6 players, ignoring.", ephemeral: true);
                return;
            }

            Team? vcTeam = DataManager.GetGuildTeamChannel(command.GuildId.GetValueOrDefault(), caller.VoiceChannel.Id);

            if (vcTeam is not null)
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder.WithTitle($"Spinning for {vcTeam.ToString()} Medic!");
                embedBuilder.WithColor(Color.Red);
                embedBuilder.WithFooter(
                    await DataManager.GetMedImmunePlayerStringAsync(command.GuildId.GetValueOrDefault(),
                                                                    connectedUsers.ToList()));

                var immunePlayers     = await DataManager.GetMedImmunePlayersAsync(command.GuildId);
                var immunePlayersList = immunePlayers.ToList();
                List<SocketGuildUser> medSpinners
                    = connectedUsers.Where(cu => !immunePlayersList.Exists(ip => ip.Id == cu.Id)).ToList();

                if (medSpinners.Count == 0)
                {
                    await DataManager.ClearListOfImmunePlayersAsync(connectedUsers);
                    medSpinners = connectedUsers.ToList();
                }

                List<SocketGuildUser>? winners
                    = await Spin(command, medSpinners, embedBuilder, SpinMode.Solo, DataManager.InstantSpin);

                if (winners is not null)
                {
                    if (DataManager.GuildHasPingsEnabled(command.GuildId.GetValueOrDefault()))
                    {
                        await command.FollowupAsync(
                            $"<@!{winners[0].Id}> is {vcTeam.ToString()} medic and will be granted med immunity after game end, unless re-spun!");
                    }
                    else
                    {
                        await command.FollowupAsync(
                            $"{winners[0].DisplayName} is {vcTeam.ToString()} medic and will be granted med immunity after game end, unless re-spun!");
                    }
                    DataManager.PrepareMedImmunity(winners[0], vcTeam.GetValueOrDefault());
                    await DataManager.UpdatePlayerStatsAsync(winners[0].Id, command.GuildId.GetValueOrDefault(),
                                                             StatTypes.MedicSpinsWon);
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