﻿using System.Text;
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
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Spinning for Team Captain!");
            embedBuilder.WithColor(Color.Teal);

            if (DataManager.PreviousGuildGameEnded(command.GuildId.GetValueOrDefault()))
            {
                IReadOnlyCollection<MedicImmunePlayer> newImmunities = DataManager.GetTemporaryMedImmunePlayers(command.GuildId);

                if (newImmunities.Count > 0)
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

                foreach (var user in caller.VoiceChannel.ConnectedUsers)
                {
                    await DataManager.UpdatePlayerStatsAsync(user.Id, command.GuildId.GetValueOrDefault(),
                                                       StatTypes.GamesPlayed);
                }
            }
            
            
            List<SocketGuildUser>? winners = await Spin(command, caller.VoiceChannel.ConnectedUsers, embedBuilder, SpinMode.Duo, DataManager.InstantSpin);
            if(winners is not null)
            {
                DataManager.StartGuildGame(command.GuildId.GetValueOrDefault());
                await DataManager.UpdatePlayerStatsAsync(winners[0].Id, command.GuildId.GetValueOrDefault(), StatTypes.CaptainSpinsWon);
                await DataManager.UpdatePlayerStatsAsync(winners[1].Id, command.GuildId.GetValueOrDefault(), StatTypes.CaptainSpinsWon);
                if (DataManager.GuildHasPingsEnabled(command.GuildId.GetValueOrDefault()))
                {
                    await command.FollowupAsync($"<@!{winners[0].Id}> and <@!{winners[1].Id}> are team captains!");
                }
                else
                {
                    await command.FollowupAsync($"{winners[0].DisplayName} and {winners[1].DisplayName} are team captains!");
                }
                // await command.FollowupAsync($"winner");
            }

            return;
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    
}