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
            if (playersInVoice < 6)
            {
                await command.RespondAsync("Spin requires 6 players, ignoring.", ephemeral: true);
                return;
            }

            ulong guildId = command.GuildId.GetValueOrDefault();
            Team? vcTeam = GuildManager.GetGuildTeamChannel(command.GuildId.GetValueOrDefault(), caller.VoiceChannel.Id);

            if (vcTeam is null)
            {
                await command.RespondAsync(
                    $"{caller.VoiceChannel.Name} is not set as a team channel and cannot be used for medic spins.",
                    ephemeral: true);
                return;
            }


            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle($"Spinning for {vcTeam.ToString()} Medic!");
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithFooter(
                await MedManager.GetMedImmunePlayerStringAsync(guildId,
                                                                connectedUsers.ToList()));

            List<MedicImmunePlayer> activeMedImmunities = await MedManager.GetMedImmunePlayersAsync(guildId);
            
            List<SocketGuildUser> currentMedSpinners
                = connectedUsers.Where(cu => !activeMedImmunities.Exists(ip => ip.Id == cu.Id)).ToList();
            if (currentMedSpinners.Count == 0)
            {
                embedBuilder.WithFooter("Too many immune players, ignoring immunities.");
                currentMedSpinners = connectedUsers.ToList();
            }

            List<SocketGuildUser>? winners
                = await SpinUsers(command, currentMedSpinners, embedBuilder, SpinMode.Solo, DataManager.InstantSpin);

            if (winners is not null)
            {
                if (GuildManager.GuildGameHasEnded(guildId))
                {
                    Console.WriteLine("smix attempted (we believe atleast), close the previous game and start smix game");
                    await GuildManager.TryEndGuildGame(guildId);
                    GuildManager.StartNewGuildGame(guildId, connectedUsers.ToList());
                }
                
                
                MedManager.PrepareTempMedImmunity(winners[0], vcTeam.GetValueOrDefault());
                await StatsManager.UpdatePlayerStatsAsync(guildId,
                                                          StatTypes.MedicSpinsWon,
                                                          winners.Select(w => w.Id).ToArray());

                if (GuildManager.GuildHasPingsEnabled(guildId))
                {
                    await command.FollowupAsync(
                        $"<@!{winners[0].Id}> is {vcTeam.ToString()} medic and will be granted med immunity after game end, unless re-spun!");
                }
                else
                {
                    await command.FollowupAsync(
                        $"{winners[0].DisplayName} is {vcTeam.ToString()} medic and will be granted med immunity after game end, unless re-spun!");
                }


                return;
            }
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }
}