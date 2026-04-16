namespace Gamelab.Levels;

public class SpawnEvent
{
    /// <summary>Distance traveled (pixels) at which this enemy spawns.</summary>
    public float Distance { get; set; }

    /// <summary>
    /// Enemy type id. Valid values are: "Mounter", "Rifle", "Shield", "Anchor", "Molotov", "TarThrower".
    /// </summary>
    public string Type { get; set; } = "Rifle";

    /// <summary>
    /// Optional approach side. Valid values for this branch are "Top" and "Bottom".
    /// Omit to let runtime/procedural generation choose randomly.
    /// </summary>
    public string Side { get; set; }
}
