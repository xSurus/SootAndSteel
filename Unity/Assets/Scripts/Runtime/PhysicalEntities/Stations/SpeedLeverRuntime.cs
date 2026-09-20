using System;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src SpeedLever. Dropped: the "Speed Change" lever sound and its "New Speed
    // Setting" parameter (NextIndex is exposed for whoever plays it).
    // Re-declares IInteractable so interface-typed calls reach OnInteract.
    public class SpeedLeverRuntime : StationRuntime, IInteractable
    {
        public Action<IPlayerActor> OnInteractOverride { get; set; }
        public ITrainState State { get; set; }
        // Speeds to cycle through; wave B passes the shared TrainSpeedSetting.Set.
        public TrainSpeedSetting.Set Speeds { get; set; }
        public int NextIndex { get; private set; }

        public override void Initialize(StationCatalogEntry catalogEntry)
        {
            base.Initialize(catalogEntry);
            if (string.IsNullOrEmpty(StationId)) StationId = StationIds.SpeedLever;
        }

        public void OnInteract(IPlayerActor interactingPlayer)
        {
            if (OnInteractOverride != null)
            {
                OnInteractOverride(interactingPlayer);
                return;
            }

            if (State.VictoryLapActive) return;

            if (!State.IsCoalOvenBurning)
            {
                State.SlowDownIfRunning();
                return;
            }

            // Src IndexOf: Stopped is not in All, so the index is -1 and next is 0 (Slow).
            var all = Speeds.All;
            int currentIndex = -1;
            for (int i = 0; i < all.Count; i++)
                if (ReferenceEquals(all[i], State.CurrentSpeed)) { currentIndex = i; break; }
            NextIndex = (currentIndex + 1) % all.Count;
            State.CurrentSpeed = all[NextIndex];
        }
    }
}
