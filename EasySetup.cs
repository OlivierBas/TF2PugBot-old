using Discord;

namespace TF2PugBot;

public static class EasySetup
{
    public const string MedDbFileName = "medimmunities";
    public const string GuildDbFileName = "guilddata";
    public const string StatsDbFileName = "playerstats";
    
    /// <summary>
    /// !IMPORTANT!
    /// The Token of the Discord Bot, given when creating a bot through discord's dev platform.
    /// If this value is set, all start-up arguments will be skipped.
    /// </summary>
    public const string Token = "";
    
    /// <summary>
    /// !IMPORTANT!
    /// The ID of the main discord server the bot should be used in,
    /// this skips the command "verification" process and allows for instant command editing.
    /// Should be set to add all commands immediately upon first start. 
    /// </summary>
    public const ulong DiscordServerId = 0; // numbers only
    
    /// <summary>
    /// !OPTIONAL!
    /// This is the ID given to the hoster of the bot (you)
    /// Will grant you permissions for the bot in every discord server.
    /// </summary>
    public const ulong OwnerId = 624280077982629888; // numbers only

    /// <summary>
    /// !OPTIONAL! (Default true)
    /// Setting this to true will skip the "animated" spin of both Team Captain & Medic spinning.
    /// This will slow down the bot usage by a lot and will freeze the bot until completion (lack of multithreading)
    /// Recommended to be set to true
    /// </summary>
    public const bool InstantSpins = true; // true or false

    /// <summary>
    /// !OPTIONAL!
    /// Text that will be displayed under the bot's name. ActivityType.Competing will show "Competing" + the ActivityText
    /// The possible ActivityTypes are:
    ///     ActivityType.Competing
    ///     ActivityType.Watching
    ///     ActivityType.Streaming
    ///     ActivityType.Playing
    ///     ActivityType.Listening
    /// </summary>
    public const string ActivityText                    = "IN EPIC MIXES";
    public static readonly ActivityType ActivityType    = ActivityType.Competing;
}