using UnityEngine;
using UnityEngine.Events;

namespace Gamelab.Players.Runtime
{
    /// <summary>
    /// Headless port of Src/Screens/JoinScreen.cs's behavior (which slots are
    /// occupied, "any joined player pressed Start/Enter advances the screen").
    /// Wave B's UI Toolkit join screen binds its visuals to IsSlotJoined and
    /// subscribes to OnReadyToAdvance instead of polling gamepad state itself
    /// (this replaces the Gum-specific SetPlayerFigureState/SetJoinButtonState
    /// visual code, which has no migration path — see the plan's scope boundary
    /// decision #2).
    /// </summary>
    [RequireComponent(typeof(PlayerJoinManager))]
    public class JoinFlowController : MonoBehaviour
    {
        public UnityEvent OnReadyToAdvance = new UnityEvent();

        private PlayerJoinManager joinManager;

        private void Awake()
        {
            joinManager = GetComponent<PlayerJoinManager>();
        }

        public bool IsSlotJoined(int slot) => joinManager.Roster.Slots.Count > slot;

        private void Update()
        {
            if (joinManager.Roster.Slots.Count == 0) return;

            foreach (PlayerSlot slot in joinManager.Roster.Slots)
            {
                if (slot.Input.IsStartJustPressed())
                {
                    OnReadyToAdvance.Invoke();
                    return;
                }
            }
        }
    }
}
