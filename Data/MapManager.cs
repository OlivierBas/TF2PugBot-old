﻿using System.Text;
using TF2PugBot.Extensions;
using TF2PugBot.Types;

namespace TF2PugBot.Data;

public static class MapManager
{
    private static List<GuildMapData> _guildMapData = new List<GuildMapData>();
    private static Dictionary<ulong, IgnoredSixesMap> _trackedMapIgnore = new Dictionary<ulong, IgnoredSixesMap>();

    public static List<GuildMapData> GuildMapData
    {
        get => _guildMapData;
        set => _guildMapData = value;
    }

    public static async Task<List<SixesMap>> GetGuildMapsAsync (ulong guildId)
    {
        var guildMapData = _guildMapData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildMapData is null)
        {
            _guildMapData.Add(new GuildMapData() { GuildId = guildId });
            await DataManager.SaveDbAsync(SaveType.GuildMaps);
            return _guildMapData.FirstOrDefault(g => g.GuildId == guildId)!.Maps;
        }

        return guildMapData.Maps;
    }

    public static async Task<List<IgnoredSixesMap>> GetIgnoredMapsAsync (ulong guildId)
    {
        var guildMapData = _guildMapData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildMapData is null)
        {
            _guildMapData.Add(new GuildMapData() { GuildId = guildId });
            return _guildMapData.FirstOrDefault(g => g.GuildId == guildId)!.IgnoredMaps;
        }

        if (_trackedMapIgnore.ContainsKey(guildId)
         && _trackedMapIgnore[guildId].Added.MinutesFromNow() >= Constants.GuildGameMinDuration)
        {
            TryAddIgnoredMap(guildId, _trackedMapIgnore[guildId]);
            await DataManager.SaveDbAsync(SaveType.GuildMaps);
        }

        return guildMapData.IgnoredMaps;
    }

    public static async Task<string> GetIgnoredMapsAsStringAsync (ulong guildId)
    {
        var ignoredMaps = await GetIgnoredMapsAsync(guildId);

        
        StringBuilder sb = new StringBuilder();
        foreach (var map in ignoredMaps)
        {
            sb.AppendLine($"{map} will be ignored for this spin");
        }

        return sb.ToString();
    }

    public static async Task ClearIgnoredMapsAsync (ulong guildId)
    {
        var data = _guildMapData.FirstOrDefault(g => g.GuildId == guildId);
        if (data is not null)
        {
            data.IgnoredMaps.RemoveAll(m => m.Added.HoursFromNow() >= data.HoursBeforeMapClear);
            await DataManager.SaveDbAsync(SaveType.GuildMaps);
        }
    }

    public static async Task<bool> TryAddMapToGuildMapsAsync (ulong guildId, string mapName)
    {
        if (mapName.Length < 4)
        {
            return false;
        }

        var maps = await GetGuildMapsAsync(guildId);
        if (maps.Count >= 20)
        {
            return false;
        }
        
        mapName = mapName.FirstLetterUppercase();

        if (maps.Exists(m => m.MapName == mapName))
        {
            return false;
        }
        
        maps.Add(mapName);
        await DataManager.SaveDbAsync(SaveType.GuildMaps);
        return true;
    }

    private static void TryAddIgnoredMap (ulong guildId, IgnoredSixesMap ignoredMap)
    {
        var guildMapData = _guildMapData.FirstOrDefault(gm => gm.GuildId == guildId);
        if (guildMapData is not null)
        {
            if (guildMapData.IgnoredMaps.Contains(ignoredMap))
            {
                return;
            }

            guildMapData.IgnoredMaps.Add(ignoredMap);
            _trackedMapIgnore[guildId] = new IgnoredSixesMap();
            

        }
    }

    public static async Task<bool> TryRemoveMapFromGuildMapsAsync (ulong guildId, string mapName)
    {
        mapName = mapName.FirstLetterUppercase();
        var maps = await GetGuildMapsAsync(guildId);
        maps.RemoveAll(m => m.MapName == mapName);
        await DataManager.SaveDbAsync(SaveType.GuildMaps);
        return true;
    }

    public static void PrepareMapIgnore (ulong guildId, string mapName)
    {
        if (!_trackedMapIgnore.ContainsKey(guildId))
        {
            _trackedMapIgnore.Add(guildId, new IgnoredSixesMap() { MapName = mapName });
        }
        else
        {
            _trackedMapIgnore[guildId] = new IgnoredSixesMap() { MapName = mapName };
        }
    }

    public static async Task UpdateMapTimeout (ulong guildId, int hours)
    {
        var guildMapData = _guildMapData.FirstOrDefault(g => g.GuildId == guildId);
        if (guildMapData is not null)
        {
            guildMapData.HoursBeforeMapClear = hours;
            await DataManager.SaveDbAsync(SaveType.GuildMaps);
        }
        else
        {
            _guildMapData.Add(new GuildMapData() { GuildId = guildId, HoursBeforeMapClear = hours });
            await DataManager.SaveDbAsync(SaveType.GuildMaps);
        }
    }
}