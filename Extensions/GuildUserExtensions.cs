using Discord.WebSocket;

namespace TF2PugBot.Extensions;

public static class GuildUserExtensions
{
    public static bool IsConnectedToVoice (this SocketGuildUser user)
    {
        if (user.VoiceChannel is not null) return true;
        return false;
    }
}