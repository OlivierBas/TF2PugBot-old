using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public abstract class BaseSpinCommand
{
    protected async Task<List<T>> Spin<T> (SocketSlashCommand command,
                                           IReadOnlyCollection<T> options,
                                           EmbedBuilder embedBuilder, SpinMode spinMode, bool instant)
    {
        List<string>  choices      = options.Select(c => c.ToString()).ToList();
        int           choicesCount = choices.Count;
        Random        rng          = new Random();
        StringBuilder sb           = new StringBuilder();

        await command.DeferAsync();
        if (instant)
        {
            int rollX = rng.Next(0, choicesCount);
            int rollY = rng.Next(0, choicesCount);
            while (rollY == rollX)
            {
                rollY = rng.Next(0, choicesCount);
            }

            await SendRollsAsync(rollX, rollY, command, sb, embedBuilder, choices, spinMode, true);
            return ChooseWinners(rollX, rollY, choicesCount, choices, options, spinMode);
        }

        if (instant == false)
        {
            int  roll = rng.Next(14, 34);
            bool lastIteration;
            for (int i = 0, y = 0, x = choicesCount / 2; i < roll; i++)
            {
                await Task.Delay(50);
                lastIteration = !(i < roll - 1);
                if (y == choicesCount)
                {
                    y = 0;
                }

                if (x == choicesCount)
                {
                    x = 0;
                }

                await SendRollsAsync(x, y, command, sb, embedBuilder, choices, spinMode, lastIteration);

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

                if (y == choicesCount)
                {
                    y = 0;
                }

                if (x == choicesCount)
                {
                    x = 0;
                }


                if (lastIteration)
                {
                    return ChooseWinners(x, y, choicesCount, choices, options, spinMode);
                }

                sb.Clear();

            }
        }


        return null;
    }
    
    protected async Task<List<SocketGuildUser>?> SpinUsers (SocketSlashCommand command,
                                                       IReadOnlyCollection<SocketGuildUser> connectedVoiceUsers,
                                                       EmbedBuilder embedBuilder, SpinMode spinMode, bool instant)
    {
        List<string>  players        = connectedVoiceUsers.Select(cu => cu.DisplayName).ToList();
        int           playersInVoice = connectedVoiceUsers.Count;
        Random        rng            = new Random();
        StringBuilder sb             = new StringBuilder();

        

        await command.DeferAsync();

        if (instant)
        {
            int rollX = rng.Next(0, playersInVoice);
            int rollY = rng.Next(0, playersInVoice);
            while (rollY == rollX)
            {
                rollY = rng.Next(0, playersInVoice);
            }

            await SendRollsAsync(rollX, rollY, command, sb, embedBuilder, players, spinMode, true);
            return ChooseUserWinners(rollX, rollY, playersInVoice, players, connectedVoiceUsers, spinMode);
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
                    return ChooseUserWinners(x, y, playersInVoice, players, connectedVoiceUsers, spinMode);
                }

                sb.Clear();

                Console.WriteLine($"iteration {i} / {roll} ");
            }
        }


        return null;
    }

    private async Task SendRollsAsync (int x, int y, SocketSlashCommand command, StringBuilder sb, EmbedBuilder eb,
                                       List<string> choices, SpinMode spinMode, bool finished)
    {
        foreach (var choice in choices)
        {
            if (choices[x] == choice
             || (spinMode == SpinMode.Duo && choices[y] == choice))
            {
                if (finished)
                {
                    sb.AppendLine("-> " + $"**{choice}**" + " <-");
                }
                else
                {
                    sb.AppendLine("-> " + $"{choice}" + " <-");
                }
            }
            else
            {
                sb.AppendLine(choice);
            }
        }

        eb.WithDescription(sb.ToString());
        await command.ModifyOriginalResponseAsync(mp => mp.Embed = eb.Build());
    }

    private List<T> ChooseWinners<T> (int x, int y, int maxChoices, List<string> choices, IReadOnlyCollection<T> options, SpinMode spinMode)
    {
        List<string> chosenPlayers = new List<string>();
        if (spinMode == SpinMode.Duo)
        {
            y = Math.Clamp(y, 0, maxChoices - 1);
            chosenPlayers.Add(choices[y]);
        }

        x = Math.Clamp(x, 0, maxChoices - 1);
        chosenPlayers.Add(choices[x]);

        List<T> spinWinners = options
                                            .Where(cu => chosenPlayers.Contains(cu.ToString()))
                                            .ToList();
        return spinWinners;

    }
    
    private List<SocketGuildUser> ChooseUserWinners (int x, int y, int maxPlayersInVoice, List<string> players, IReadOnlyCollection<SocketGuildUser> connectedVoiceUsers, SpinMode spinMode)
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