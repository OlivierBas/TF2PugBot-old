﻿using Discord.WebSocket;

namespace TF2PugBot.Types;

public class MedicImmunePlayer
{
    public ulong Id { get; set; } = default;
    public ulong GuildId { get; set; } = default;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime Added { get; set; } = DateTime.Now;

    public static implicit operator MedicImmunePlayer (SocketGuildUser user) => new MedicImmunePlayer()
        { Id = user.Id, DisplayName = user.DisplayName, GuildId = user.Guild.Id };

    /// <inheritdoc />
    public override bool Equals (object? obj)
    {
        var mip = obj as MedicImmunePlayer;
        if (mip != null)
        {
            return mip.Id == Id && mip.GuildId == GuildId;
        }
        return base.Equals(obj);
    }
}