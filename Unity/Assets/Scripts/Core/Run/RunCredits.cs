using System;

namespace Gamelab.Run
{
    // Credit and shop-relevant part of Src RunSession. The RunSession save system is deferred
    // and will own or wrap this class.
    public class RunCredits
    {
        public int Credits { get; private set; }
        public int TotalUpgradesBought { get; private set; }
        public bool HubScreenCraftingHelpVisible { get; set; } = true;

        public event Action Changed;

        public void AddCredits(int amount)
        {
            if (amount <= 0) return;
            Credits += amount;
            Changed?.Invoke();
        }

        public bool TrySpendCredits(int amount)
        {
            if (amount < 0 || Credits < amount) return false;
            Credits -= amount;
            Changed?.Invoke();
            return true;
        }

        public void RegisterUpgradePurchased() => TotalUpgradesBought++;
    }
}
