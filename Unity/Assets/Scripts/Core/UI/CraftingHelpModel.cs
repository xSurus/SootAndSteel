using System;
using Gamelab.Run;

namespace Gamelab.UI
{
    /// <summary>Crafting help panel visibility, persisted in RunCredits like Src RunSession.HubScreenCraftingHelpVisible.</summary>
    public class CraftingHelpModel
    {
        private readonly RunCredits run;

        public CraftingHelpModel(RunCredits run)
        {
            this.run = run;
        }

        public bool Visible
        {
            get => run.HubScreenCraftingHelpVisible;
            set
            {
                if (run.HubScreenCraftingHelpVisible == value) return;
                run.HubScreenCraftingHelpVisible = value;
                Changed?.Invoke();
            }
        }

        public event Action Changed;

        public void Toggle() => Visible = !Visible;
    }
}
