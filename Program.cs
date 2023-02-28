using Discord;
using Discord.WebSocket;
using TF2PugBot.Commands.Management;
using TF2PugBot.Commands.Modify;
using TF2PugBot.Commands.Spin;
using TF2PugBot.Data;
using TF2PugBot.Helpers;
using TF2PugBot.Types;

namespace TF2PugBot;

public class Program
{
    private DiscordSocketClient? _client;
    private static ulong? _devGuildId;
    public static Task Main (string[] args)
    {        
        Console.Clear();
        if (String.IsNullOrEmpty(EasySetup.Token))
        {
            DataManager.Token = args[0];
            if (ulong.TryParse(args[1], out ulong guildId))
            {
                _devGuildId = guildId;
            }
            if (ulong.TryParse(args[2], out ulong devId))
            {
                DataManager.DevId = devId;
            }
            if (bool.TryParse(args[3], out bool instantSpin))
            {
                DataManager.InstantSpin = instantSpin;
            }
        }
        else
        {
            DataManager.Token       = EasySetup.Token;
            DataManager.InstantSpin = EasySetup.InstantSpins;
            DataManager.DevId       = EasySetup.OwnerId;
            _devGuildId             = EasySetup.DiscordServerId;
        }
        
        return new Program().MainAsync();
    }

    private async Task MainAsync ()
    {
        Console.WriteLine("Starting bot..");
        _client = new DiscordSocketClient();
        await _client.LoginAsync(TokenType.Bot, DataManager.Token);
        await _client.StartAsync();
        Game g = new Game(EasySetup.ActivityText, EasySetup.ActivityType);
        await _client.SetActivityAsync(g);

        _client.Ready                += Client_SetUp;
        _client.SlashCommandExecuted += Client_CommandHandler;
        _client.JoinedGuild          += Client_JoinedGuild;

        await Task.Delay(-1);
    }

    private async Task Client_SetUp ()
    {
        if (_devGuildId is null)
        {
            throw new Exception("Dev Guild Id is not set, use Args[1] or EasySetup.cs");
        }
        var devGuild = _client!.GetGuild(_devGuildId.GetValueOrDefault());


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


        var modifyImmunityCommand = new SlashCommandBuilder();
        modifyImmunityCommand.WithName("immunity");
        modifyImmunityCommand.WithDescription("Grant or Revoke medic immunities"); 
        modifyImmunityCommand.AddOption(new SlashCommandOptionBuilder()
                                         .WithName("grant")
                                         .WithDescription("Grant medic immunity")
                                         .WithType(ApplicationCommandOptionType.SubCommand)
                                         .AddOption("user", ApplicationCommandOptionType.User,
                                                    "The user to be given med immunity for 12 hours.", isRequired: true));
        modifyImmunityCommand.AddOption(new SlashCommandOptionBuilder()
                                        .WithName("revoke")
                                        .WithDescription("Revoke medic immunity")
                                        .WithType(ApplicationCommandOptionType.SubCommand)
                                        .AddOption("user", ApplicationCommandOptionType.User,
                                                   "The user to revoke medic immunity from", isRequired: true));

        modifyImmunityCommand.AddOption("get", ApplicationCommandOptionType.SubCommand, "Check all the medic immune players");

        try
        {
            await CommandCreator.CreateCommandAsync(devGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(devGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            await CommandCreator.CreateCommandAsync(devGuild, setTeamChannelCommand.Build(), CommandNames.SetTeamChannel);
            await CommandCreator.CreateCommandAsync(devGuild, setManagementRoleCommand.Build(), CommandNames.SetAdminRole);
            await CommandCreator.CreateCommandAsync(devGuild, modifyImmunityCommand.Build(), CommandNames.ModifyMedicImmunity);

            await _client.CreateGlobalApplicationCommandAsync(captainSpinCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(medicSpinCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(setTeamChannelCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(setManagementRoleCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(modifyImmunityCommand.Build());

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + "" + ex.StackTrace);
        }

        foreach (var joinedGuild in _client.Guilds)
        {
            await DataManager.InitializeGuildDataAsync(joinedGuild);
        }

        Console.WriteLine($"Bot is running in {_client.Guilds.Count} guilds!");
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
                        $"{command.CommandName} was attempted to execute, but it does not exist or is not assigned a CommandName");
                    break;
                case CommandNames.CaptainSpin:
                    await new SpinCaptainsCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.MedicSpin:
                    await new SpinMedicCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.SetTeamChannel:
                    await new ConfigureTeamChannelCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.SetAdminRole:
                    await new ConfigureAdminRoleCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.ModifyMedicImmunity:
                    await new ModifyMedImmunityCommand().PerformAsync(command, caller);
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
        await DataManager.InitializeGuildDataAsync(guild);
    }
    
}