using System.Numerics;
using NUnit.Framework;
using Gamelab.Input;

namespace Gamelab.Tests.Input
{
    public class DirectionalInputRepeaterTests
    {
        [Test]
        public void Tick_MovementCrossesThreshold_FiresJustPressedOnce()
        {
            var repeater = new DirectionalInputRepeater();

            repeater.Tick(Vector2.Zero, 0.016f);
            Assert.IsFalse(repeater.IsRightJustPressed);

            repeater.Tick(new Vector2(1f, 0f), 0.016f);
            Assert.IsTrue(repeater.IsRightJustPressed);

            repeater.Tick(new Vector2(1f, 0f), 0.016f);
            Assert.IsFalse(repeater.IsRightJustPressed, "should not re-fire every frame while held below the repeat delay");
        }

        [Test]
        public void Tick_HeldPastInitialDelay_RepeatsAtRepeatRate()
        {
            var repeater = new DirectionalInputRepeater(
                pressThreshold: 0.5f, initialRepeatDelaySeconds: 0.3f, repeatRateSeconds: 0.1f, movementDeadzoneSquared: 0.25f);

            repeater.Tick(new Vector2(0f, -1f), 0f); // just-pressed frame (dt=0 keeps hold timer at 0 here)
            Assert.IsTrue(repeater.IsUpJustPressed);

            // Advance past the 0.3s initial delay without crossing a repeat boundary yet.
            repeater.Tick(new Vector2(0f, -1f), 0.25f);
            Assert.IsFalse(repeater.IsUpJustPressed);

            repeater.Tick(new Vector2(0f, -1f), 0.06f); // holdTimer=0.31 >= 0.3, repeatTimer=0.31 >= 0.1 -> repeat fires
            Assert.IsTrue(repeater.IsUpJustPressed);
        }

        [Test]
        public void Tick_MovementBelowDeadzone_ResetsHoldTimer()
        {
            var repeater = new DirectionalInputRepeater();

            repeater.Tick(new Vector2(1f, 0f), 0.016f);   // press
            repeater.Tick(new Vector2(1f, 0f), 0.35f);    // held long enough that leftover timer state would look "due" for a repeat
            repeater.Tick(Vector2.Zero, 0.5f);            // release -> resets hold/repeat timers
            repeater.Tick(new Vector2(1f, 0f), 0.016f);   // re-press: justPressed fires here (expected, not what's under test)
            repeater.Tick(new Vector2(1f, 0f), 0.05f);    // only ~0.066s since re-press, well under the 0.3s initial delay

            Assert.IsFalse(repeater.IsRightJustPressed, "hold timer must reset when movement drops below the deadzone, not carry over into an early repeat");
        }

        [Test]
        public void Tick_OppositeDirectionsAreIndependent()
        {
            var repeater = new DirectionalInputRepeater();

            repeater.Tick(new Vector2(-1f, 0f), 0.016f);
            Assert.IsTrue(repeater.IsLeftJustPressed);
            Assert.IsFalse(repeater.IsRightJustPressed);
        }
    }
}
