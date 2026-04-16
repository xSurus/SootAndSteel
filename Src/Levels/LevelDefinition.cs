using System.Collections.Generic;

namespace Gamelab.Levels;

public class LevelDefinition
{
    public float LevelDistance { get; set; } = 3000f;
    public List<SpawnEvent> SpawnEvents { get; set; } = new();
}
