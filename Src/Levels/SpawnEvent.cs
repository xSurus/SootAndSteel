namespace Gamelab.Levels;

public class SpawnEvent
{
    /// <summary>Distance traveled (pixels) at which this enemy spawns.</summary>
    public float Distance { get; set; }

    /// <summary>"Shooter" or "Thief"</summary>
    public string Type { get; set; } = "Shooter";

    /// <summary>
    /// Optional approach side. For Thief: "Top", "Bottom", "Right".
    /// For Shooter: "Top" or "Bottom" forces spawn above/below train; omit for random.
    /// </summary>
    public string Side { get; set; }
}
