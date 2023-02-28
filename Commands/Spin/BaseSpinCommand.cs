using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public abstract class BaseSpinCommand
{
    protected async Task<List<SocketGuildUser>?> Spin (SocketSlashCommand command,
                                                       IReadOnlyCollection<SocketGuildUser> connectedVoiceUsers,
                                                       EmbedBuilder embedBuilder, SpinMode spinMode, bool instant)
    {
        List<string>  players        = connectedVoiceUsers.Select(cu => cu.DisplayName).ToList();
        int           playersInVoice = connectedVoiceUsers.Count;
        Random        rng            = new Random();
        StringBuilder sb             = new StringBuilder();


        if (playersInVoice < 3)
        {
            await command.RespondAsync("Too few players to spin.", ephemeral: true);
            return null;
        }

        await command.DeferAsync();

        if (instant)
        {
            int rollX = rng.Next(0, playersInVoice + 1);
            int rollY = rng.Next(0, playersInVoice + 1);
            while (rollY == rollX)
            {
                rollY = rng.Next(0, playersInVoice + 1);
            }

            await SendRollsAsync(rollX, rollY, command, sb, embedBuilder, players, spinMode, true);
            return ChooseWinners(rollX, rollY, playersInVoice, players, connectedVoiceUsers, spinMode);
        }

        if (instant == false)
        {
            int  roll          = rng.Next(14, 34);
            bool lastIteration;
            for (int i = 0, y = 0, x = playersInVoice / 2; i < roll; i++)
            {
                await Task.Delay(50);
                lastIteration = !(i < roll - 1);
                if (y == playersInVoice)
                {
                    y = 0;
                }

                if (x == playersInVoice)
                {
                    x = 0;
                }

                await SendRollsAsync(x, y, command, sb, embedBuilder, players, spinMode, lastIteration);

                if (spinMode == SpinMode.Duo
                 && (i % 2 == 0 || rng.Next(0, 2) == 1))
                {
                    y++;
                }
                else
                {
                    x++;
                }

                if (spinMode == SpinMode.Duo
                 && x == y)
                {
                    x++;
                }

                if (y == playersInVoice)
                {
                    y = 0;
                }

                if (x == playersInVoice)
                {
                    x = 0;
                }


                if (lastIteration)
                {
                    return ChooseWinners(x, y, playersInVoice, players, connectedVoiceUsers, spinMode);
                }

                sb.Clear();

                Console.WriteLine($"iteration {i} / {roll} ");
            }
        }


        return null;
    }

    private async Task SendRollsAsync (int x, int y, SocketSlashCommand command, StringBuilder sb, EmbedBuilder eb,
                                       List<string> players, SpinMode spinMode, bool finished)
    {
        foreach (var player in players)
        {
            if (players[x] == player
             || (spinMode == SpinMode.Duo && players[y] == player))
            {
                if (finished)
                {
                    sb.AppendLine("-> " + $"**{player}**" + " <-");
                }
                else
                {
                    sb.AppendLine("-> " + $"{player}" + " <-");
                }
            }
            else
            {
                sb.AppendLine(player);
            }
        }

        eb.WithDescription(sb.ToString());
        await command.ModifyOriginalResponseAsync(mp => mp.Embed = eb.Build());
    }

    private List<SocketGuildUser> ChooseWinners (int x, int y, int maxPlayersInVoice, List<string> players, IReadOnlyCollection<SocketGuildUser> connectedVoiceUsers, SpinMode spinMode)
    {
        List<string> chosenPlayers = new List<string>();
        if (spinMode == SpinMode.Duo)
        {
            y = Math.Clamp(y, 0, maxPlayersInVoice - 1);
            chosenPlayers.Add(players[y]);
        }

        x = Math.Clamp(x, 0, maxPlayersInVoice - 1);
        chosenPlayers.Add(players[x]);

        List<SocketGuildUser> spinWinners = connectedVoiceUsers
                                            .Where(cu => chosenPlayers.Contains(cu.DisplayName))
                                            .ToList();
        return spinWinners;

    }
}