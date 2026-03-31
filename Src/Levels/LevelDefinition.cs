using System.Collections.Generic;

namespace Gamelab.Levels;

public class LevelDefinition
{
    /// <summary>Total distance (pixels) the train must travel to complete this level.</summary>
    public float LevelDistance { get; set; } = 3000f;

    /// <summary>Ordered list of enemy spawn events, sorted by Distance ascending.</summary>
    public List<SpawnEvent> SpawnEvents { get; set; } = new();
}
