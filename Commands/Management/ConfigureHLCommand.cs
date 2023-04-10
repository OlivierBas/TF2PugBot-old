﻿using Discord.WebSocket;
using TF2PugBot.Data;

namespace TF2PugBot.Commands.Management;

public class ConfigureHLCommand : ICommand
{
    /// <inheritdoc />
    public async Task PerformAsync (SocketSlashCommand command, SocketGuildUser caller)
    {
        if (GuildManager.HasAccessToCommand(command.GuildId, caller))
        {
            var argBool = command.Data.Options.First().Value;

            try
            {
                bool value = (bool )argBool;
                await GuildManager.SetGuildHLModeAsync(command.GuildId, value);
                
                string action = value ? "enabled" : "disabled";
                await command.RespondAsync($"Successfully `{action}` HL Mode", ephemeral: true);
                return;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync("Something went wrong.", ephemeral: true);
            }
        }

        await command.RespondAsync("You do not have access to this command", ephemeral: true);

    }
}