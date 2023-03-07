using Discord;
using Discord.WebSocket;
using TF2PugBot.Commands.Management;
using TF2PugBot.Commands.Modify;
using TF2PugBot.Commands.Spin;
using TF2PugBot.Commands.Stats;
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
            try
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
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Not enough arguments were passed, order is: Token  Guild Id  Dev Id  Instant Spin ");
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
        Game game = new Game(EasySetup.ActivityText, EasySetup.ActivityType);
        await _client.SetActivityAsync(game);

        _client.UserVoiceStateUpdated += Client_UserStateChanged;
        _client.Ready                 += Client_SetUp;
        _client.SlashCommandExecuted  += Client_CommandHandler;
        _client.JoinedGuild           += Client_JoinedGuild;

        await Task.Delay(-1);
    }

    private async Task Client_SetUp ()
    {
        if (_devGuildId is null)
        {
            throw new Exception("Dev Guild Id is not set, use Args[1] or EasySetup.cs");
        }

        var devGuild = _client!.GetGuild(_devGuildId.GetValueOrDefault());


        var captainSpinCommand = new SlashCommandBuilder()
                                 .WithName("spinforcaptain")
                                 .WithDescription("Spin for Captain (Be in Voice Channel)");

        var medicSpinCommand = new SlashCommandBuilder()
                               .WithName("spinformedic")
                               .WithDescription("Spin for Medic (Be in Voice Channel)");

        var getStatsCommand = new SlashCommandBuilder()
                              .WithName("stats")
                              .WithDescription("Get stats of user")
                              .AddOption("user", ApplicationCommandOptionType.User, "The user to check stats for",
                                         isRequired: false);

        var setPingUsersCommand = new SlashCommandBuilder()
                                  .WithName("configure-pings")
                                  .WithDescription("Enable or disable user mentions after a won spin")
                                  .AddOption("value", ApplicationCommandOptionType.Boolean,
                                             "Whether to ping users when they have won a roll");


        var setTeamChannelCommand = new SlashCommandBuilder()
                                    .WithName("configure-channels")
                                    .WithDescription("Set Team channel for Medic Spin")
                                    .AddOption(new SlashCommandOptionBuilder()
                                               .WithName("team")
                                               .WithDescription("Sets the specified team's channel")
                                               .WithType(ApplicationCommandOptionType.SubCommandGroup)
                                               .AddOption(new SlashCommandOptionBuilder()
                                                          .WithName("blu")
                                                          .WithDescription("Set BLU Team's channel")
                                                          .WithType(ApplicationCommandOptionType.SubCommand)
                                                          .AddOption("channel", ApplicationCommandOptionType.Channel,
                                                                     "The voice channel to be used for the BLU team",
                                                                     isRequired: true))
                                               .AddOption(new SlashCommandOptionBuilder()
                                                          .WithName("red")
                                                          .WithDescription("Set RED Team's channel")
                                                          .WithType(ApplicationCommandOptionType.SubCommand)
                                                          .AddOption("channel", ApplicationCommandOptionType.Channel,
                                                                     "The voice channel to be used for the RED team",
                                                                     isRequired: true))
                                    );

        var setManagementRoleCommand = new SlashCommandBuilder()
                                       .WithName("configure-admins")
                                       .WithDescription("Set Admin Role for bot management")
                                       .AddOption("role", ApplicationCommandOptionType.Role,
                                                  "The role that is able to configure this bot and grant or revoke immunities",
                                                  isRequired: true);


        var modifyImmunityCommand = new SlashCommandBuilder()
                                    .WithName("immunity")
                                    .WithDescription("Grant or Revoke medic immunities")
                                    .AddOption(new SlashCommandOptionBuilder()
                                               .WithName("grant")
                                               .WithDescription("Grant medic immunity")
                                               .WithType(ApplicationCommandOptionType.SubCommand)
                                               .AddOption("user", ApplicationCommandOptionType.User,
                                                          "The user to be given med immunity for 12 hours.",
                                                          isRequired: true));
        modifyImmunityCommand.AddOption(new SlashCommandOptionBuilder()
                                        .WithName("revoke")
                                        .WithDescription("Revoke medic immunity")
                                        .WithType(ApplicationCommandOptionType.SubCommand)
                                        .AddOption("user", ApplicationCommandOptionType.User,
                                                   "The user to revoke medic immunity from", isRequired: true));

        modifyImmunityCommand.AddOption("get", ApplicationCommandOptionType.SubCommand,
                                        "Check all the medic immune players");

        try
        {
            await CommandCreator.CreateCommandAsync(devGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(devGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            await CommandCreator.CreateCommandAsync(devGuild, setTeamChannelCommand.Build(),
                                                    CommandNames.SetTeamChannel);
            await CommandCreator.CreateCommandAsync(devGuild, setManagementRoleCommand.Build(),
                                                    CommandNames.SetAdminRole);
            await CommandCreator.CreateCommandAsync(devGuild, modifyImmunityCommand.Build(),
                                                    CommandNames.ModifyMedicImmunity);
            await CommandCreator.CreateCommandAsync(devGuild, getStatsCommand.Build(), CommandNames.GetStats);
            await CommandCreator.CreateCommandAsync(devGuild, setPingUsersCommand.Build(), CommandNames.SetPings);

            /*await _client.CreateGlobalApplicationCommandAsync(captainSpinCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(medicSpinCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(setTeamChannelCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(setManagementRoleCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(modifyImmunityCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(getStatsCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(setPingUsersCommand.Build());*/
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
        if (command.GuildId is null)
        {
            await command.RespondAsync("Commands are only to be used in guilds", ephemeral: true);
            return;
        }

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
                case CommandNames.SetPings:
                    await new ConfigurePingsCommand().PerformAsync(command, caller);
                    break;
                case CommandNames.GetStats:
                    await new GetStatsCommand().PerformAsync(command, caller);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
    }

    private async Task Client_JoinedGuild (SocketGuild guild)
    {
        await DataManager.InitializeGuildDataAsync(guild);
    }

    private async Task Client_UserStateChanged (SocketUser user, SocketVoiceState previousState,
                                                SocketVoiceState newState)
    {
        ulong guildId   = previousState.VoiceChannel.Guild.Id;
        if (!DataManager.GuildGameHasEnded(guildId))
        {
            if (DataManager.GetGuildTeamChannel(guildId, newState.VoiceChannel.Id) != null)
            {
                DataManager.AddPlayerToGuildGame(guildId, user.Id);
            }
            else
            {
                DataManager.RemovePlayerFromGuildGame(guildId, user.Id);
            }
        }
    }
}