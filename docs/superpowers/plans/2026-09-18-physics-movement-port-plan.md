# Physics & movement port (Wave A1) implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port the physics/movement substrate (`Src/PhysicalEntities/AbstractPhysicalEntity.cs`) to a Rigidbody2D/Collider2D-based Unity equivalent, retune Unity's global 2D physics/time settings to match the original Aether.Physics2D world, and produce test evidence (plus a demo scene) that a physics body moves and collides with parity to the MonoGame reference.

**Architecture:** `Gamelab.Runtime` gets one new `PhysicalEntity` MonoBehaviour that wraps a sibling `Rigidbody2D`/`CircleCollider2D` the way the original wrapped an Aether `Body` — this is the only file explicitly named in the A1 work order. Global `ProjectSettings` (gravity, fixed timestep, max allowed timestep) are retuned so every future Rigidbody2D in the project behaves like the original's Aether `World`. No engine-free `Gamelab.Core` code is needed: the original `AbstractPhysicalEntity` has no pure logic beyond a pixel↔meter conversion that Unity doesn't need (see "Design decisions" below) and a highlight mechanic that belongs to another agent's interfaces. All tests are `Gamelab.Tests.PlayMode` (new asmdef, this is the first Wave A subsystem needing PlayMode), since verifying physics behavior requires stepping the simulation.

**Tech Stack:** Unity 6 LTS, C# 9.0/netstandard2.1, Unity 2D physics (Box2D-backed), Unity Test Framework (NUnit, PlayMode).

**Spec:** `docs/superpowers/specs/2026-09-18-unity-port-design.md`, work order row A1 in `docs/superpowers/plans/2026-09-18-unity-port-plan.md` ("Phase 1-3: Subsystem waves" → Wave A table), and `Unity/CONVENTIONS.md`.

## Global Constraints

- Unity 6 LTS, C#, URP 2D renderer (per spec "Target")
- Desktop build targets only: Windows, macOS, Linux (per spec "Target")
- No online multiplayer, no gameplay rebalancing (per spec "Non-goals")
- The MonoGame build (`Src/Gamelab.csproj`) stays runnable and unmodified throughout (per spec "Testing & verification strategy"); this plan never touches anything under `Src/`
- `Gamelab.Core` (`noEngineReferences: true`) holds only engine-agnostic logic; `Gamelab.Runtime` holds MonoBehaviours/UnityEngine-coupled code (`Unity/CONVENTIONS.md`)
- C# 9.0/netstandard2.1: no file-scoped namespaces, no `Random.Shared`, check every ported file for other C#10+/.NET6+ syntax (`Unity/CONVENTIONS.md`)
- Verify via headless Unity CLI only (no Editor GUI available); never combine `-quit` with `-runTests` (`Unity/CONVENTIONS.md`)
- `git add Unity/Assets` (or `git status` afterward) so folder-level `.meta` files aren't missed (`Unity/CONVENTIONS.md`)

## Scope & boundary decisions

- **Source scope, as assigned:** `Src/PhysicalEntities/AbstractPhysicalEntity.cs`, plus "movement-related `Src/Components/`". `Src/Components/` was inspected in full (48 files) and contains exclusively Gum/Myra UI overlay code (`HubOverlay`, `PauseOverlay`, `ToolTip`, `Slider`, etc.) — zero physics/movement content (`grep` for `Physics|Velocity|RigidBody|Force` across the folder returns nothing). **Boundary decision: nothing in `Src/Components/` belongs to A1.** It's all UI, which the plan's own Wave B table assigns to B2.
- **Not touching `Src/PhysicalEntities/AbstractGrabbable.cs`.** Same top-level folder as my one named file, but it implements `IGrabbable` (from the excluded `Src/PhysicalEntities/Interfaces/`), depends on `ISoundService` (A3's domain) and station grab/weld-joint mechanics — it's interaction/station logic, not physics substrate. **Boundary decision: out of scope for A1**, likely belongs with whichever agent ends up owning `Src/PhysicalEntities/Stations/` (A2, per the Wave A table) or a Wave B agent; flagging as a possible ownership gap in the final report.
- **Not touching `Src/PhysicalEntities/Interfaces/*`** (explicitly excluded — A2's domain), including `IPhysicalEntity` and `IHighlightable`, which the original `AbstractPhysicalEntity` implements. The ported `PhysicalEntity` below reproduces their method/property shape (`Position`, a `Rigidbody2D`-backed body) so A2 can retrofit `: IPhysicalEntity` later without changing this file's public surface. Highlight state (`CanHighlight`/`IsHighlighted`/`OnHighlight(Player)`/`OnHighlightRemoved(Player)`) is dropped entirely from this port: it takes a `Player` parameter (A4's domain, doesn't exist in Unity yet) and is pure interaction-eligibility state, not physics. Flagging as an integration point for A2/A4.
- **Not touching `Src/Enemies/Movement/EnemyMovementController.cs`**, even though its folder name is literally "Movement" — it lives under `Src/Enemies/`, explicitly excluded (A2's domain per the Wave A table).
- **Not porting `Src/Utils/WorldUtility.cs`'s pixel↔meter conversion.** See "Design decisions" below — Unity doesn't need it.
- **Read-only reference for physics tuning, not ported wholesale:** `Src/Config/GameplayConfig.cs` (density/damping/velocity constants) and `Src/Players/Player.cs` (concrete circle-body construction pattern, `World.CreateCircle(...)` + `LinearDamping` + `FixedRotation`). Both belong to other agents (Config isn't owned by anyone in Wave A; `Src/Players/` is A4's). I use their numeric constants as realistic, in-repo values to retune Unity's settings and to drive the parity test/demo scene, but I don't create a ported config class — that's a shared-config decision for whoever integrates all four agents' constant needs (flagging in report).

## Design decisions

- **No pixel↔meter runtime conversion.** The original needs `WorldUtility.ToMeters`/`ToPixels` because Aether's `Body.Position` is in meters while XNA's `SpriteBatch` draws in raw pixels — two different coordinate spaces bridged by `PixelsPerMeter = 100`. Unity has no separate pixel-space renderer: `Transform`/`Rigidbody2D.position` is always in world units, and a sprite's on-screen size is governed by its **Pixels Per Unit** import setting (`Unity/CONVENTIONS.md`'s sprite import rule). Setting a sprite's PPU to 100 makes 1 Unity world unit visually equal to what was 1 meter (100px) in the original — achieving the same visual scale without any runtime conversion call. **Integration note for whoever imports the real player/enemy sprites (B1/UI wave):** use PPU = 100 for anything that shares scale with `PlayerRadiusPixels`/physics bodies, to keep the original's 100px-per-meter visual proportions.
- **No `Draw(SpriteBatch)` method.** XNA's per-frame imperative draw call has no Unity equivalent; `SpriteRenderer` handles rendering declaratively. Dropped from the port.
- **`PhysicalEntity` auto-discovers its `Rigidbody2D`** via `[RequireComponent]` + `GetComponent` in `Awake()`, instead of the original's externally-assigned `PhysicsBody` setter — because in Unity the body is always a sibling component by construction, not something a factory injects after the fact.

---

### Task 1: Retune global physics/time settings for Aether parity

**Files:**
- Modify: `Unity/ProjectSettings/Physics2DSettings.asset`
- Modify: `Unity/ProjectSettings/TimeManager.asset`
- Create: `Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef`
- Test: `Unity/Assets/Tests/PlayMode/PhysicsWorldSettingsTests.cs`

**Interfaces:**
- Produces: the `Gamelab.Tests.PlayMode` assembly (references `Gamelab.Core`, `Gamelab.Runtime`), which every later task's tests are added to.

Source values being matched, from `Src/Config/GameplayConfig.cs`: `FixedTimeStep = 1f / 60f`, `MaxAccumulatedDeltaSeconds = 0.25f`. Gravity is `Vector2.Zero` per `Src/Map/Train/State/GameplayContext.cs:15` (`new World(Vector2.Zero)` — this is a top-down game, no gravity). Velocity/position solver iterations are left at Unity's current defaults (8/3): this matches the standard Box2D reference defaults that both Aether.Physics2D and Unity's 2D physics are built on, so no change is needed there.

- [ ] **Step 1: Write the failing test**

Create the asmdef first so the test can compile:

```json
// Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef
{
    "name": "Gamelab.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core",
        "Gamelab.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
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

```csharp
// Unity/Assets/Tests/PlayMode/PhysicsWorldSettingsTests.cs
using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.PhysicalEntities
{
    /// <summary>
    /// Regression guard for the ProjectSettings retuning in this plan's Task 1: if these ever drift back
    /// to Unity's defaults, every dynamic PhysicalEntity's behavior silently stops matching the Aether
    /// reference (gravity would pull bodies down; the fixed step would run at 50Hz instead of 60Hz).
    /// </summary>
    public class PhysicsWorldSettingsTests
    {
        [Test]
        public void Physics2D_Gravity_IsZero_MatchingAetherWorld()
        {
            Assert.AreEqual(Vector2.zero, Physics2D.gravity);
        }

        [Test]
        public void FixedTimestep_Matches_OriginalGameplayConfig()
        {
            Assert.AreEqual(1f / 60f, Time.fixedDeltaTime, 0.0001f);
        }

        [Test]
        public void MaximumAllowedTimestep_Matches_OriginalMaxAccumulatedDelta()
        {
            Assert.AreEqual(0.25f, Time.maximumDeltaTime, 0.0001f);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task1.xml -logFile /tmp/physics-task1.log
```

Expected: the results XML contains `Physics2D_Gravity_IsZero_MatchingAetherWorld` and `FixedTimestep_Matches_OriginalGameplayConfig` with `result="Failed"` (Unity's defaults are gravity `(0,-9.81)` and fixed timestep `0.02`). `MaximumAllowedTimestep_Matches_OriginalMaxAccumulatedDelta` may already pass or fail depending on Unity's default (`0.3333`) — expect `Failed` too.

- [ ] **Step 3: Edit the ProjectSettings**

In `Unity/ProjectSettings/Physics2DSettings.asset`, change:
```yaml
  m_Gravity: {x: 0, y: -9.81}
```
to:
```yaml
  m_Gravity: {x: 0, y: 0}
```

In `Unity/ProjectSettings/TimeManager.asset`, change:
```yaml
  Fixed Timestep: 0.02
  Maximum Allowed Timestep: 0.33333334
```
to:
```yaml
  Fixed Timestep: 0.016666668
  Maximum Allowed Timestep: 0.25
```

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task1-pass.xml -logFile /tmp/physics-task1-pass.log
```

Expected: all three test-case entries in the XML have `result="Passed"`.

- [ ] **Step 5: Commit**

```bash
git add Unity/ProjectSettings/Physics2DSettings.asset Unity/ProjectSettings/TimeManager.asset Unity/Assets
git commit -m "Retune Physics2D gravity/timestep to match Aether.Physics2D world"
```

---

### Task 2: Port `PhysicalEntity` (Rigidbody2D/Collider2D wrapper)

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/PhysicalEntity.cs`
- Test: `Unity/Assets/Tests/PlayMode/PhysicalEntityConfigurationTests.cs`

**Interfaces:**
- Consumes: nothing from earlier tasks (only the ProjectSettings/asmdef from Task 1).
- Produces: `Gamelab.PhysicalEntities.PhysicalEntity` (Unity `MonoBehaviour`, `Gamelab.Runtime` assembly) with:
  - `Rigidbody2D Body { get; }`
  - `Vector2 Position { get; set; }` (pass-through to `Body.position`)
  - `CircleCollider2D ConfigureAsDynamicCircle(float radiusMeters, float density, float linearDamping, bool fixedRotation)`
  
  Later tasks (and A2/A4 when they build on this) depend on this exact class name, namespace, and these exact members.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/PlayMode/PhysicalEntityConfigurationTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicalEntityConfigurationTests
    {
        private GameObject entityObject;

        [TearDown]
        public void TearDown()
        {
            if (entityObject != null) Object.Destroy(entityObject);
        }

        [UnityTest]
        public IEnumerator ConfigureAsDynamicCircle_SetsBodyAndColliderToMatchPlayerParity()
        {
            entityObject = new GameObject("TestEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();

            // Player-parity constants from Src/Config/GameplayConfig.cs: PlayerRadiusPixels=10,
            // PixelsPerMeter=100 -> 0.1m radius; PlayerDensity=3; PlayerLinearDamping=20; the player body
            // is FixedRotation=true (Src/Players/Player.cs:76-79).
            CircleCollider2D collider = entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            yield return null;

            Assert.AreEqual(RigidbodyType2D.Dynamic, entity.Body.bodyType);
            Assert.AreEqual(20f, entity.Body.linearDamping, 0.0001f);
            Assert.IsTrue(entity.Body.freezeRotation);
            Assert.AreEqual(0f, entity.Body.gravityScale, 0.0001f);
            Assert.AreEqual(0.1f, collider.radius, 0.0001f);
            Assert.AreEqual(3f, collider.density, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Position_GetSet_PassesThroughToRigidbody2D()
        {
            entityObject = new GameObject("TestEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            yield return null;

            entity.Position = new Vector2(1.5f, -2f);
            yield return null;

            Assert.AreEqual(new Vector2(1.5f, -2f), entity.Body.position);
            Assert.AreEqual(entity.Body.position, entity.Position);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task2.xml -logFile /tmp/physics-task2.log
```

Expected: compile error (`PhysicalEntity` doesn't exist yet) — grep the log for `error CS` to confirm, and confirm no `<test-case>` for these two tests appears with `result="Passed"`.

- [ ] **Step 3: Implement `PhysicalEntity`**

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/PhysicalEntity.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities
{
    /// <summary>
    /// Ported from Src/PhysicalEntities/AbstractPhysicalEntity.cs. Wraps a sibling Rigidbody2D the way the
    /// original wrapped an Aether.Physics2D Body.
    ///
    /// Deliberately drops two things from the original (see docs/superpowers/plans/
    /// 2026-09-18-physics-movement-port-plan.md, "Design decisions"):
    /// - The pixel&lt;-&gt;meter Position conversion the original needed to bridge Aether (meters) with
    ///   XNA's pixel-space SpriteBatch. Unity's Rigidbody2D.position is already in world units; a
    ///   sprite's Pixels-Per-Unit import setting is what keeps world units visually matching the
    ///   original's pixel scale, so no runtime conversion is needed here.
    /// - Draw(SpriteBatch) and the CanHighlight/IsHighlighted/OnHighlight/OnHighlightRemoved highlight
    ///   state. Draw has no Unity equivalent (SpriteRenderer replaces it). Highlighting depends on
    ///   IHighlightable (Src/PhysicalEntities/Interfaces/, owned by agent A2) and a Player parameter
    ///   (Src/Players/, owned by agent A4) — retrofitting IPhysicalEntity/IHighlightable onto this class
    ///   is an integration task for whichever of those agents lands second.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicalEntity : MonoBehaviour
    {
        private Rigidbody2D body;

        public Rigidbody2D Body => body;

        public Vector2 Position
        {
            get => Body.position;
            set => Body.position = value;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Configures this entity's body/collider to match the original's circle-body construction
        /// pattern: World.CreateCircle(radius, density, position, BodyType.Dynamic) + LinearDamping +
        /// FixedRotation (see e.g. Src/Players/Player.cs:76-79). Callers pick radius/density/damping per
        /// entity type, same as the original did per subclass. Gravity scale is forced to 0 to match the
        /// original's zero-gravity World (Src/Map/Train/State/GameplayContext.cs:15) even if a future
        /// change to ProjectSettings/Physics2DSettings.asset reintroduces global gravity.
        /// </summary>
        public CircleCollider2D ConfigureAsDynamicCircle(float radiusMeters, float density, float linearDamping,
            bool fixedRotation)
        {
            if (body == null) body = GetComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Dynamic;
            body.useAutoMass = true;
            body.linearDamping = linearDamping;
            body.freezeRotation = fixedRotation;
            body.gravityScale = 0f;

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null) collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = radiusMeters;
            collider.density = density;

            return collider;
        }
    }
}
```

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task2-pass.xml -logFile /tmp/physics-task2-pass.log
```

Expected: both test-case entries `result="Passed"` in the XML, plus Task 1's three tests still passing (regression check).

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Port AbstractPhysicalEntity as a Rigidbody2D-backed PhysicalEntity"
```

---

### Task 3: Movement/collision physics parity tests

**Files:**
- Test: `Unity/Assets/Tests/PlayMode/PhysicsMovementParityTests.cs`

**Interfaces:**
- Consumes: `Gamelab.PhysicalEntities.PhysicalEntity.ConfigureAsDynamicCircle` and `.Body` from Task 2.
- Produces: nothing new consumed by later tasks — this is the Wave A gate's automated parity evidence ("a physics body moving/colliding with parity to the MonoGame reference").

Both Aether.Physics2D (the original) and Unity's 2D physics are Box2D-family engines, and both integrate linear damping the same documented way each fixed step: `velocity += dt * invMass * force; velocity *= 1f / (1f + dt * damping);` (spec: "Both Aether and Unity 2D physics are Box2D-based; forces/restitution/friction need re-tuning, not redesign"). So tuning the same density/damping/force constants from `Src/Config/GameplayConfig.cs` into a `Rigidbody2D` reproduces the original's trajectory shape by engine equivalence, not by replaying Aether itself. `ExpectedVelocityAfterSteps` below recomputes that same recurrence independently in plain C# as the oracle this test checks Unity's actual simulated `Rigidbody2D` against — the parity claim being tested is "Unity's physics does what the shared Box2D damping model documents," which is exactly what "retuned constants, not redesign" means.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/PlayMode/PhysicsMovementParityTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicsMovementParityTests
    {
        private const float FixedDt = 1f / 60f;
        private const float Radius = 0.1f;
        private const float Density = 3f;
        private const float LinearDamping = 20f;
        private const float ForceMagnitude = 10f;

        private GameObject entityObject;
        private GameObject wallObject;

        [TearDown]
        public void TearDown()
        {
            if (entityObject != null) Object.Destroy(entityObject);
            if (wallObject != null) Object.Destroy(wallObject);
        }

        private static float Mass(float radius, float density) => density * Mathf.PI * radius * radius;

        private static Vector2 ExpectedVelocityAfterSteps(Vector2 force, float mass, float damping, int steps,
            float dt)
        {
            Vector2 velocity = Vector2.zero;
            Vector2 acceleration = force / mass;
            for (int i = 0; i < steps; i++)
            {
                velocity += acceleration * dt;
                velocity *= 1f / (1f + dt * damping);
            }

            return velocity;
        }

        [UnityTest]
        public IEnumerator DynamicCircle_UnderConstantForce_MatchesBox2DDampingRecurrence()
        {
            entityObject = new GameObject("MovingEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(Radius, Density, LinearDamping, true);
            entity.Position = Vector2.zero;

            Vector2 force = new Vector2(ForceMagnitude, 0f);
            const int steps = 30;

            for (int i = 0; i < steps; i++)
            {
                entity.Body.AddForce(force);
                yield return new WaitForFixedUpdate();
            }

            Vector2 expected = ExpectedVelocityAfterSteps(force, Mass(Radius, Density), LinearDamping, steps,
                FixedDt);
            Vector2 actual = entity.Body.linearVelocity;

            Assert.AreEqual(expected.x, actual.x, expected.x * 0.1f + 0.01f,
                $"expected ~{expected}, got {actual} after {steps} fixed steps");
            Assert.AreEqual(expected.y, actual.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator DynamicCircle_PushedIntoStaticWall_RestsAtWallInsteadOfTunneling()
        {
            entityObject = new GameObject("MovingEntity");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(Radius, Density, LinearDamping, true);
            entity.Position = new Vector2(-1f, 0f);

            wallObject = new GameObject("Wall");
            wallObject.transform.position = Vector3.zero;
            BoxCollider2D wallCollider = wallObject.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(0.2f, 2f);
            Rigidbody2D wallBody = wallObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;

            Vector2 force = new Vector2(ForceMagnitude, 0f);
            for (int i = 0; i < 120; i++)
            {
                entity.Body.AddForce(force);
                yield return new WaitForFixedUpdate();
            }

            // The wall's left face is at x = -0.1 (half-width 0.1); a body of radius 0.1 resting against
            // it settles around x ~= -0.2, and must never pass x = -0.1 (that would mean it tunneled
            // through the wall).
            Assert.Less(entity.Position.x, -0.1f,
                $"entity tunneled through the wall, ended up at {entity.Position}");
            Assert.Greater(entity.Position.x, -0.95f,
                $"entity should have moved substantially toward the wall, only reached {entity.Position}");
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task3.xml -logFile /tmp/physics-task3.log
```

Expected: compile succeeds (everything these tests use already exists from Task 2), but review the XML — if either test unexpectedly already passes, treat that as a signal to double check the tolerance/setup is actually exercising real physics before moving on, since a passing "failing test" step means the test isn't testing anything. In the normal case here both should already pass on first run since Task 2 didn't need further implementation — if so, skip step 3 and go straight to confirming pass in step 4 (this is a test-only task; there's no new production code to write).

- [ ] **Step 3: n/a — no production code for this task**

This task only adds tests against the `PhysicalEntity` built in Task 2; there is nothing to implement.

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task3-pass.xml -logFile /tmp/physics-task3-pass.log
```

Expected: both new test-case entries `result="Passed"`, plus all of Task 1 and Task 2's tests still passing. If the damping test fails outside tolerance, don't loosen the tolerance to make it pass — that means the recurrence model or the assumed Unity damping formula is wrong, which is a real parity bug; re-derive `ExpectedVelocityAfterSteps` against Unity's actual documented Rigidbody2D damping behavior first.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Add physics movement/collision parity tests for PhysicalEntity"
```

---

### Task 4: Demo scene for the visual side-by-side check

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/PushTowardWallDemo.cs`
- Create: `Unity/Assets/Editor/Gamelab.Editor.asmdef`
- Create: `Unity/Assets/Editor/PhysicsParityDemoSceneBuilder.cs`
- Create (generated, not hand-authored): `Unity/Assets/Scenes/PhysicsParityDemo.unity`
- Modify: `Unity/ProjectSettings/EditorBuildSettings.asset` (scene gets registered so it can be loaded by path in tests/builds)
- Test: `Unity/Assets/Tests/PlayMode/PhysicsParityDemoSceneTests.cs`

**Interfaces:**
- Consumes: `PhysicalEntity.ConfigureAsDynamicCircle`/`.Body` (Task 2).
- Produces: the `Assets/Scenes/PhysicsParityDemo.unity` scene, the deliverable for the Wave A gate's "side-by-side visual check" (see note below on why this plan can't complete that check itself).

**Why generate the scene from code instead of hand-authoring `.unity` YAML:** there's no Editor GUI available to author or validate a scene interactively, and hand-writing `.unity` YAML (transform hierarchies, component `fileID`s, GUIDs) has no compile-time safety net — mistakes surface only when something tries to load the scene. Building the scene through a small Editor script and letting `EditorSceneManager.SaveScene` serialize it reuses Unity's own, guaranteed-well-formed serializer, and can be re-run any time via `-executeMethod`. This mirrors the "use the platform feature, don't hand-roll the format" rule that applies everywhere else in this codebase.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/PushTowardWallDemo.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities
{
    /// <summary>
    /// Demo-only driver for Assets/Scenes/PhysicsParityDemo.unity: pushes its PhysicalEntity toward the
    /// wall at a constant force, matching PhysicsMovementParityTests' force so pressing Play visually
    /// reproduces what that test asserts numerically.
    /// </summary>
    [RequireComponent(typeof(PhysicalEntity))]
    public class PushTowardWallDemo : MonoBehaviour
    {
        [SerializeField] private Vector2 force = new Vector2(10f, 0f);
        private PhysicalEntity entity;

        private void Awake()
        {
            entity = GetComponent<PhysicalEntity>();
        }

        private void FixedUpdate()
        {
            entity.Body.AddForce(force);
        }
    }
}
```

```json
// Unity/Assets/Editor/Gamelab.Editor.asmdef
{
    "name": "Gamelab.Editor",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core",
        "Gamelab.Runtime"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": []
}
```

```csharp
// Unity/Assets/Editor/PhysicsParityDemoSceneBuilder.cs
using System.Collections.Generic;
using Gamelab.PhysicalEntities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gamelab.Editor
{
    /// <summary>
    /// Regenerates Assets/Scenes/PhysicsParityDemo.unity: a Player-parity dynamic circle body pushed into
    /// a static wall, for a human to open in the Editor and visually compare against the MonoGame
    /// reference build (Wave A verification gate). Run headless via:
    ///   Unity -batchmode -quit -projectPath Unity -executeMethod Gamelab.Editor.PhysicsParityDemoSceneBuilder.Build
    /// </summary>
    public static class PhysicsParityDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PhysicsParityDemo.unity";

        [MenuItem("Gamelab/Physics/Rebuild Parity Demo Scene")]
        public static void Build()
        {
            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";

            GameObject entityObject = new GameObject("PlayerParityBody");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            entity.Position = new Vector2(-2f, 0f);
            entityObject.AddComponent<PushTowardWallDemo>();

            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.position = Vector3.zero;
            BoxCollider2D wallCollider = wallObject.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(0.2f, 2f);
            Rigidbody2D wallBody = wallObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;

            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings();
        }

        private static void RegisterInBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in existing)
            {
                if (s.path == ScenePath) return;
            }

            List<EditorBuildSettingsScene> updated = new List<EditorBuildSettingsScene>(existing)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = updated.ToArray();
        }
    }
}
```

```csharp
// Unity/Assets/Tests/PlayMode/PhysicsParityDemoSceneTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicsParityDemoSceneTests
    {
        [UnityTest]
        public IEnumerator PhysicsParityDemoScene_BodyMovesTowardWallAndComesToRest()
        {
            yield return SceneManager.LoadSceneAsync("PhysicsParityDemo", LoadSceneMode.Additive);

            PhysicalEntity entity = Object.FindFirstObjectByType<PhysicalEntity>();
            Assert.IsNotNull(entity, "PhysicsParityDemo.unity should contain a PhysicalEntity");

            float startX = entity.Position.x;

            for (int i = 0; i < 90; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.Greater(entity.Position.x, startX, "the body should have moved toward the wall");
            Assert.Less(entity.Position.x, -0.05f, "the body should have stopped at the wall, not passed through it");

            yield return SceneManager.UnloadSceneAsync("PhysicsParityDemo");
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task4.xml -logFile /tmp/physics-task4.log
```

Expected: the test fails (scene doesn't exist yet — `SceneManager.LoadSceneAsync` will error/the coroutine won't find a `PhysicalEntity`). Confirm via the XML/log, not just the exit code (per `Unity/CONVENTIONS.md`).

- [ ] **Step 3: Generate the scene**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath "Unity" \
  -executeMethod Gamelab.Editor.PhysicsParityDemoSceneBuilder.Build \
  -logFile /tmp/physics-task4-build.log
```

Check exit code 0 and `grep -i "error"` on the log is empty. Confirm `Unity/Assets/Scenes/PhysicsParityDemo.unity` and its `.meta` file now exist, and that `Unity/ProjectSettings/EditorBuildSettings.asset` now lists it.

- [ ] **Step 4: Run it and confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/physics-task4-pass.xml -logFile /tmp/physics-task4-pass.log
```

Expected: this test passes, and every test from Tasks 1-3 still passes (full regression check — this is the last task, so this run doubles as the whole subsystem's final automated verification).

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets Unity/ProjectSettings/EditorBuildSettings.asset
git commit -m "Add physics parity demo scene and scene-load smoke test"
```

**Note on the "side-by-side visual check" gate item:** this plan's author (agent A1) has no Unity Editor GUI access, only headless CLI (`Unity/CONVENTIONS.md`). Task 4 produces a real, loadable scene and strong automated (non-visual) evidence that its contents move and collide correctly, but the literal "open both builds side by side and look" comparison against the MonoGame reference needs a human (or an agent with GUI/screenshot tooling) to open `Assets/Scenes/PhysicsParityDemo.unity` in the Editor and press Play next to `dotnet run --project Src/Gamelab.csproj`. Flagging this explicitly in the final report rather than claiming the gate item is fully closed.

---

## Wave A1 verification gate

- [x] `Gamelab.Tests.PlayMode` passes in full (all four tasks' tests, run together in one final pass) — re-verified fresh after the final-review fix commit (297aa1b): 9/9 passed.
- [x] `Unity/Assets/Scenes/PhysicsParityDemo.unity` exists, is registered in Build Settings, and its scene-load test passes — `PhysicsParityDemoScene_BodyMovesTowardWallAndComesToRest` passed in the same run.
- [x] MonoGame reference build still runs unmodified: `dotnet run --project Src/Gamelab.csproj` — re-verified: builds with 0 errors, runs, loads content, reaches "Game initialized".
- [x] No file under `Src/` was modified (`git status` / `git diff` against the branch's base commit shows changes only under `Unity/` and `docs/superpowers/plans/`) — re-verified: `git diff --stat 6c13b36..297aa1b -- Src/` is empty.
- [ ] Human visual side-by-side check against the MonoGame reference — **still not completed** (no Editor GUI/screenshot tooling available to this agent either), same gap Task 4 originally flagged. Flagged for the coordinator. See `.superpowers/sdd/2026-09-18-physics-movement-port-plan/progress.md` for the full final-review/re-review record, including a note that the demo scene's rendered ball is small on screen (~3% of viewport height) — worth a quick look whenever this check happens.
