using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TF2PugBot.Commands.Management;
using TF2PugBot.Commands.Spin;
using TF2PugBot.Helpers;
using TF2PugBot.Types;

namespace TF2PugBot;

public class Program
{
    private DiscordSocketClient? _client;

    public static Task Main (string[] args)
    {
        DataManager.Token = args[0];
        return new Program().MainAsync();
    }

    private async Task MainAsync ()
    {
        _client = new DiscordSocketClient();
        await _client.LoginAsync(TokenType.Bot, DataManager.Token);
        await _client.StartAsync();
        Game g = new Game("IN EPIC MIXES", ActivityType.Competing);
        await _client.SetActivityAsync(g);

        _client.Ready                += Client_SetUp;
        _client.SlashCommandExecuted += Client_CommandHandler;
        _client.JoinedGuild          += Client_JoinedGuild;

        await Task.Delay(-1);
    }

    private async Task Client_SetUp ()
    {
        var devGuild = _client!.GetGuild(654049424439377971);
        var mixGuild = _client!.GetGuild(1058364795063124008);

        var captainSpinCommand = new SlashCommandBuilder();
        captainSpinCommand.WithName("spinforcaptain");
        captainSpinCommand.WithDescription("Spin for Captain (Be in Voice Channel)");
        
        var medicSpinCommand = new SlashCommandBuilder();
        medicSpinCommand.WithName("spinformedic");
        medicSpinCommand.WithDescription("Spin for Medic (Be in Voice Channel)");


        var setTeamChannelCommand = new SlashCommandBuilder();
        setTeamChannelCommand.WithName("configure-channels");
        setTeamChannelCommand.WithDescription("Set Team channel for Medic Spin");
        setTeamChannelCommand.AddOption(new SlashCommandOptionBuilder()
                                        .WithName("team")
                                        .WithDescription("Sets the specified team's channel")
                                        .WithType(ApplicationCommandOptionType.SubCommandGroup)
                                        .AddOption(new SlashCommandOptionBuilder()
                                                   .WithName("blu")
                                                   .WithDescription("Set BLU Team's channel")
                                                   .WithType(ApplicationCommandOptionType.SubCommand)
                                                   .AddOption("channel", ApplicationCommandOptionType.Channel, "The voice channel to be used for the BLU team", isRequired: true))
                                        .AddOption(new SlashCommandOptionBuilder()
                                                   .WithName("red")
                                                   .WithDescription("Set RED Team's channel")
                                                   .WithType(ApplicationCommandOptionType.SubCommand)
                                                   .AddOption("channel", ApplicationCommandOptionType.Channel, "The voice channel to be used for the RED team", isRequired: true))
                                        );
        
        var setManagementRoleCommand = new SlashCommandBuilder();
        setManagementRoleCommand.WithName("configure-admins");
        setManagementRoleCommand.WithDescription("Set Admin Role for bot management");
        setManagementRoleCommand.AddOption("role", ApplicationCommandOptionType.Role, "The role that is able to configure this bot and grant or revoke immunities", isRequired: true);


        try
        {
            await CommandCreator.CreateCommandAsync(devGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(devGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            await CommandCreator.CreateCommandAsync(devGuild, setTeamChannelCommand.Build(), CommandNames.SetTeamChannel);
            await CommandCreator.CreateCommandAsync(devGuild, setManagementRoleCommand.Build(), CommandNames.SetAdminRole);
            
            await CommandCreator.CreateCommandAsync(mixGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(mixGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + "" + ex.StackTrace);
        }

        foreach (var joinedGuild in _client.Guilds)
        {
            DataManager.InitializeGuildData(joinedGuild);
        }

        Console.WriteLine($"Client is running and listening in {_client.Guilds.Count} guilds!");
    }

    private async Task Client_CommandHandler (SocketSlashCommand command)
    {
        SocketGuildUser caller = _client!.GetGuild(command.GuildId.GetValueOrDefault()).GetUser(command.User.Id);
        try
        {
            switch (CommandHandler.GetCommandName(command))
            {
                default:
                case CommandNames.NotFound:
                    Console.WriteLine(
                        $"{command.CommandName} was attempted to execute, it does not exist or is not assigned a CommandName");
                    break;
                case CommandNames.CaptainSpin:
                    await new SpinCaptainsCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.MedicSpin:
                    await new SpinMediCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.SetTeamChannel:
                    await new ConfigureTeamChannelCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.SetAdminRole:
                    await new ConfigureAdminRoleCommand().PerformAsync(command, caller);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private async Task Client_JoinedGuild (SocketGuild guild)
    {
        DataManager.InitializeGuildData(guild);
    }
    
}