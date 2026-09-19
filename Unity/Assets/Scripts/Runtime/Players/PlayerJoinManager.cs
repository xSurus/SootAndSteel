using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Gamelab.Players;
using Gamelab.Input.Runtime;

namespace Gamelab.Players.Runtime
{
    /// <summary>
    /// Wraps UnityEngine.InputSystem.PlayerInputManager to replace the hand-rolled
    /// gamepad-polling join loop in Src/Screens/JoinScreen.cs. PlayerInputManager
    /// already handles: detecting a new device pressing the join button, refusing to
    /// let a device that's already controlling a player join a second one, and
    /// assigning a split-screen index per joined player — none of that needs
    /// reimplementing here (spec: expect less code than the original, not more).
    /// </summary>
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerJoinManager : MonoBehaviour
    {
        public PlayerRoster Roster { get; } = new PlayerRoster();

        private PlayerInputManager playerInputManager;
        private InputActionAsset joinActions;
        private readonly Dictionary<int, PlayerInput> playersBySlot = new Dictionary<int, PlayerInput>();

        // Deviation from brief: PlayerInputManager.notificationBehavior defaults to
        // SendMessages, under which the onPlayerJoined C# event never fires (Unity
        // only invokes it when notificationBehavior == InvokeCSharpEvents). Configure()
        // and Awake() both route through EnsureWired() so this is set exactly once
        // regardless of call order, and so a join is handled exactly once per physical
        // join (Ruling 2: unsubscribe-before-subscribe guards double registration when
        // both Awake() and Configure() run, e.g. AddComponent<PlayerJoinManager>()
        // firing Awake() followed by an explicit Configure() call in tests).
        public void Configure(PlayerInputManager manager, InputActionAsset actions)
        {
            if (playerInputManager != null && playerInputManager != manager)
                playerInputManager.onPlayerJoined -= HandlePlayerJoined;
            playerInputManager = manager;
            // Separate clones for the join action and the prefab: joined players disable
            // bindings of other devices on their asset, which would block later joins,
            // and player 0 would otherwise hold the caller's shared asset instance.
            joinActions = Instantiate(actions);

            // Deviation from brief: the prefab's PlayerInput needs an actions asset
            // assigned before PlayerInputManager clones it, otherwise the cloned
            // instance's PlayerInput.actions is null and PlayerInputHandler.SetActions()
            // (which does input.actions["Player/Move"]) throws a NullReferenceException
            // on the first join. Unity's own PlayerInput duplicates this asset per
            // instance automatically once it sees the same asset shared across two
            // active PlayerInputs (see PlayerInput.InitializeActions/
            // CopyActionAssetAndApplyBindingOverrides), so assigning the same asset
            // instance here for every player is safe and is how per-player action
            // isolation is supposed to work.
            PlayerInput prefabInput = manager.playerPrefab.GetComponent<PlayerInput>();
            if (prefabInput != null)
                prefabInput.actions = Instantiate(actions);

            EnsureWired();
        }

        private void Awake()
        {
            if (playerInputManager == null)
                playerInputManager = GetComponent<PlayerInputManager>();
            EnsureWired();
        }

        private void EnsureWired()
        {
            playerInputManager.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            // maxPlayerCount is read-only at runtime; over-capacity joins are rejected in HandlePlayerJoined.
            // Join only on A / Space (Join/JoinAction), per GameInstructions.md.
            var joinAction = joinActions != null ? joinActions.FindAction("Join/JoinAction") : null;
            if (joinAction != null)
            {
                playerInputManager.joinAction = new InputActionProperty(joinAction);
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered;
            }
            playerInputManager.onPlayerJoined -= HandlePlayerJoined;
            playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }

        private void OnDestroy()
        {
            if (playerInputManager != null)
                playerInputManager.onPlayerJoined -= HandlePlayerJoined;
        }

        private void HandlePlayerJoined(PlayerInput input)
        {
            var handler = input.GetComponent<PlayerInputHandler>();
            handler.SetActions(input);

            if (!Roster.JoinPlayer(handler))
            {
                Destroy(input.gameObject); // at MaxPlayers capacity — reject the join
                return;
            }

            int slotIndex = Roster.Slots.Count - 1;
            playersBySlot[slotIndex] = input;
        }

        public int GetSplitScreenIndex(int slotIndex) =>
            playersBySlot.TryGetValue(slotIndex, out PlayerInput input) ? input.splitScreenIndex : -1;

        public void ResetJoins()
        {
            foreach (PlayerInput input in playersBySlot.Values)
                if (input != null) Destroy(input.gameObject);
            playersBySlot.Clear();
            Roster.Reset();
        }
    }
}
