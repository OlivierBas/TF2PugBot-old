using System.Text;
using Discord;
using Discord.WebSocket;
using TF2PugBot.Types;

namespace TF2PugBot.Commands.Spin;

public abstract class BaseSpinCommand
{
    protected async Task<List<SocketGuildUser>?> Spin (SocketSlashCommand command, IReadOnlyCollection<SocketGuildUser> connectedVoiceUsers, EmbedBuilder embedBuilder, SpinMode spinMode)
    {
        List<string> chosenPlayers  = new List<string>();
        List<string> players        = connectedVoiceUsers.Select(cu => cu.DisplayName).ToList();
        int          playersInVoice = connectedVoiceUsers.Count;
        Random       rng            = new Random();
        int          max            = rng.Next(10, 20);
        
        StringBuilder sb = new StringBuilder();
        await command.DeferAsync();

        for (int i = 0, y = 0, x = playersInVoice / 2; i < max; i++)
        {
            if (y == playersInVoice)
            {
                y = 0;
            }

            if (x == playersInVoice)
            {
                x = 0;
            }


            foreach (var player in players)
            {
                if (players[x] == player
                 || (spinMode == SpinMode.Duo && players[y] == player))
                {
                    sb.AppendLine("-> " + $"**{player}**");
                }
                else
                {
                    sb.AppendLine(player);
                }
            }
            embedBuilder.WithDescription(sb.ToString());



            /*if (rng.Next(0, 20) % 2 == 0)
            {
                y++;
                if (y == x)
                {
                    y++;
                }
            }
            else if (rng.Next(0, 20) % 2 == 0)
            {

                x++;
                if (x == y)
                {
                    x++;
                }
            }*/

            if (spinMode == SpinMode.Duo && i % 2 == 0)
            {
                y++;
            }
            else
            {
                x++;
            }

            if (spinMode == SpinMode.Duo && x == y)
            {
                x++;
            }

            await command.ModifyOriginalResponseAsync(mp => mp.Embed = embedBuilder.Build());
            sb.Clear();
            if (i == max - 1)
            {

                if (spinMode == SpinMode.Duo)
                {
                    y = Math.Clamp(y, 0, playersInVoice - 1);
                    chosenPlayers.Add(players[y]);
                }

                x = Math.Clamp(x, 0, playersInVoice - 1);
                chosenPlayers.Add(players[x]);

                List<SocketGuildUser> spinWinners = connectedVoiceUsers.Where(cu => chosenPlayers.Contains(cu.DisplayName))
                                     .ToList();
                return spinWinners;
            }
        }

        return null;
    }
}