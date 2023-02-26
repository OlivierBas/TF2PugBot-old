using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TF2PugBot.Types;

public class GuildTeamChannelData
{

    private ulong? _redTeamVoiceChannelId;
    private ulong? _bluTeamVoiceChannelId;
    
    public ulong GuildId { get; set; } = default;


    public ulong? RedTeamVoiceChannelId
    {
        get => _redTeamVoiceChannelId;
        set => TryUpdateValue(Team.RED, value.GetValueOrDefault());
    }


    public ulong? BluTeamVoiceChannelId
    {
        get => _bluTeamVoiceChannelId;
        set => TryUpdateValue(Team.BLU, value.GetValueOrDefault());
    }

    public bool TryUpdateValue (Team team, ulong channelId)
    {
        switch (team)
        {
            case Team.RED:
                if (channelId == _bluTeamVoiceChannelId)
                {
                    return false;
                }

                _redTeamVoiceChannelId = channelId;
                return true;
            
            case Team.BLU:
                if (channelId == _redTeamVoiceChannelId)
                {
                    return false;
                }

                _bluTeamVoiceChannelId = channelId;
                return true;
        }

        return false;
    }

    

}