using System;
using System.Collections.Generic;

namespace Gamelab.UI
{
    /// <summary>
    /// Ready, hint, depart-decision and hold-timer logic of Src HubScreen.UpdateDepartureLogic.
    /// The lever interaction calls ToggleReady. Update runs once per frame while not paused.
    /// </summary>
    public class HubDepartureModel
    {
        public const float LeverHintDelaySeconds = 5f;
        public const float HintVisibleSeconds = 8f;
        public const float DecisionInputBlockSeconds = 0.2f;

        private const string HintKeyLever = "lever";
        private const string HintText = "To depart, each player should interact with the lever.";
        private const string DecisionText = "Bought items are still off-board. Depart anyway?";

        private readonly float departHoldSeconds;
        private readonly HashSet<int> readyPlayers = new HashSet<int>();
        private readonly List<int> joined = new List<int>();

        private float hubElapsed;
        private int? lastReadyPlayerIndex;
        private bool allowDepartWithPendingItems;
        private int pendingCount;
        private int previousPendingCount;
        private string hintKey;
        private float hintVisibleTimer;
        private int? decisionPlayerIndex;
        private float decisionInputBlockTimer;

        public HubDepartureModel(float departHoldSeconds = 0.75f)
        {
            this.departHoldSeconds = departHoldSeconds;
        }

        public DialogBubbleModel Bubble { get; } = new DialogBubbleModel();
        public IReadOnlyCollection<int> ReadyPlayers => readyPlayers;
        public bool AllReady { get; private set; }
        public bool IsDecisionOpen { get; private set; }
        public bool Departing { get; private set; }
        public float HoldTimer { get; private set; }

        public event Action Changed;
        public event Action DepartRequested;

        public bool IsReady(int playerIndex) => readyPlayers.Contains(playerIndex);

        /// <summary>Index = position in the joined list (colour slot), as Src HubOverlay.SyncReadySprites. Slots beyond 4 are ignored.</summary>
        public bool[] ReadySlots
        {
            get
            {
                var slots = new bool[4];
                for (int i = 0; i < joined.Count && i < slots.Length; i++)
                    slots[i] = readyPlayers.Contains(joined[i]);
                return slots;
            }
        }

        /// <summary>
        /// Lever seam. Uses the joined list and pending count from the most recent Update, so it can be
        /// one frame stale. Src reads them live from the world.
        /// </summary>
        public void ToggleReady(int playerIndex)
        {
            if (IsDecisionOpen) return;

            bool justBecameReady = readyPlayers.Add(playerIndex);
            if (justBecameReady)
                lastReadyPlayerIndex = playerIndex;
            else
            {
                readyPlayers.Remove(playerIndex);
                if (lastReadyPlayerIndex == playerIndex)
                    lastReadyPlayerIndex = null;
            }

            allowDepartWithPendingItems = false;

            if (justBecameReady && ComputeAllReady() && pendingCount > 0)
                OpenDecision(playerIndex);

            AllReady = ComputeAllReady();
            Changed?.Invoke();
        }

        public void Update(float dt, IReadOnlyList<int> joinedPlayerIndices, int pendingOffBoardCount,
            bool interactPressed, bool grabPressed)
        {
            if (Departing) return;

            hubElapsed += dt;
            UpdateDecisionInput(dt, interactPressed, grabPressed);

            joined.Clear();
            for (int i = 0; i < joinedPlayerIndices.Count; i++) joined.Add(joinedPlayerIndices[i]);
            if (readyPlayers.RemoveWhere(index => !joined.Contains(index)) > 0)
                Changed?.Invoke();

            bool allReady = ComputeAllReady();
            AllReady = allReady;

            pendingCount = pendingOffBoardCount;
            if (allowDepartWithPendingItems && pendingCount > previousPendingCount)
                allowDepartWithPendingItems = false;
            if (!allReady || pendingCount == 0)
                allowDepartWithPendingItems = false;

            UpdateHints(allReady, dt);
            EnsureDecisionShown(allReady);

            bool canDepart = allReady && (pendingCount == 0 || allowDepartWithPendingItems) && !IsDecisionOpen;
            HoldTimer = canDepart ? HoldTimer + dt : 0f;

            if (HoldTimer >= departHoldSeconds)
            {
                Bubble.Hide();
                Departing = true;
                Changed?.Invoke();
                DepartRequested?.Invoke();
            }

            previousPendingCount = pendingCount;
        }

        private bool ComputeAllReady()
        {
            if (joined.Count == 0) return false;
            for (int i = 0; i < joined.Count; i++)
                if (!readyPlayers.Contains(joined[i])) return false;
            return true;
        }

        private void UpdateDecisionInput(float dt, bool interactPressed, bool grabPressed)
        {
            if (!IsDecisionOpen) return;

            if (decisionInputBlockTimer > 0f)
            {
                decisionInputBlockTimer -= dt;
                return;
            }

            if (!interactPressed && !grabPressed) return;

            if (grabPressed && decisionPlayerIndex.HasValue)
            {
                readyPlayers.Remove(decisionPlayerIndex.Value);
                allowDepartWithPendingItems = false;
            }
            else if (interactPressed)
            {
                allowDepartWithPendingItems = true;
            }

            IsDecisionOpen = false;
            decisionPlayerIndex = null;
            Bubble.Hide();
            Changed?.Invoke();
        }

        private void UpdateHints(bool allReady, float dt)
        {
            if (IsDecisionOpen) return;

            string key = !allReady && hubElapsed >= LeverHintDelaySeconds ? HintKeyLever : null;

            if (key != hintKey)
            {
                hintKey = key;
                hintVisibleTimer = 0f;
                if (key == null)
                {
                    Bubble.Hide();
                    return;
                }
                Bubble.ShowPassive("Hint", HintText);
                hintVisibleTimer = HintVisibleSeconds;
                return;
            }

            if (key == null || hintVisibleTimer <= 0f) return;

            hintVisibleTimer -= dt;
            if (hintVisibleTimer <= 0f)
                Bubble.Hide();
        }

        private void EnsureDecisionShown(bool allReady)
        {
            if (IsDecisionOpen || allowDepartWithPendingItems) return;
            if (!allReady || pendingCount <= 0) return;

            int player = lastReadyPlayerIndex ?? (joined.Count > 0 ? joined[joined.Count - 1] : -1);
            if (player >= 0) OpenDecision(player);
        }

        private void OpenDecision(int playerIndex)
        {
            IsDecisionOpen = true;
            decisionPlayerIndex = playerIndex;
            hintKey = null;
            hintVisibleTimer = 0f;
            decisionInputBlockTimer = DecisionInputBlockSeconds;
            Bubble.ShowDecision("Hint", DecisionText, "No", "Yes");
            Changed?.Invoke();
        }
    }
}
