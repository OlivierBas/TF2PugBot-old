using Discord;
using Discord.WebSocket;
using TF2PugBot.Types;

namespace TF2PugBot.Helpers;

public static class CommandCreator
{
    public static async Task<SocketApplicationCommand?> CreateCommandAsync (SocketGuild? guild, SlashCommandProperties command, CommandNames givenName)
    {
        if (guild is not null)
        {
            SocketApplicationCommand addedCommand = await guild.CreateApplicationCommandAsync(command);
            CommandHandler.AddCommand(addedCommand, givenName);
            return addedCommand;
        }

        return null;
    }
}