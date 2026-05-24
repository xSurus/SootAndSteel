using System;
using System.Collections.Generic;

namespace Gamelab.Serialization;

public class RunSession
{
    public int CurrentLevel { get; set; } = 0;
    public int Credits { get; set; } = 0;
    public List<StationSaveData> TrainLayout { get; set; } = new();
    public int RunSeed { get; set; } = Random.Shared.Next();
    public bool TutorialCompleted { get; set; } = false;
    public int TotalEnemiesNeutralized { get; set; } = 0;
    public int TotalDistanceTravelledMeters { get; set; } = 0;
    public int TotalUpgradesBought { get; set; } = 0;
    public bool HubScreenCraftingHelpVisible { get; set; } = true;

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

    public void AddEnemiesNeutralized(int amount)
    {
        if (amount <= 0) return;
        TotalEnemiesNeutralized += amount;
    }

    public void AddDistanceTravelled(float distanceMeters)
    {
        if (distanceMeters <= 0f) return;
        TotalDistanceTravelledMeters += (int)Math.Round(distanceMeters);
    }

    public void RegisterUpgradePurchased() => TotalUpgradesBought++;
}