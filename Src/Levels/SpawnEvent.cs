using Gamelab.Enemies.Core;

namespace Gamelab.Levels;

public class SpawnEvent
{
    /// <summary>Distance traveled (pixels) at which this enemy spawns.</summary>
    public float Distance { get; set; }

    /// <summary>
    /// Enemy type id. Valid values for this branch are "Rifle" and "TutorialRifle".
    /// </summary>
    public string Type { get; set; } = EnemyIds.Rifle;

    /// <summary>
    /// Enemy ammo preset id. Omit to use the baseline enemy loadout.
    /// </summary>
    public string AmmoId { get; set; } = EnemyAmmoIds.Basic;

    /// <summary>
    /// Optional approach side. Valid values for this branch are "Top" and "Bottom".
    /// Omit to let runtime/procedural generation choose randomly.
    /// </summary>
    public string Side { get; set; }
}
