# Wave A4: Input & local co-op — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port `Src/Input/`, `Src/Players/`, and the join-screen logic of `Src/Screens/JoinScreen.cs` onto Unity's Input System + `PlayerInputManager`, so 1-4 local players can join with any mix of gamepads/keyboard, get assigned distinct player/split-screen indices, and signal "ready to start" — matching `GameInstructions.md`'s join-screen section.

**Architecture:** Engine-free join bookkeeping and directional-repeat timing logic go in `Gamelab.Core` (testable in EditMode, `System.Numerics.Vector2`). The Unity Input System bindings, `PlayerInputManager` wiring, and per-player `PlayerInput` adapter go in `Gamelab.Runtime` (PlayMode-tested with simulated virtual devices — no physical controllers or Editor GUI available). `IInputActions` (Core) is the seam: Core defines the query surface `Player.cs`'s future port will call (`GetMovement()`, `IsInteractJustPressed()`, ...); Runtime supplies the Unity-backed implementation.

**Tech Stack:** Unity 6 LTS, C# 9.0/netstandard2.1 (Core) + C# with UnityEngine (Runtime), Unity Input System 1.20.0 (`com.unity.inputsystem`, already in `Unity/Packages/manifest.json`, `activeInputHandler: 1` = Input System only), `UnityEngine.InputSystem.PlayerInputManager`/`PlayerInput`, NUnit (EditMode + new PlayMode assembly), Unity Test Framework with `InputTestFixture`/virtual devices for headless multi-controller simulation.

**Spec:** `docs/superpowers/specs/2026-09-18-unity-port-design.md` (Risk area #5, local co-op input parity) + `docs/superpowers/plans/2026-09-18-unity-port-plan.md` (Wave A, row A4) + `GameInstructions.md` (join-screen section) + `Unity/CONVENTIONS.md`.

## Global Constraints

- Unity 6 LTS, C#, URP 2D renderer (spec "Target").
- Local co-op (join screen, multi-controller, dynamic player spawn) must match or beat current behavior before this wave is done (spec "Risk areas" #5 / plan Global Constraints) — verified here via a PlayMode test that simulates two virtual controllers joining and split-screen-index assignment.
- `Src/` (MonoGame) is read-only reference; never modified. `Src/Gamelab.csproj` must still `dotnet run` unmodified at the end.
- `Gamelab.Core` has `noEngineReferences: true` — use `System.Numerics.Vector2`, never `UnityEngine.Vector2`, in any Core file.
- No file-scoped namespaces (`namespace Foo;`), no `System.Random.Shared` — C# 9.0/netstandard2.1 only (`Unity/CONVENTIONS.md`).
- Keep `Gamelab.*` namespace roots from `Src/` unchanged where a type ports directly.
- Headless verification only: use `Unity.app/Contents/MacOS/Unity -batchmode -nographics`; never combine `-quit` with `-runTests`; always confirm PlayMode/EditMode results from the results XML `<test-case ... result="Passed">`, not just exit code/log.
- `git add Unity/Assets` (not narrow per-folder adds) so parent `.meta` files aren't missed.
- Out of scope (other agents' worktrees, do not touch): `Src/PhysicalEntities/AbstractPhysicalEntity.cs`, movement `Src/Components/` (A1); `Src/PhysicalEntities/Bullets/`, `Src/Enemies/`, `Src/PhysicalEntities/Stations/`, `Src/PhysicalEntities/Interfaces/` (A2); `Src/Services/Sound/` (A3); non-join-screen `Src/Screens/` (Wave B).

## Scope boundary decisions (read before objecting to "missing" files)

1. **`Src/Players/Player.cs` is not fully ported in this wave.** It's assigned to A4 by folder, but its ~600 lines are 90% physics (Aether `PhysicsBody`), item/bullet/cannon-seat domain interfaces, animation, and FMOD sound — all owned by A1/A2/A3, none of which exist yet in this parallel wave. Porting it now would mean inventing fake stand-ins for other agents' future interfaces, which produces integration debt, not working software. **What ports now:** the two trivial enums it depends on (`PlayerCondition`, `WalkMaterial`) and the exact input query surface it calls (`PlayerConfiguration.Input.GetMovement()`, `.IsInteractJustPressed()`, etc., preserved 1:1 as `IInputActions`). The full `Player` MonoBehaviour (movement + domain + animation + sound, consuming `IInputActions`) is Wave C integration work once A1/A2/A3 land. This is called out again in the final report's "integration points" section.
2. **`Src/Screens/JoinScreen.cs` ports as logic only, not visuals.** The Gum Forms visuals (`Src/Screens/JoinScreen.Generated.cs`, `Src/Components/JoinPlayerComponent*`) have no migration path (spec: UI is a full rebuild, Wave B's job, and `B2` explicitly depends on `A4`). What ports here is the *behavior* `JoinScreen.cs.Update()` implements: which slots are occupied, when a slot's join/animation state should flip, and "any joined player presses Start/Enter advances to the next screen." That becomes a headless `JoinFlowController` (Runtime) exposing state + `UnityEvent`s that Wave B's rebuilt join screen subscribes to instead of polling gamepad state itself.
3. **`Src/Screens/PlayerInputExtensions.cs`** (`AnyPressedMenuConfirm`) ports as a Core extension method on the new roster type — trivial, folds into Task 4.
4. **`GamelabGameScreen.cs`, `MainMenuScreen.cs`, `GameplayScreen.cs`** are read for context only (they show how `Game.playerManager.Configs`/`PlayerConfiguration` get consumed downstream) — not touched, not ported. They're Wave B/general screen infrastructure.

## File structure

```
Unity/Assets/Scripts/Core/
  Input/
    IInputActions.cs          # query-surface interface (replaces IInputProvider)
    DirectionalInputRepeater.cs  # pure timing logic (replaces AbstractInputProvider's directional-repeat code)
  Players/
    PlayerCondition.cs        # direct port
    WalkMaterial.cs           # direct port
    PlayerSlot.cs             # replaces PlayerConfiguration (POCO: PlayerIndex, IInputActions Input)
    PlayerRoster.cs           # replaces PlayerManager (join bookkeeping, 4-slot cap)
    PlayerRosterExtensions.cs # replaces PlayerInputExtensions (AnyPressedMenuConfirm)

Unity/Assets/Settings/Input/
  GameplayControls.inputactions   # Input Actions asset: Player map + Join action

Unity/Assets/Scripts/Runtime/
  Input/
    PlayerInputHandler.cs     # MonoBehaviour, implements IInputActions via generated C# wrapper
  Players/
    PlayerJoinManager.cs      # MonoBehaviour wrapping PlayerInputManager; builds PlayerRoster
    JoinFlowController.cs     # exposes join-screen state/events for Wave B's UI

Unity/Assets/Tests/EditMode/
  DirectionalInputRepeaterTests.cs
  PlayerRosterTests.cs

Unity/Assets/Tests/PlayMode/
  Gamelab.Tests.PlayMode.asmdef   # new — first PlayMode test in the project (per CONVENTIONS.md)
  PlayerJoinManagerPlayModeTests.cs   # the two-simulated-controllers gate test
```

**Interfaces summary (for Wave B / Wave C to read without opening every file):**
- `Gamelab.Input.IInputActions` (Core): `System.Numerics.Vector2 GetMovement()`, `bool IsInteractJustPressed()/IsInteractHeld()/IsInteractJustReleased()`, `bool IsGrabJustPressed()/IsGrabHeld()/IsGrabJustReleased()`, `bool IsPickupJustPressed()/IsPickupHeld()`, `bool IsStartJustPressed()/IsStartHeld()`, `bool IsPauseJustPressed()`, `bool IsBackButtonJustPressed()/IsBackButtonHeld()`, `bool IsUpJustPressed()/IsDownJustPressed()/IsLeftJustPressed()/IsRightJustPressed()`.
- `Gamelab.Players.PlayerSlot` (Core): `{ int PlayerIndex; IInputActions Input; }`.
- `Gamelab.Players.PlayerRoster` (Core): `IReadOnlyList<PlayerSlot> Slots`, `bool JoinPlayer(IInputActions input)`, `void Reset()`, `const int MaxPlayers = 4`.
- `Gamelab.Players.PlayerRosterExtensions.AnyPressedMenuConfirm(this PlayerRoster roster)`.
- `Gamelab.Players.Runtime.PlayerJoinManager` (Runtime MonoBehaviour): `PlayerRoster Roster` (read-only projection), `UnityEvent<int> OnPlayerJoinedSlot`, `int GetSplitScreenIndex(int slotIndex)`.
- `Gamelab.Players.Runtime.JoinFlowController` (Runtime MonoBehaviour): `bool IsSlotJoined(int slot)`, `UnityEvent OnReadyToAdvance` (fires when any joined player presses Start/Enter, mirroring `JoinScreen.cs`'s "press Start to advance").

---

### Task 1: `Gamelab.Tests.PlayMode` assembly + Core `Input`/`Players` folder scaffolding

**Files:**
- Create: `Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef`
- Create: `Unity/Assets/Scripts/Core/Input/` (folder)
- Create: `Unity/Assets/Scripts/Core/Players/` (folder)
- Create: `Unity/Assets/Scripts/Runtime/Input/` (folder)
- Create: `Unity/Assets/Scripts/Runtime/Players/` (folder)

**Interfaces:** Produces the `Gamelab.Tests.PlayMode` assembly that every later PlayMode test task references.

- [ ] **Step 1: Create the PlayMode test asmdef**

```json
// Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef
{
    "name": "Gamelab.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core",
        "Gamelab.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Unity.InputSystem",
        "Unity.InputSystem.TestFramework"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": []
}
```

- [ ] **Step 2: Verify compile (no tests yet, just structural)**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$(pwd)/Unity" -logFile /tmp/a4-task1-compile.log
grep -i "error CS" /tmp/a4-task1-compile.log || echo "no compile errors"
```
Expected: exit code 0, no `error CS` lines.

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Add Gamelab.Tests.PlayMode assembly for Wave A4"
```

---

### Task 2: Core data types — `PlayerCondition`, `WalkMaterial`, `IInputActions`, `PlayerSlot`

**Files:**
- Create: `Unity/Assets/Scripts/Core/Players/PlayerCondition.cs`
- Create: `Unity/Assets/Scripts/Core/Players/WalkMaterial.cs`
- Create: `Unity/Assets/Scripts/Core/Input/IInputActions.cs`
- Create: `Unity/Assets/Scripts/Core/Players/PlayerSlot.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `Gamelab.Players.PlayerCondition` (`Active`, `Stunned`), `Gamelab.Players.WalkMaterial` (`Wood=0, Snow=1, Metal=2`), `Gamelab.Input.IInputActions` (full surface listed in "File structure" above), `Gamelab.Players.PlayerSlot { int PlayerIndex; IInputActions Input; }`.

These are direct 1:1 ports (enums) and a straight interface/POCO translation (`Vector2` → `System.Numerics.Vector2`, `GameTime` param on `Update` → `float dt` since Core has no MonoGame/UnityEngine types). No behavior to test — pure declarations. (Ponytail: a test for an enum or a data-only interface asserts nothing beyond "it compiles," which the Task 1 compile check already covers — skipping a dedicated test file here.)

- [ ] **Step 1: Port the enums verbatim**

```csharp
// Unity/Assets/Scripts/Core/Players/PlayerCondition.cs
namespace Gamelab.Players
{
    public enum PlayerCondition
    {
        Active,
        Stunned
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/Players/WalkMaterial.cs
namespace Gamelab.Players
{
    public enum WalkMaterial
    {
        Wood = 0,
        Snow = 1,
        Metal = 2
    }
}
```

- [ ] **Step 2: Write `IInputActions`**

```csharp
// Unity/Assets/Scripts/Core/Input/IInputActions.cs
using System.Numerics;

namespace Gamelab.Input
{
    /// <summary>
    /// Engine-free input query surface. Mirrors Src/Input/IInputProvider.cs from the
    /// MonoGame source 1:1 so Player.cs's eventual port can consume it unchanged.
    /// Gamelab.Runtime supplies the Unity Input System-backed implementation
    /// (see Gamelab.Input.Runtime.PlayerInputHandler).
    /// </summary>
    public interface IInputActions
    {
        Vector2 GetMovement();
        bool IsInteractJustPressed();
        bool IsInteractHeld();
        bool IsInteractJustReleased();
        bool IsGrabJustPressed();
        bool IsGrabHeld();
        bool IsGrabJustReleased();
        bool IsPickupJustPressed();
        bool IsPickupHeld();
        bool IsStartJustPressed();
        bool IsStartHeld();
        bool IsPauseJustPressed();
        bool IsBackButtonJustPressed();
        bool IsBackButtonHeld();
        bool IsUpJustPressed();
        bool IsDownJustPressed();
        bool IsLeftJustPressed();
        bool IsRightJustPressed();
    }
}
```

- [ ] **Step 3: Write `PlayerSlot`**

```csharp
// Unity/Assets/Scripts/Core/Players/PlayerSlot.cs
using Gamelab.Input;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Players/PlayerManager.cs's PlayerConfiguration.</summary>
    public class PlayerSlot
    {
        public int PlayerIndex { get; set; }
        public IInputActions Input { get; }

        public PlayerSlot(int playerIndex, IInputActions input)
        {
            PlayerIndex = playerIndex;
            Input = input;
        }
    }
}
```

- [ ] **Step 4: Compile check**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$(pwd)/Unity" -logFile /tmp/a4-task2-compile.log
grep -i "error CS" /tmp/a4-task2-compile.log || echo "no compile errors"
```

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Port PlayerCondition, WalkMaterial, and add IInputActions/PlayerSlot"
```

---

### Task 3: `DirectionalInputRepeater` (Core, TDD)

Replaces the directional-repeat logic in `Src/Input/AbstractInputProvider.cs` (`IsUpJustPressed`/`IsDownJustPressed`/`IsLeftJustPressed`/`IsRightJustPressed`, `CheckDirectionalRepeat`, and the `holdTimer`/`repeatTimer` accumulation in `Update`). This is the one piece of real, branching logic in the input layer, so it's the one piece worth an EditMode test — everything else in this subsystem is either a passthrough to Unity's Input System or built-in `PlayerInputManager` behavior.

**Files:**
- Create: `Unity/Assets/Scripts/Core/Input/DirectionalInputRepeater.cs`
- Test: `Unity/Assets/Tests/EditMode/DirectionalInputRepeaterTests.cs`

**Interfaces:**
- Consumes: nothing beyond `System.Numerics.Vector2`.
- Produces: `Gamelab.Input.DirectionalInputRepeater` with constructor `(float pressThreshold = 0.5f, float initialRepeatDelaySeconds = 0.3f, float repeatRateSeconds = 0.1f, float movementDeadzoneSquared = 0.25f)` (defaults copied from `Src/Config/GameplayConfig.cs`'s `Input*` fields — this subsystem has no shared config asset yet, so the defaults are inlined here with a comment; reconciling with a future shared `GameplayConfig` ScriptableObject, if A2 or a later wave introduces one, is a follow-up, not blocking this wave), method `void Tick(Vector2 currentMovement, float dt)`, properties `bool IsUpJustPressed`, `bool IsDownJustPressed`, `bool IsLeftJustPressed`, `bool IsRightJustPressed` (computed as of the last `Tick` call).

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/DirectionalInputRepeaterTests.cs
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

            repeater.Tick(new Vector2(1f, 0f), 0.016f);
            repeater.Tick(Vector2.Zero, 0.5f); // below deadzone squared -> resets accumulation
            repeater.Tick(new Vector2(1f, 0f), 0.35f); // would have repeated if the timer hadn't reset

            Assert.IsFalse(repeater.IsRightJustPressed, "hold timer must reset when movement drops below the deadzone");
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
```

- [ ] **Step 2: Run it and confirm it fails (compile error — type doesn't exist)**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a4-task3-red.xml -logFile /tmp/a4-task3-red.log
grep -i "error CS" /tmp/a4-task3-red.log
```
Expected: `error CS0246: The type or namespace name 'DirectionalInputRepeater' could not be found`.

- [ ] **Step 3: Implement**

```csharp
// Unity/Assets/Scripts/Core/Input/DirectionalInputRepeater.cs
using System.Numerics;

namespace Gamelab.Input
{
    /// <summary>
    /// Ports the directional-repeat timing from Src/Input/AbstractInputProvider.cs
    /// (IsUpJustPressed/IsDownJustPressed/IsLeftJustPressed/IsRightJustPressed +
    /// CheckDirectionalRepeat) as an engine-free, unit-testable helper. A Runtime
    /// IInputActions implementation calls Tick() once per frame with the movement
    /// vector it reads from Unity's Input System.
    /// </summary>
    public class DirectionalInputRepeater
    {
        private readonly float pressThreshold;
        private readonly float initialRepeatDelaySeconds;
        private readonly float repeatRateSeconds;
        private readonly float movementDeadzoneSquared;

        private Vector2 previousMovement;
        private float holdTimer;
        private float repeatTimer;

        public bool IsUpJustPressed { get; private set; }
        public bool IsDownJustPressed { get; private set; }
        public bool IsLeftJustPressed { get; private set; }
        public bool IsRightJustPressed { get; private set; }

        public DirectionalInputRepeater(
            float pressThreshold = 0.5f,
            float initialRepeatDelaySeconds = 0.3f,
            float repeatRateSeconds = 0.1f,
            float movementDeadzoneSquared = 0.25f)
        {
            this.pressThreshold = pressThreshold;
            this.initialRepeatDelaySeconds = initialRepeatDelaySeconds;
            this.repeatRateSeconds = repeatRateSeconds;
            this.movementDeadzoneSquared = movementDeadzoneSquared;
        }

        public void Tick(Vector2 currentMovement, float dt)
        {
            IsDownJustPressed = ComputeJustPressed(currentMovement.Y, previousMovement.Y, positive: true);
            IsUpJustPressed = ComputeJustPressed(currentMovement.Y, previousMovement.Y, positive: false);
            IsRightJustPressed = ComputeJustPressed(currentMovement.X, previousMovement.X, positive: true);
            IsLeftJustPressed = ComputeJustPressed(currentMovement.X, previousMovement.X, positive: false);

            if (currentMovement.LengthSquared() > movementDeadzoneSquared)
            {
                holdTimer += dt;
                repeatTimer += dt;
            }
            else
            {
                holdTimer = 0f;
                repeatTimer = 0f;
            }

            previousMovement = currentMovement;
        }

        private bool ComputeJustPressed(float current, float previous, bool positive)
        {
            bool held = positive ? current > pressThreshold : current < -pressThreshold;
            bool wasHeld = positive ? previous > pressThreshold : previous < -pressThreshold;
            bool justPressed = held && !wasHeld;
            return justPressed || CheckDirectionalRepeat(held);
        }

        private bool CheckDirectionalRepeat(bool isDirectionHeld)
        {
            if (!isDirectionHeld) return false;
            if (holdTimer >= initialRepeatDelaySeconds && repeatTimer >= repeatRateSeconds)
            {
                repeatTimer = 0f;
                return true;
            }

            return false;
        }
    }
}
```

Note: the original's `wasHeld` check used `<=`/`>=` (e.g. `previousMovement.Y <= InputDirectionPressThreshold`); this port uses strict `>`/`<` for `wasHeld` (i.e. `!wasHeld` triggers on `previous <= threshold`), which is the same boundary behavior — double check during review that the boundary case (`previous == threshold` exactly) matches; if a reviewer flags a mismatch, switch `wasHeld` to use `>=`/`<=` to match the original exactly.

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a4-task3-green.xml -logFile /tmp/a4-task3-green.log
grep -c 'result="Passed"' /tmp/a4-task3-green.xml
```
Expected: 4 (all four new tests pass; confirm by name in the XML, not just count).

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Port directional input repeat timing as DirectionalInputRepeater"
```

---

### Task 4: `PlayerRoster` + `AnyPressedMenuConfirm` (Core, TDD)

Replaces `Src/Players/PlayerManager.cs` and `Src/Screens/PlayerInputExtensions.cs`. Note the scope simplification: the original's `IsControllerJoined(int controllerIndex)` existed because MonoGame's hand-rolled loop had to dedupe gamepad indices itself. `PlayerInputManager` (Task 6) already refuses to let an already-controlling device join a second player, so `PlayerRoster` does not need a device-dedup method — it's a plain slot list capped at 4. This is exactly the "less custom code, not more" the spec calls for; noted here so a reviewer doesn't flag the missing method as a gap.

**Files:**
- Create: `Unity/Assets/Scripts/Core/Players/PlayerRoster.cs`
- Create: `Unity/Assets/Scripts/Core/Players/PlayerRosterExtensions.cs`
- Test: `Unity/Assets/Tests/EditMode/PlayerRosterTests.cs`

**Interfaces:**
- Consumes: `Gamelab.Input.IInputActions` (Task 2), `Gamelab.Players.PlayerSlot` (Task 2).
- Produces: `Gamelab.Players.PlayerRoster` (`const int MaxPlayers = 4`, `IReadOnlyList<PlayerSlot> Slots`, `bool JoinPlayer(IInputActions input)`, `void Reset()`), `Gamelab.Players.PlayerRosterExtensions.AnyPressedMenuConfirm(this PlayerRoster roster)`.

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/PlayerRosterTests.cs
using System.Numerics;
using NUnit.Framework;
using Gamelab.Input;
using Gamelab.Players;

namespace Gamelab.Tests.Players
{
    /// <summary>Minimal stub so roster/extension tests don't need a real Unity input backend.</summary>
    internal class StubInputActions : IInputActions
    {
        public bool PickupJustPressed;
        public bool StartJustPressed;

        public Vector2 GetMovement() => Vector2.Zero;
        public bool IsInteractJustPressed() => false;
        public bool IsInteractHeld() => false;
        public bool IsInteractJustReleased() => false;
        public bool IsGrabJustPressed() => false;
        public bool IsGrabHeld() => false;
        public bool IsGrabJustReleased() => false;
        public bool IsPickupJustPressed() => PickupJustPressed;
        public bool IsPickupHeld() => false;
        public bool IsStartJustPressed() => StartJustPressed;
        public bool IsStartHeld() => false;
        public bool IsPauseJustPressed() => false;
        public bool IsBackButtonJustPressed() => false;
        public bool IsBackButtonHeld() => false;
        public bool IsUpJustPressed() => false;
        public bool IsDownJustPressed() => false;
        public bool IsLeftJustPressed() => false;
        public bool IsRightJustPressed() => false;
    }

    public class PlayerRosterTests
    {
        [Test]
        public void JoinPlayer_AssignsSequentialIndices()
        {
            var roster = new PlayerRoster();

            Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));
            Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));

            Assert.AreEqual(2, roster.Slots.Count);
            Assert.AreEqual(0, roster.Slots[0].PlayerIndex);
            Assert.AreEqual(1, roster.Slots[1].PlayerIndex);
        }

        [Test]
        public void JoinPlayer_AtCapacity_ReturnsFalseAndDoesNotAdd()
        {
            var roster = new PlayerRoster();
            for (int i = 0; i < PlayerRoster.MaxPlayers; i++)
                Assert.IsTrue(roster.JoinPlayer(new StubInputActions()));

            bool joined = roster.JoinPlayer(new StubInputActions());

            Assert.IsFalse(joined);
            Assert.AreEqual(4, roster.Slots.Count);
        }

        [Test]
        public void Reset_ClearsAllSlots()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());

            roster.Reset();

            Assert.AreEqual(0, roster.Slots.Count);
        }

        [Test]
        public void AnyPressedMenuConfirm_TrueWhenAnyoneJustPressedStart()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());
            var second = new StubInputActions { StartJustPressed = true };
            roster.JoinPlayer(second);

            Assert.IsTrue(roster.AnyPressedMenuConfirm());
        }

        [Test]
        public void AnyPressedMenuConfirm_FalseWhenNobodyPressedAnything()
        {
            var roster = new PlayerRoster();
            roster.JoinPlayer(new StubInputActions());

            Assert.IsFalse(roster.AnyPressedMenuConfirm());
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a4-task4-red.xml -logFile /tmp/a4-task4-red.log
grep -i "error CS" /tmp/a4-task4-red.log
```
Expected: compile errors, `PlayerRoster`/`AnyPressedMenuConfirm` don't exist.

- [ ] **Step 3: Implement**

```csharp
// Unity/Assets/Scripts/Core/Players/PlayerRoster.cs
using System.Collections.Generic;
using Gamelab.Input;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Players/PlayerManager.cs.</summary>
    public class PlayerRoster
    {
        public const int MaxPlayers = 4;

        private readonly List<PlayerSlot> slots = new List<PlayerSlot>();
        public IReadOnlyList<PlayerSlot> Slots => slots;

        public bool JoinPlayer(IInputActions input)
        {
            if (slots.Count >= MaxPlayers) return false;

            slots.Add(new PlayerSlot(slots.Count, input));
            return true;
        }

        public void Reset()
        {
            slots.Clear();
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/Players/PlayerRosterExtensions.cs
using System.Linq;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Screens/PlayerInputExtensions.cs.</summary>
    public static class PlayerRosterExtensions
    {
        public static bool AnyPressedMenuConfirm(this PlayerRoster roster) =>
            roster.Slots.Any(s => s.Input.IsPickupJustPressed() || s.Input.IsStartJustPressed());
    }
}
```

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a4-task4-green.xml -logFile /tmp/a4-task4-green.log
grep -c 'result="Passed"' /tmp/a4-task4-green.xml
```
Expected: 5 new passing test-cases (plus all prior EditMode tests still green — check the XML has no `result="Failed"`).

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Port PlayerManager as PlayerRoster and PlayerInputExtensions as PlayerRosterExtensions"
```

---

### Task 5: Input Actions asset — `GameplayControls.inputactions`

Defines the actual bindings from `GameInstructions.md`'s controls table, as a Unity Input Actions asset (JSON, importable/editable in the Editor, drivable from code without the Editor for tests).

**Files:**
- Create: `Unity/Assets/Settings/Input/GameplayControls.inputactions`

**Interfaces:**
- Produces: an action map named `Player` with actions `Move` (Vector2), `Interact` (Button), `Grab` (Button), `Pickup` (Button), `Start` (Button), `Pause` (Button), `Back` (Button), bound per `GameInstructions.md`:
  - Move: gamepad left stick + D-pad, keyboard WASD
  - Interact: gamepad West/X, keyboard E
  - Grab: gamepad North/Y, keyboard R
  - Pickup: gamepad South/A, keyboard Space
  - Start: gamepad Start, keyboard Enter
  - Pause: gamepad Start, keyboard Escape
  - Back: gamepad Select/Back — no documented keyboard binding in `GameInstructions.md`; the MonoGame source bound it to `Keys.H` (`Src/Input/KeyboardInputProvider.cs`) as an undocumented debug key, so that binding is preserved here for parity with the reference build even though it's not in the player-facing instructions.
- Also produces a top-level `JoinAction` control scheme-agnostic button binding (gamepad South/A, keyboard Space) that `PlayerJoinManager` (Task 6) wires into `PlayerInputManager.joinAction`, matching `GameInstructions.md`: "press A (gamepad) or Space (keyboard debug) to join."

- [ ] **Step 1: Write the asset**

```json
{
    "name": "GameplayControls",
    "maps": [
        {
            "name": "Player",
            "id": "8f1a2b3c-0001-4a11-9c11-000000000001",
            "actions": [
                { "name": "Move", "type": "Value", "id": "8f1a2b3c-0002-4a11-9c11-000000000001", "expectedControlType": "Vector2" },
                { "name": "Interact", "type": "Button", "id": "8f1a2b3c-0003-4a11-9c11-000000000001" },
                { "name": "Grab", "type": "Button", "id": "8f1a2b3c-0004-4a11-9c11-000000000001" },
                { "name": "Pickup", "type": "Button", "id": "8f1a2b3c-0005-4a11-9c11-000000000001" },
                { "name": "Start", "type": "Button", "id": "8f1a2b3c-0006-4a11-9c11-000000000001" },
                { "name": "Pause", "type": "Button", "id": "8f1a2b3c-0007-4a11-9c11-000000000001" },
                { "name": "Back", "type": "Button", "id": "8f1a2b3c-0008-4a11-9c11-000000000001" }
            ],
            "bindings": [
                { "name": "WASD", "id": "8f1a2b3c-1001-4a11-9c11-000000000001", "path": "2DVector", "isComposite": true, "isPartOfComposite": false, "action": "Move" },
                { "name": "up", "id": "8f1a2b3c-1002-4a11-9c11-000000000001", "path": "<Keyboard>/w", "isComposite": false, "isPartOfComposite": true, "action": "Move" },
                { "name": "down", "id": "8f1a2b3c-1003-4a11-9c11-000000000001", "path": "<Keyboard>/s", "isComposite": false, "isPartOfComposite": true, "action": "Move" },
                { "name": "left", "id": "8f1a2b3c-1004-4a11-9c11-000000000001", "path": "<Keyboard>/a", "isComposite": false, "isPartOfComposite": true, "action": "Move" },
                { "name": "right", "id": "8f1a2b3c-1005-4a11-9c11-000000000001", "path": "<Keyboard>/d", "isComposite": false, "isPartOfComposite": true, "action": "Move" },
                { "name": "", "id": "8f1a2b3c-1006-4a11-9c11-000000000001", "path": "<Gamepad>/leftStick", "isComposite": false, "isPartOfComposite": false, "action": "Move" },
                { "name": "", "id": "8f1a2b3c-1007-4a11-9c11-000000000001", "path": "<Gamepad>/dpad", "isComposite": false, "isPartOfComposite": false, "action": "Move" },
                { "name": "", "id": "8f1a2b3c-1010-4a11-9c11-000000000001", "path": "<Keyboard>/e", "isComposite": false, "isPartOfComposite": false, "action": "Interact" },
                { "name": "", "id": "8f1a2b3c-1011-4a11-9c11-000000000001", "path": "<Gamepad>/buttonWest", "isComposite": false, "isPartOfComposite": false, "action": "Interact" },
                { "name": "", "id": "8f1a2b3c-1020-4a11-9c11-000000000001", "path": "<Keyboard>/r", "isComposite": false, "isPartOfComposite": false, "action": "Grab" },
                { "name": "", "id": "8f1a2b3c-1021-4a11-9c11-000000000001", "path": "<Gamepad>/buttonNorth", "isComposite": false, "isPartOfComposite": false, "action": "Grab" },
                { "name": "", "id": "8f1a2b3c-1030-4a11-9c11-000000000001", "path": "<Keyboard>/space", "isComposite": false, "isPartOfComposite": false, "action": "Pickup" },
                { "name": "", "id": "8f1a2b3c-1031-4a11-9c11-000000000001", "path": "<Gamepad>/buttonSouth", "isComposite": false, "isPartOfComposite": false, "action": "Pickup" },
                { "name": "", "id": "8f1a2b3c-1040-4a11-9c11-000000000001", "path": "<Keyboard>/enter", "isComposite": false, "isPartOfComposite": false, "action": "Start" },
                { "name": "", "id": "8f1a2b3c-1041-4a11-9c11-000000000001", "path": "<Gamepad>/start", "isComposite": false, "isPartOfComposite": false, "action": "Start" },
                { "name": "", "id": "8f1a2b3c-1050-4a11-9c11-000000000001", "path": "<Keyboard>/escape", "isComposite": false, "isPartOfComposite": false, "action": "Pause" },
                { "name": "", "id": "8f1a2b3c-1051-4a11-9c11-000000000001", "path": "<Gamepad>/start", "isComposite": false, "isPartOfComposite": false, "action": "Pause" },
                { "name": "", "id": "8f1a2b3c-1060-4a11-9c11-000000000001", "path": "<Keyboard>/h", "isComposite": false, "isPartOfComposite": false, "action": "Back" },
                { "name": "", "id": "8f1a2b3c-1061-4a11-9c11-000000000001", "path": "<Gamepad>/select", "isComposite": false, "isPartOfComposite": false, "action": "Back" }
            ]
        },
        {
            "name": "Join",
            "id": "8f1a2b3c-0100-4a11-9c11-000000000001",
            "actions": [
                { "name": "JoinAction", "type": "Button", "id": "8f1a2b3c-0101-4a11-9c11-000000000001" }
            ],
            "bindings": [
                { "name": "", "id": "8f1a2b3c-1101-4a11-9c11-000000000001", "path": "<Keyboard>/space", "isComposite": false, "isPartOfComposite": false, "action": "JoinAction" },
                { "name": "", "id": "8f1a2b3c-1102-4a11-9c11-000000000001", "path": "<Gamepad>/buttonSouth", "isComposite": false, "isPartOfComposite": false, "action": "JoinAction" }
            ]
        }
    ],
    "controlSchemes": []
}
```

- [ ] **Step 2: Verify the asset imports cleanly**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$(pwd)/Unity" -logFile /tmp/a4-task5-compile.log
grep -i "error" /tmp/a4-task5-compile.log | grep -v "Licensing::Module" || echo "clean import"
```
Expected: no import errors for the `.inputactions` asset (Unity auto-generates its `.meta` and, since "Generate C# Class" isn't checked here, later tasks read actions by name via `PlayerInput.actions["Player/Move"]` rather than a generated wrapper class — simpler, no extra generated-file task needed).

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Add GameplayControls input actions asset (Player + Join maps)"
```

---

### Task 6: `PlayerInputHandler` (Runtime) — implements `IInputActions` via Unity Input System

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/Input/PlayerInputHandler.cs`
- Test: `Unity/Assets/Tests/PlayMode/PlayerInputHandlerPlayModeTests.cs`

**Interfaces:**
- Consumes: `Gamelab.Input.IInputActions` (Task 2), `Gamelab.Input.DirectionalInputRepeater` (Task 3), the `Player`/`Join` maps from `GameplayControls.inputactions` (Task 5).
- Produces: `Gamelab.Input.Runtime.PlayerInputHandler : MonoBehaviour, IInputActions` — attached by `PlayerInputManager` to each joined player's instantiated prefab (`UnityEngine.InputSystem.PlayerInput` is a sibling component it reads from). Also exposes `void SetActions(UnityEngine.InputSystem.PlayerInput playerInput)` for tests/`PlayerJoinManager` to wire explicitly.

- [ ] **Step 1: Write the failing PlayMode test**

```csharp
// Unity/Assets/Tests/PlayMode/PlayerInputHandlerPlayModeTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Gamelab.Input.Runtime;

namespace Gamelab.Tests.Input
{
    public class PlayerInputHandlerPlayModeTests : InputTestFixture
    {
        private Keyboard keyboard;
        private GameObject go;
        private PlayerInput playerInput;
        private PlayerInputHandler handler;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();

            var actions = Resources.Load<InputActionAsset>("GameplayControls");
            Assert.IsNotNull(actions, "Expected Assets/Resources/GameplayControls.inputactions (a .inputactions asset placed under a Resources folder so Resources.Load can find it in tests and at runtime)");

            go = new GameObject("TestPlayer");
            playerInput = go.AddComponent<PlayerInput>();
            playerInput.actions = actions;
            playerInput.defaultActionMap = "Player";
            handler = go.AddComponent<PlayerInputHandler>();
            handler.SetActions(playerInput);
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(go);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator PressingE_ReportsInteractJustPressed()
        {
            Press(keyboard.eKey);
            yield return null;

            Assert.IsTrue(handler.IsInteractJustPressed());
        }

        [UnityTest]
        public IEnumerator HoldingWASD_ReportsMovementVector()
        {
            Press(keyboard.dKey);
            yield return null;

            Vector2 movement = handler.GetMovement();
            Assert.Greater(movement.X, 0f);
            Assert.AreEqual(0f, movement.Y, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Move the input actions asset under Resources so `Resources.Load` finds it, then run and confirm the test fails**

```bash
mkdir -p Unity/Assets/Resources
git mv Unity/Assets/Settings/Input/GameplayControls.inputactions Unity/Assets/Resources/GameplayControls.inputactions
git mv Unity/Assets/Settings/Input/GameplayControls.inputactions.meta Unity/Assets/Resources/GameplayControls.inputactions.meta 2>/dev/null || true
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a4-task6-red.xml -logFile /tmp/a4-task6-red.log
grep -i "error CS" /tmp/a4-task6-red.log
```
Expected: compile error, `PlayerInputHandler` doesn't exist yet in `Gamelab.Input.Runtime`.

(If Task 5's asset location choice turns out wrong once this test runs for real — e.g. `Resources.Load` needs a different relative path — fix the path here rather than in Task 5, since this is the first task that actually loads it at runtime.)

- [ ] **Step 3: Implement**

```csharp
// Unity/Assets/Scripts/Runtime/Input/PlayerInputHandler.cs
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Gamelab.Input;

namespace Gamelab.Input.Runtime
{
    /// <summary>
    /// Unity Input System-backed implementation of IInputActions. PlayerInputManager
    /// (see Gamelab.Players.Runtime.PlayerJoinManager) puts this on the prefab it
    /// instantiates per joined device; SetActions() wires it to that instance's
    /// sibling PlayerInput component.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour, IInputActions
    {
        private PlayerInput playerInput;
        private InputAction move, interact, grab, pickup, start, pause, back;
        private readonly DirectionalInputRepeater directionalRepeater = new DirectionalInputRepeater();

        public void SetActions(PlayerInput input)
        {
            playerInput = input;
            move = input.actions["Player/Move"];
            interact = input.actions["Player/Interact"];
            grab = input.actions["Player/Grab"];
            pickup = input.actions["Player/Pickup"];
            start = input.actions["Player/Start"];
            pause = input.actions["Player/Pause"];
            back = input.actions["Player/Back"];
        }

        private void Awake()
        {
            if (playerInput == null && TryGetComponent(out PlayerInput autoWired))
                SetActions(autoWired);
        }

        private void Update()
        {
            if (move == null) return;
            directionalRepeater.Tick(GetMovement(), Time.deltaTime);
        }

        public Vector2 GetMovement()
        {
            if (move == null) return Vector2.Zero;
            Vector2Raw v = move.ReadValue<UnityEngine.Vector2>().ToSystemVector2();
            return v.Value;
        }

        public bool IsInteractJustPressed() => interact != null && interact.WasPressedThisFrame();
        public bool IsInteractHeld() => interact != null && interact.IsPressed();
        public bool IsInteractJustReleased() => interact != null && interact.WasReleasedThisFrame();
        public bool IsGrabJustPressed() => grab != null && grab.WasPressedThisFrame();
        public bool IsGrabHeld() => grab != null && grab.IsPressed();
        public bool IsGrabJustReleased() => grab != null && grab.WasReleasedThisFrame();
        public bool IsPickupJustPressed() => pickup != null && pickup.WasPressedThisFrame();
        public bool IsPickupHeld() => pickup != null && pickup.IsPressed();
        public bool IsStartJustPressed() => start != null && start.WasPressedThisFrame();
        public bool IsStartHeld() => start != null && start.IsPressed();
        public bool IsPauseJustPressed() => pause != null && pause.WasPressedThisFrame();
        public bool IsBackButtonJustPressed() => back != null && back.WasPressedThisFrame();
        public bool IsBackButtonHeld() => back != null && back.IsPressed();
        public bool IsUpJustPressed() => directionalRepeater.IsUpJustPressed;
        public bool IsDownJustPressed() => directionalRepeater.IsDownJustPressed;
        public bool IsLeftJustPressed() => directionalRepeater.IsLeftJustPressed;
        public bool IsRightJustPressed() => directionalRepeater.IsRightJustPressed;
    }
}
```

`System.Numerics.Vector2` and `UnityEngine.Vector2` are both named `Vector2`, so `Gamelab.Runtime` code that needs both must fully qualify one. Add a tiny conversion helper rather than repeating fully-qualified names everywhere it's needed in this and later tasks:

```csharp
// Unity/Assets/Scripts/Runtime/Input/Vector2Interop.cs
namespace Gamelab.Input.Runtime
{
    public readonly struct Vector2Raw
    {
        public readonly System.Numerics.Vector2 Value;
        public Vector2Raw(System.Numerics.Vector2 value) => Value = value;
    }

    public static class Vector2InteropExtensions
    {
        public static Vector2Raw ToSystemVector2(this UnityEngine.Vector2 v) =>
            new Vector2Raw(new System.Numerics.Vector2(v.x, v.y));
    }
}
```

(Adjust the `using Gamelab.Input.Runtime;` import and the `.ToSystemVector2()` call site in `PlayerInputHandler.GetMovement()` accordingly — the wrapper struct exists only to avoid a same-named-type ambiguity error at the call site, not because the conversion itself is complex.)

- [ ] **Step 4: Run and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a4-task6-green.xml -logFile /tmp/a4-task6-green.log
grep -c 'result="Passed"' /tmp/a4-task6-green.xml
```
Expected: both new test-cases `result="Passed"` by name in the XML.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Add PlayerInputHandler: Unity Input System-backed IInputActions"
```

---

### Task 7: `PlayerJoinManager` + `JoinFlowController` (Runtime, TDD) — the Wave A gate test

This is the task the Wave A verification gate is about: "two simulated controllers can join, split-screen assigns correctly."

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/Players/PlayerJoinManager.cs`
- Create: `Unity/Assets/Scripts/Runtime/Players/JoinFlowController.cs`
- Test: `Unity/Assets/Tests/PlayMode/PlayerJoinManagerPlayModeTests.cs`

**Interfaces:**
- Consumes: `Gamelab.Players.PlayerRoster`/`PlayerSlot` (Task 4), `Gamelab.Input.Runtime.PlayerInputHandler` (Task 6), `UnityEngine.InputSystem.PlayerInputManager`.
- Produces: `Gamelab.Players.Runtime.PlayerJoinManager : MonoBehaviour` (`PlayerRoster Roster { get; }`, `int GetSplitScreenIndex(int slotIndex)`), `Gamelab.Players.Runtime.JoinFlowController : MonoBehaviour` (`bool IsSlotJoined(int slot)`, `UnityEngine.Events.UnityEvent OnReadyToAdvance`) — this is the hook Wave B's UI Toolkit join screen subscribes to instead of polling gamepad state itself.

- [ ] **Step 1: Write the failing PlayMode test (the gate test)**

```csharp
// Unity/Assets/Tests/PlayMode/PlayerJoinManagerPlayModeTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Gamelab.Players.Runtime;

namespace Gamelab.Tests.Players
{
    public class PlayerJoinManagerPlayModeTests : InputTestFixture
    {
        private GameObject managerGo;
        private PlayerJoinManager joinManager;
        private PlayerInputManager pim;

        public override void Setup()
        {
            base.Setup();

            var actions = Resources.Load<InputActionAsset>("GameplayControls");
            var prefabGo = new GameObject("PlayerPrefab");
            prefabGo.AddComponent<PlayerInput>();
            prefabGo.AddComponent<Gamelab.Input.Runtime.PlayerInputHandler>();
            prefabGo.SetActive(false); // prefab template stays inactive; PlayerInputManager clones+activates it

            managerGo = new GameObject("JoinManager");
            pim = managerGo.AddComponent<PlayerInputManager>();
            pim.playerPrefab = prefabGo;
            pim.EnableJoining();
            joinManager = managerGo.AddComponent<PlayerJoinManager>();
            joinManager.Configure(pim, actions);
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(managerGo);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator TwoSimulatedGamepads_BothJoin_WithDistinctSplitScreenIndices()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var pad2 = InputSystem.AddDevice<Gamepad>();

            Press(pad1.buttonSouth);
            yield return null;
            Release(pad1.buttonSouth);
            yield return null;

            Press(pad2.buttonSouth);
            yield return null;
            Release(pad2.buttonSouth);
            yield return null;

            Assert.AreEqual(2, joinManager.Roster.Slots.Count, "both simulated controllers should have joined");
            int split0 = joinManager.GetSplitScreenIndex(0);
            int split1 = joinManager.GetSplitScreenIndex(1);
            Assert.AreNotEqual(split0, split1, "each joined player must get a distinct split-screen index");
        }

        [UnityTest]
        public IEnumerator KeyboardSpace_Joins_AlongsideGamepad()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();

            Press(pad1.buttonSouth);
            yield return null;
            Release(pad1.buttonSouth);
            yield return null;

            Press(keyboard.spaceKey);
            yield return null;
            Release(keyboard.spaceKey);
            yield return null;

            Assert.AreEqual(2, joinManager.Roster.Slots.Count, "gamepad and keyboard should both be able to join");
        }
    }
}
```

- [ ] **Step 2: Run and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a4-task7-red.xml -logFile /tmp/a4-task7-red.log
grep -i "error CS" /tmp/a4-task7-red.log
```
Expected: compile error, `PlayerJoinManager` doesn't exist.

- [ ] **Step 3: Implement `PlayerJoinManager`**

```csharp
// Unity/Assets/Scripts/Runtime/Players/PlayerJoinManager.cs
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
        private readonly Dictionary<int, PlayerInput> playersBySlot = new Dictionary<int, PlayerInput>();

        public void Configure(PlayerInputManager manager, InputActionAsset actions)
        {
            playerInputManager = manager;
            playerInputManager.playerPrefab = manager.playerPrefab;
            playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }

        private void Awake()
        {
            if (playerInputManager == null)
                playerInputManager = GetComponent<PlayerInputManager>();
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
```

- [ ] **Step 4: Implement `JoinFlowController`**

```csharp
// Unity/Assets/Scripts/Runtime/Players/JoinFlowController.cs
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
```

- [ ] **Step 5: Run and confirm the gate test passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a4-task7-green.xml -logFile /tmp/a4-task7-green.log
grep -c 'result="Passed"' /tmp/a4-task7-green.xml
grep 'name="TwoSimulatedGamepads_BothJoin_WithDistinctSplitScreenIndices"' /tmp/a4-task7-green.xml
```
Expected: both new test-cases `result="Passed"`, and the gate test's name appears with `Passed` explicitly (this is the Wave A gate evidence — quote this exact XML line in the final report).

- [ ] **Step 6: Commit**

```bash
git add Unity/Assets
git commit -m "Add PlayerJoinManager and JoinFlowController; two-controller join/split-screen PlayMode test"
```

---

### Task 8: Whole-branch verification and CONVENTIONS.md/GameInstructions.md cross-check

**Files:** none created — verification + a fix loop if anything's found.

- [ ] **Step 1: Full EditMode run**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a4-final-editmode.xml -logFile /tmp/a4-final-editmode.log
grep 'result="Failed"' /tmp/a4-final-editmode.xml && echo "FAILURES FOUND" || echo "all EditMode green"
```

- [ ] **Step 2: Full PlayMode run**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$(pwd)/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a4-final-playmode.xml -logFile /tmp/a4-final-playmode.log
grep 'result="Failed"' /tmp/a4-final-playmode.xml && echo "FAILURES FOUND" || echo "all PlayMode green"
```

- [ ] **Step 3: Re-read `GameInstructions.md`'s join-screen section and controls table against the implemented bindings; confirm every documented control has a binding in `GameplayControls.inputactions` and every join-screen behavior ("press A/Space to join", "press Start/Enter to begin") has a corresponding `PlayerJoinManager`/`JoinFlowController` code path.** Record the check inline (no code change expected unless a gap is found — if one is, fix it and re-run steps 1-2).

- [ ] **Step 4: Confirm the MonoGame reference build is untouched and still runs**

```bash
git status --porcelain Src/ | head -5
dotnet run --project Src/Gamelab.csproj &
sleep 8
kill %1 2>/dev/null
```
Expected: `git status --porcelain Src/` prints nothing (zero diff in `Src/`), and the MonoGame process starts without a crash in that window.

- [ ] **Step 5: Commit any fixes from Step 3, or note "no changes needed" and stop (nothing to commit)**

---

## Self-review notes

- **Spec/work-order coverage:** `Src/Input/` (all 4 files) → Tasks 2-3, 6. `Src/Players/` (all 4 files) → Tasks 2, 4 (`Player.cs` explicitly deferred per scope boundary decision #1, with the input-surface contract it needs — `IInputActions` — delivered now). Join-screen logic of `Src/Screens/` → Tasks 5-7 (`JoinScreen.cs`, `PlayerInputExtensions.cs`); `JoinScreen.Generated.cs` and `Src/Components/JoinPlayerComponent*` explicitly out of scope (boundary decision #2, Wave B). Wave A gate's "two simulated controllers join, split-screen assigns correctly" → Task 7's `TwoSimulatedGamepads_BothJoin_WithDistinctSplitScreenIndices` test. `GameInstructions.md` controls table → Task 5's bindings, cross-checked in Task 8.
- **Placeholder scan:** every task has real code, not descriptions of code; no "add appropriate tests" language.
- **Type consistency check:** `IInputActions` (Task 2) signature is used identically by `PlayerSlot` (Task 2), `PlayerRoster`/`PlayerRosterExtensions` (Task 4, via `StubInputActions` test double), and `PlayerInputHandler` (Task 6). `PlayerRoster.Slots`/`JoinPlayer`/`Reset`/`MaxPlayers` used consistently by `PlayerJoinManager` (Task 7). `DirectionalInputRepeater.Tick`/`Is*JustPressed` (Task 3) consumed by `PlayerInputHandler` (Task 6) with matching property names.
