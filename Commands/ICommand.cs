using Discord.WebSocket;

namespace TF2PugBot.Commands;

public interface ICommand
{
    /// <summary>
    /// Performs the <paramref name="command"/>
    /// </summary>
    /// <param name="command">The Command to be performed</param>
    /// <param name="caller">The User that called the command</param>
    public Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller);
}