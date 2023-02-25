using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

        _client.Ready += Client_SetUp;
        _client.SlashCommandExecuted += Client_CommandHandler;

        await Task.Delay(-1);
    }

    private async Task Client_SetUp ()
    {
        var devGuild = _client!.GetGuild(654049424439377971);
        var mixGuild = _client!.GetGuild(1058364795063124008);
        
        
        var testingCommand = new SlashCommandBuilder();
        testingCommand.WithName("testing");
        testingCommand.WithDescription("Example command");

        var captainSpinCommand = new SlashCommandBuilder();
        captainSpinCommand.WithName("spinforcaptain");
        captainSpinCommand.WithDescription("Spin for Captain (Be in Voice Channel)");
        
        var medicSpinCommand = new SlashCommandBuilder();
        medicSpinCommand.WithName("spinformedic");
        medicSpinCommand.WithDescription("Spin for Medic (Be in Voice Channel)");

        try
        {

            await CommandCreator.CreateCommandAsync(devGuild, testingCommand.Build(), CommandNames.Testing);
            await CommandCreator.CreateCommandAsync(devGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(devGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            await CommandCreator.CreateCommandAsync(mixGuild, testingCommand.Build(), CommandNames.Testing);
            await CommandCreator.CreateCommandAsync(mixGuild, captainSpinCommand.Build(), CommandNames.CaptainSpin);
            await CommandCreator.CreateCommandAsync(mixGuild, medicSpinCommand.Build(), CommandNames.MedicSpin);
            
        }
        catch (CommandException ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine($"Client is running and listening in {_client.Guilds.Count} guilds!");
    }

    private async Task Client_CommandHandler (SocketSlashCommand command)
    {
        SocketGuildUser caller = _client!.GetGuild(command.GuildId.GetValueOrDefault()).GetUser(command.User.Id);

        switch (CommandHandler.GetCommandName(command))
        {
            default:
            case CommandNames.NotFound:
                Console.WriteLine($"{command.CommandName} was attempted to execute, it does not exist or is not assigned a CommandName");
                break;
            case CommandNames.CaptainSpin:
                await new SpinCaptainsCommand().Perform(command, caller);
                break;
            case CommandNames.MedicSpin:
                await new SpinMediCommand().Perform(command, caller);
                break;
            
            
            /*case CommandNames.Testing:
                await command.RespondAsync("Testing!");
                break;*/
        }
    }
    
}