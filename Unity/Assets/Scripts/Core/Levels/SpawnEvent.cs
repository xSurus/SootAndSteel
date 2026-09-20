using Gamelab.Enemies.Core;

namespace Gamelab.Levels
{
    public class SpawnEvent
    {
        /// <summary>Distance traveled (pixels) at which this enemy spawns.</summary>
        public float Distance { get; set; }

        /// <summary>Enemy type id, "Rifle" or "TutorialRifle".</summary>
        public string Type { get; set; } = EnemyIds.Rifle;

        /// <summary>Enemy ammo preset id. Defaults to the baseline loadout.</summary>
        public string AmmoId { get; set; } = EnemyAmmoIds.Basic;

        /// <summary>Approach side, "Top" or "Bottom". Null lets the runtime choose.</summary>
        public string Side { get; set; }
    }
}
