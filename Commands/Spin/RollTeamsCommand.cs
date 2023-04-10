using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Data;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public class RollTeamsCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (caller.IsConnectedToVoice())
        {
            var connectedUsers = caller.VoiceChannel.ConnectedUsers;
            int playersInVoice = connectedUsers.Count;
            if (playersInVoice < 12)
            {
                await command.RespondAsync("Roll requires atleast 12 players, ignoring.", ephemeral: true);
                return;
            }

            ulong guildId        = command.GuildId.GetValueOrDefault();
            bool  includeClasses = GuildManager.GuildIsHLMode(guildId);
            bool  mentionUsers   = GuildManager.GuildHasPingsEnabled(guildId);

            EmbedBuilder  embedBuilder = new EmbedBuilder();
            StringBuilder sb           = new StringBuilder();

            embedBuilder.WithTitle("Spinning for Teams!");
            embedBuilder.WithColor(Color.Teal);

            try
            {
                if (GuildManager.GuildGameHasEnded(guildId))
                {
                    MedicImmunePlayer[]? newImmunities = MedManager.GetTemporaryMedImmunePlayers(guildId);

                    if (newImmunities is not null)
                    {
                        foreach (MedicImmunePlayer player in newImmunities)
                        {
                            if (player is not null)
                            {
                                sb.AppendLine($"{player.DisplayName} will be granted med immunity for next medic spin");
                            }
                        }

                        embedBuilder.WithFooter(sb.ToString());

                        await MedManager.MakePermanentImmunitiesAsync(guildId);
                    }

                    await GuildManager.TryEndGuildGame(guildId);
                }


                var winners = RollRandomTeams(guildId, caller.VoiceChannel.ConnectedUsers, playersInVoice);
                if (winners.Item1.Length >= 6
                 && winners.Item2.Length >= 6)
                {
                    sb.Clear();                                                // use same builder for BLU team.
                    BuildTeamString(sb, winners.Item2, Team.BLU, mentionUsers, includeClasses); // Item 2 = BLU
                    embedBuilder.AddField("BLU Team:", sb.ToString(), true);

                    sb.Clear();                                                // use same builder for RED team.
                    BuildTeamString(sb, winners.Item1, Team.RED, mentionUsers, includeClasses); // Item 1 = RED
                    embedBuilder.AddField("RED Team:", sb.ToString(), true);


                    var players = winners.Item1.Concat(winners.Item2).ToList();
                    GuildManager.StartNewGuildGame(guildId, players);

                    await command.RespondAsync(embed: embedBuilder.Build());
                }

                await command.RespondAsync("Team roll failed, wrong amount of players?",
                                           ephemeral: true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        await command.RespondAsync("You are not in a voice channel with other players!", ephemeral: true);
    }

    private void BuildTeamString (StringBuilder stringBuilder, SocketGuildUser[] players, Team team,
                                  bool mentionUsers = true, bool includeClasses = false)
    {
        for (int i = 0; i < players.Length; i++)
        {
            var    player     = players[i];
            string playerName = mentionUsers ? $"<@!{player.Id}>" : $"{player.DisplayName}";

            if (includeClasses)
            {
                string rolledClass = i switch
                {
                    0 => "**Scout**",
                    1 => "**Soldier**",
                    2 => "**Pyro**",
                    4 => "**Demoman**",
                    5 => "**Heavy**",
                    6 => "**Engineer**",
                    7 => "**Medic**",
                    8 => "**Sniper**",
                    9 => "**Spy**",
                    _ => "Something went wrong :)"
                };
                if (i > 9) continue; // ???
                if (i == 7)
                    MedManager.PrepareTempMedImmunity(
                        player, team); // Grant temp med immunity if player got picked as med.
                stringBuilder.Append(rolledClass + ": ");
            }

            stringBuilder.Append($"{playerName} \n");
        }
    }

    private (SocketGuildUser[], SocketGuildUser[]) RollRandomTeams (ulong guildId,
                                                                    IReadOnlyCollection<SocketGuildUser> users,
                                                                    int playersInVoice)
    {
        Random rng = new Random();

        int maxTeamPlayers = 12;

        if (GuildManager.GuildIsHLMode(guildId))
        {
            maxTeamPlayers = 18;
        }


        var shuffled = users.OrderBy(u => rng.Next()).Take(maxTeamPlayers).ToList();

        bool[] picked = Enumerable.Repeat(true, maxTeamPlayers / 2)
                                  .Concat(Enumerable.Repeat(false, maxTeamPlayers - maxTeamPlayers / 2))
                                  .OrderBy(_ => rng).ToArray();
        SocketGuildUser[] red = shuffled.Where((u, i) => picked[i]).ToArray();
        SocketGuildUser[] blu = shuffled.Where((u, i) => !picked[i]).ToArray();
        return (red, blu);
    }
}