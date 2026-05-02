using System;
using System.Collections.Generic;

namespace Gamelab.Serialization;

public class RunSession
{
    public int CurrentLevel { get; set; } = 0;
    public int Credits { get; set; } = 0;
    public List<StationSaveData> TrainLayout { get; set; } = new();
    public int RunSeed { get; set; } = Random.Shared.Next();

    public void AddCredits(int amount)
    {
        if (amount <= 0) return;
        Credits += amount;
    }

    public bool TrySpendCredits(int amount)
    {
        if (amount < 0 || Credits < amount) return false;
        Credits -= amount;
        return true;
    }
}