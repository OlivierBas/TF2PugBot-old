namespace TF2PugBot.Types;

public enum CommandNames
{
    NotFound,
    CaptainSpin,
    MedicSpin,
    SetTeamChannel,
    SetAdminRole,
    SetPings,
    ModifyMedicImmunity,
    GetStats,
}

public enum SpinMode
{
    Solo,
    Duo
}

public enum Team
{
    RED,
    BLU
}

public enum StatTypes
{
    GamesPlayed,
    CaptainSpinsWon,
    MedicSpinsWon,
}

public enum SaveType
{
    MedImmunities,
    PlayerStats,
    GuildData,
    GuildMaps
}