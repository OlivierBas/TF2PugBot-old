using Discord.WebSocket;
using TF2PugBot.Types;

namespace TF2PugBot.Helpers;

public static class CommandHandler
{
    private static Dictionary<CommandNames, string> _commands = new Dictionary<CommandNames, string>();

    public static void AddCommand (SocketApplicationCommand command, CommandNames givenName)
    {
        if (_commands.ContainsKey(givenName))
        {
            throw new Exception("Command Name already in use!");
        }
        _commands.Add(givenName, command.Name);
    }

    public static CommandNames GetCommandName (SocketSlashCommand command)
    {
        return _commands.FirstOrDefault(v => v.Value == command.CommandName).Key;
    }

}