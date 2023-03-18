namespace TF2PugBot.Types;

public class SixesMap
{ 
    public string MapName { get; set; }

    /// <inheritdoc />
    public override string ToString ()
    {
        return MapName;
    }

    public static implicit operator SixesMap (string mapName) => new SixesMap() { MapName = mapName };
    public static implicit operator string (SixesMap map) => map.MapName;
}