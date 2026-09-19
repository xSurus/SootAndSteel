# A2: Domain/catalog layer port implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port the domain/catalog layer (bullets, enemies, stations, physical-entity interfaces) from `Src/PhysicalEntities/Bullets/`, `Src/Enemies/`, `Src/PhysicalEntities/Stations/`, `Src/PhysicalEntities/Interfaces/` into Unity as `Gamelab.Core` data/interfaces plus `Gamelab.Runtime` `ScriptableObject` catalogs, proving the whole thing works with a catalog-driven bullet and enemy PlayMode spawn test.

**Architecture:** Split every ported type along the `Gamelab.Core` (engine-free, `System.Numerics.Vector2`, testable without a scene) / `Gamelab.Runtime` (`ScriptableObject`/`MonoBehaviour`, `UnityEngine` types) line from `Unity/CONVENTIONS.md`. The bullet casing+propellant+projectile combinator becomes composable `ScriptableObject` "component assets" referenced by a `BulletDefinitionAsset`. `EnemyCatalog`/`StationRegistry` become `ScriptableObject` catalog assets. Where this subsystem depends on sibling Wave A/B subsystems that haven't landed yet (A1 physics body type, A4 `Player` type, the unassigned `Src/Items/` inventory system), a minimal decoupling seam is defined instead of guessing their design, and the seam is called out for the coordinator.

**Tech Stack:** Unity 6 LTS, C# 9.0/netstandard2.1, Unity Test Framework (NUnit), `ScriptableObject`, `Rigidbody2D`.

**Spec:** `docs/superpowers/specs/2026-09-18-unity-port-design.md` and the A2 work order in `docs/superpowers/plans/2026-09-18-unity-port-plan.md` ("Phase 1-3: Subsystem waves" → Wave A → row A2).

## Global Constraints

- Unity 6 LTS, C#, URP 2D renderer; desktop targets only (spec "Target")
- No online multiplayer, no gameplay rebalancing (spec "Non-goals")
- `Src/Gamelab.csproj` (MonoGame reference build) must keep building unmodified; **do not touch any file under `Src/`** (read-only reference)
- Do not touch `Src/PhysicalEntities/AbstractPhysicalEntity.cs` or movement-related `Src/Components/` (agent A1's physics layer) or `Src/Services/Sound/` (A3) or `Src/Input/`, `Src/Players/`, `Src/Screens/` (A4)
- Follow `Unity/CONVENTIONS.md`: Core/Runtime assembly split, `Gamelab.*` namespace roots kept, sprite import rule (not touched by this plan, no sprites), catalogs as `ScriptableObject`, headless verification method (never combine `-quit` with `-runTests`)
- C# 9.0/netstandard2.1 language constraints from `CONVENTIONS.md`, **extended by this plan** (see Task 1) because the `Src/` code this subsystem ports from leans on considerably newer C# syntax than the two examples `CONVENTIONS.md` already lists
- Wave A verification gate for A2 (from the overall plan): own EditMode/PlayMode tests pass; a catalog-driven bullet (casing+propellant+projectile) and one enemy spawn correctly from data via a PlayMode smoke test; MonoGame reference build still runs unmodified

## Scope decisions (read before starting any task)

`Src/` in this area is far more cross-coupled than the work order's four folders suggest — nearly every file also touches `Src/Services/Sound`, `Src/Services/Vfx`, `Src/Items/` (inventory), `Src/Config/` (JSON `GameplayConfig`), and `Src/Players/Player`, none of which exist in Unity yet and none of which are this agent's job. Rather than guess those subsystems' future shape, this plan draws these lines:

1. **Physics body.** `Src/` types hold an Aether `Body PhysicsBody`. A1 (physics/movement) owns retuning movement, but plain `Rigidbody2D` creation/velocity assignment is a core Unity API, not something specific to A1's design — so this plan types physics bodies directly as `UnityEngine.Rigidbody2D` rather than inventing a placeholder abstraction or blocking on A1. If A1 lands a wrapper type instead of bare `Rigidbody2D`, that's a small follow-up adapter, not a redesign.
2. **Player.** Several interfaces (`IGrabbable`, `IHighlightable`, `IInteractable`, `IPickable`, `ICannonSeat`) take a `Player` parameter. A4 owns `Player` and hasn't landed. This plan defines an empty marker interface `IPlayerActor` in `Gamelab.Runtime` that these signatures use instead; A4's future `Player` MonoBehaviour implements it in one line. This *is* "the integration point" called out in the work order, applied by analogy to A4 the same way the work order calls it out for A1.
3. **Items/inventory.** `IItemProvider`/`IItemReceiver` (this agent's interfaces) and `AbstractStation`/bullet catalog code reference `Gamelab.Items.Item`/`BulletItem`, which live in `Src/Items/` — a folder not assigned to *any* Wave A agent. This plan defines a minimal Core `Item` (just an `Id`) sufficient to compile and test the provider/receiver contract and the bullet component-id recipe shape, and does **not** port `ItemRegistry`/`ItemDefinition`/shop pricing/texture drawing (real inventory system, unbuilt, not this agent's job). Flag this gap to the coordinator: nobody owns `Src/Items/` yet.
4. **Sound/VFX/animation.** Every concrete enemy/station in `Src/` calls `ISoundService`/`IVfxService`/`IAnimationService` (A3 + unassigned Vfx/Animation services) directly in constructors and update loops. This plan ports the **data and pure logic** (health, damage, targeting math, movement tuning) and leaves sound/VFX call sites out entirely rather than stubbing them with fake services — they get wired in once A3 lands. This is why `Enemy` (horse+rider state machine, heavy sound/animation coupling) is *not* ported behaviorally; `DummyEnemy` (a flat-colored box, zero sound/animation/VFX coupling) is the vertical-slice enemy instead. `Enemy`'s pure-data pieces (stats, ammo definition, targeting math) are still ported since other things need them.
5. **Config.** `Src/Config/GameplayConfig.cs` (JSON-loaded balance numbers) isn't this agent's job either. Where a ported type needs a default numeric value (bullet speed, enemy health, etc.), this plan copies today's literal default from `Src/Config/GameplayConfig.cs` directly into the `ScriptableObject`'s Inspector default, rather than inventing a config-loading system.
6. **Bullet "combine" crafting logic.** `Items/Bullets/BulletItem.cs`'s multi-component combine/upgrade-badge logic exists to support the player's in-game crafting UI (Workbench). That UI doesn't exist yet (Wave B). This plan ports only the ordered-component-id recipe shape needed by the catalog (`BulletRecipe`), not the combine/badge logic.

These are deliberate scope cuts, not oversights — each is called out again in its task and in the final report.

## Additional C# 9.0/netstandard2.1 constraints found during this port

`Unity/CONVENTIONS.md` already bans file-scoped namespaces and `Random.Shared`, and says the list isn't exhaustive. Porting this subsystem hit several more `Src/` idioms that are C# 10-12/.NET 6+ only. Every task below already accounts for these, but they're listed centrally so a task reviewer can recognize a violation on sight:

- **No primary constructors on non-record classes/structs** (C# 12), e.g. `class EnemySlotManager()`, `class Counter(Vector2 position) : AbstractStation(...)`, `class EnemyMovementController(Body physicsBody, EnemyMovementProfile profile)`. Use an explicit constructor body instead.
- **No `record struct` / `readonly record struct`** (C# 10). Use a plain `readonly struct` with hand-written properties and constructor (value equality isn't needed by anything this plan ports; if it becomes needed later, hand-write `Equals`/`GetHashCode`).
- **No collection expressions** (`[1, 2, 3]`, `[]`) (C# 12). Use `new[] { ... }`, `new List<T> { ... }`, `new HashSet<T>()`.
- **No `ArgumentException.ThrowIfNullOrWhiteSpace` / `ArgumentNullException.ThrowIfNull`** (.NET 6/7 static helpers). Use `if (string.IsNullOrWhiteSpace(x)) throw new ArgumentException(...)` / `if (x == null) throw new ArgumentNullException(...)` instead.
- **Verify `System.Linq.Enumerable.MinBy`/`MaxBy` before using them** — added in .NET 6; not guaranteed present in Unity's netstandard2.1 BCL. Prefer a manual loop or `OrderBy(...).FirstOrDefault()` when in doubt (this plan doesn't need `MinBy` in what it ports, but a future breadth task might reach for it — don't).
- This plan does not use `Newtonsoft.Json` — no JSON path is used (catalogs are `ScriptableObject` assets per `CONVENTIONS.md`), so no new package dependency is introduced.

### Task 0: Record these findings in `Unity/CONVENTIONS.md`

**Files:**
- Modify: `Unity/CONVENTIONS.md`

- [ ] **Step 1: Append a new `## Additional C# 9.0 constraints (found porting Wave A2)` section**

Append the five bullet points above (primary constructors, record struct, collection expressions, `ArgumentException.ThrowIfNullOrWhiteSpace`/`ArgumentNullException.ThrowIfNull`, `MinBy`/`MaxBy`) verbatim as a new section at the end of the "C# language version constraints" area of the file, under a new `###` heading `Found by A2 (domain/catalog layer)`. This is an additive, low-conflict-risk edit — other Wave A agents' branches don't touch this section.

- [ ] **Step 2: Commit**

```bash
git add Unity/CONVENTIONS.md
git commit -m "Record additional C# 9.0 constraints found porting the domain/catalog layer"
```

---

## Milestone 1: Interfaces

### Task 1: Core (engine-free) interfaces

**Files:**
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IUpdatable.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/SpriteRect.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/ITooltipable.cs`
- Create: `Unity/Assets/Scripts/Core/Items/Item.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IItemProvider.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IItemReceiver.cs`
- Test: `Unity/Assets/Tests/EditMode/Interfaces/ItemProviderReceiverTests.cs`

**Interfaces:**
- Produces: `Gamelab.PhysicalEntities.Interfaces.IUpdatable`, `.ITooltipable`, `.SpriteRect`, `.IItemProvider`, `.IItemReceiver`, `.ConsumerTicket`, `.ProviderTicket`; `Gamelab.Items.Item`. Later Core/Runtime tasks (bullets, enemies, stations) consume these exact names.

`Src/PhysicalEntities/Interfaces/IUpdatable.cs` has zero engine coupling — port verbatim (braced namespace form):

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IUpdatable.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IUpdatable
    {
        void Update(float dt);
    }
}
```

`ITooltipable` originally used XNA's `Vector2`/`Rectangle`. `Vector2` becomes `System.Numerics.Vector2` per `CONVENTIONS.md`. `Rectangle` is an int source-rect for a sprite atlas — engine-agnostic in spirit but `UnityEngine.RectInt` would violate `noEngineReferences`, and there's no netstandard2.1 BCL equivalent worth reaching for (`System.Drawing.Rectangle` drags in Windows-only baggage and isn't idiomatic Unity). Define a 4-int Core value type instead:

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/SpriteRect.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    public readonly struct SpriteRect
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public SpriteRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/ITooltipable.cs
using System.Numerics;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface ITooltipable
    {
        Vector2 Position { get; }
        string GetTitle();
        string GetDescription();
        bool IsVisible { get; }
        string CategoryName => null;
        string FunctionalityName => null;
        SpriteRect? IconSourceRect => null;
        int? Cost => null;
    }
}
```

`Item` is a deliberately minimal stand-in for `Src/Items/Item.cs` — see "Scope decisions" #3. It carries only what `IItemProvider`/`IItemReceiver`/the bullet recipe shape need (an id); it does not port `ItemDefinition`/`ItemRegistry`/texture drawing:

```csharp
// Unity/Assets/Scripts/Core/Items/Item.cs
namespace Gamelab.Items
{
    // ponytail: minimal stand-in for Src/Items/Item.cs. The real inventory system
    // (ItemRegistry, pricing, textures) is unbuilt and unassigned in Wave A/B as of
    // this port. Extend this type (or replace it) when that system gets built.
    public class Item
    {
        public string Id { get; }

        public Item(string id)
        {
            Id = id;
        }
    }
}
```

`IItemProvider`/`IItemReceiver` port near-verbatim from `Src/PhysicalEntities/Interfaces/IItemProvider.cs`/`IItemReceiver.cs`, minus the `using Gamelab.Items;` becoming a same-solution reference (both now in Core), and with the primary-constructor classes (`ConsumerTicket(IItemReceiver consumer)`, `ProviderTicket(IItemProvider provider)`) rewritten for C# 9:

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IItemProvider.cs
using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IItemProvider
    {
        Item PeekNextItem();
        bool TryProvideItem(out Item item, IItemReceiver consumer = null);
        bool CanProvideItem(IItemReceiver consumer);

        void PingPullIntent(IItemReceiver consumer, float dt)
        {
        }
    }

    public class ConsumerTicket
    {
        public IItemReceiver Consumer { get; }
        public float TimeSinceLastPing { get; set; } = 0f;

        public ConsumerTicket(IItemReceiver consumer)
        {
            Consumer = consumer;
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Interfaces/IItemReceiver.cs
using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IItemReceiver
    {
        bool CanReceiveItem(Item item, IItemProvider source);
        void ReceiveItem(Item item, IItemProvider source);

        void PingPushIntent(IItemProvider source, float dt)
        {
        }
    }

    public class ProviderTicket
    {
        public IItemProvider Provider { get; }
        public float TimeSinceLastPing { get; set; } = 0f;

        public ProviderTicket(IItemProvider provider)
        {
            Provider = provider;
        }
    }
}
```

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/Interfaces/ItemProviderReceiverTests.cs
using NUnit.Framework;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.Tests.Interfaces
{
    public class ItemProviderReceiverTests
    {
        private class FakeReceiver : IItemReceiver
        {
            public bool CanReceiveItem(Item item, IItemProvider source) => true;
            public void ReceiveItem(Item item, IItemProvider source) { }
        }

        [Test]
        public void ConsumerTicket_StartsAtZeroPingTime()
        {
            var ticket = new ConsumerTicket(new FakeReceiver());

            Assert.AreEqual(0f, ticket.TimeSinceLastPing);
        }

        [Test]
        public void SpriteRect_StoresAllFields()
        {
            var rect = new SpriteRect(1, 2, 3, 4);

            Assert.AreEqual(1, rect.X);
            Assert.AreEqual(2, rect.Y);
            Assert.AreEqual(3, rect.Width);
            Assert.AreEqual(4, rect.Height);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails to compile** (types don't exist yet)

Use the headless compile-only check from `CONVENTIONS.md`:
```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "Unity" -logFile /tmp/a2-task1-precheck.log
grep -i "error CS" /tmp/a2-task1-precheck.log
```
Expected: `error CS` lines referencing the missing types.

- [ ] **Step 3: Create the six files above**

- [ ] **Step 4: Compile check, then run tests**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "Unity" -logFile /tmp/a2-task1-compile.log
grep -i "error CS" /tmp/a2-task1-compile.log   # expect empty

/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a2-task1-results.xml -logFile /tmp/a2-task1-tests.log
```
Confirm `/tmp/a2-task1-results.xml` contains `<test-case ... name="...ConsumerTicket_StartsAtZeroPingTime" ... result="Passed" />` and the `SpriteRect` test, not just exit code 0.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets
git commit -m "Port engine-free physical-entity interfaces to Gamelab.Core"
```

### Task 2: Runtime (engine-coupled) interfaces

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPlayerActor.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPhysicalEntity.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IGrabbable.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IHighlightable.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IInteractable.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPickable.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/ICannonSeat.cs`

**Interfaces:**
- Consumes: nothing from Task 1 directly (these don't need `Item`).
- Produces: `Gamelab.PhysicalEntities.Interfaces.IPlayerActor` (empty marker — **the A4 integration point**: A4's future `Player` MonoBehaviour must implement this), `.IPhysicalEntity`, `.IGrabbable`, `.IHighlightable`, `.IInteractable`, `.IPickable`, `.ICannonSeat`, `.SeatPosition`. `IDamageable` is intentionally *not* in this task — it depends on the bullet runtime type and is created in Task 5 alongside it.

`IPhysicalEntity` drops `Draw(SpriteBatch)` entirely (Unity renders via `SpriteRenderer`/URP, not a manual per-frame draw call) and types the physics body as `Rigidbody2D` per "Scope decisions" #1:

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPlayerActor.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    // Integration point for A4 (Src/Players/): A4's `Player` MonoBehaviour should
    // implement this marker interface so it satisfies IGrabbable/IHighlightable/
    // IInteractable/IPickable/ICannonSeat without this subsystem depending on A4's
    // concrete Player type.
    public interface IPlayerActor
    {
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPhysicalEntity.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IPhysicalEntity
    {
        Rigidbody2D PhysicsBody { get; }
        Vector2 Position { get; set; }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IGrabbable.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IGrabbable : IPhysicalEntity
    {
        bool OnGrab(IPlayerActor player, Vector2 grabPointWorldMeters);
        void OnRelease(IPlayerActor player);
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IHighlightable.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IHighlightable : IPhysicalEntity
    {
        bool IsHighlighted { get; }
        void OnHighlight(IPlayerActor player);
        void OnHighlightRemoved(IPlayerActor player);
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IInteractable.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IInteractable : IPhysicalEntity
    {
        void OnInteract(IPlayerActor interactingPlayer)
        {
        }

        void OnInteractHeld(IPlayerActor interactingPlayer, float dt)
        {
        }

        void OnInteractReleased(IPlayerActor interactingPlayer)
        {
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IPickable.cs
namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IPickable : IPhysicalEntity
    {
        void OnPickup(IPlayerActor interactingPlayer)
        {
        }

        void OnPickupHeld(IPlayerActor interactingPlayer, float dt)
        {
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/ICannonSeat.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public enum SeatPosition
    {
        Bottom,
        Top
    }

    public interface ICannonSeat
    {
        Rigidbody2D PhysicsBody { get; }
        SeatPosition SeatPosition { get; }
        Vector2 DrawOffset { get; }
        void OnRelease(IPlayerActor player);
        void OnInteract(IPlayerActor player);

        void OnPickup(IPlayerActor player)
        {
        }
    }
}
```

- [ ] **Step 1: Create the seven files above**

- [ ] **Step 2: Compile check**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "Unity" -logFile /tmp/a2-task2-compile.log
grep -i "error CS" /tmp/a2-task2-compile.log   # expect empty
```

There's no behavior to unit test here (pure interface declarations with no logic) — an EditMode test asserting an interface exists would be a placeholder test, so this task's verification is the compile check plus the fact that later tasks' implementers of these interfaces compile and are tested.

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Port engine-coupled physical-entity interfaces to Gamelab.Runtime"
```

---

## Milestone 2: Bullets vertical slice

### Task 3: Bullet Core data (stats, faction, component ids, recipe)

**Files:**
- Create: `Unity/Assets/Scripts/Core/Items/Bullets/EComponentType.cs`
- Create: `Unity/Assets/Scripts/Core/Items/Bullets/ComponentIds.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletFaction.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletStats.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletRecipe.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletTargetingMath.cs`
- Test: `Unity/Assets/Tests/EditMode/Bullets/BulletStatsTests.cs`
- Test: `Unity/Assets/Tests/EditMode/Bullets/BulletRecipeTests.cs`
- Test: `Unity/Assets/Tests/EditMode/Bullets/BulletTargetingMathTests.cs`

**Interfaces:**
- Produces: `Gamelab.Items.Bullets.EComponentType` (`Projectile`, `Casing`, `Propellant`, `Bullet`), `Gamelab.Items.Bullets.ComponentIds` (string constants, see below), `Gamelab.PhysicalEntities.Bullets.BulletFaction` (`Player`, `Enemy`), `.BulletStats` (struct with `Speed/Damage/Pierce/Size/Spread/Lifetime` floats), `.BulletRecipe` (ordered component-id list + combine), `.BulletTargetingMath.GetDistanceToLine(Vector2 a, Vector2 b, Vector2 p)`. Task 5 (bullet Runtime) and Task 6 (enemy Core) consume these exact names.

`EComponentType`/`ComponentIds` port verbatim from `Src/Items/Bullets/EComponentType.cs`/`ComponentIds.cs` (braced namespace, no other changes needed — already C# 9-safe):

```csharp
// Unity/Assets/Scripts/Core/Items/Bullets/EComponentType.cs
namespace Gamelab.Items.Bullets
{
    public enum EComponentType
    {
        Projectile,
        Casing,
        Propellant,
        Bullet
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/Items/Bullets/ComponentIds.cs
namespace Gamelab.Items.Bullets
{
    public static class ComponentIds
    {
        // Projectiles
        public const string BasicProjectile = "BasicProjectile";
        public const string FrangibleProjectile = "FrangibleProjectile";
        public const string PiercingProjectile = "PiercingProjectile";
        public const string MatryoshkaProjectile = "MatryoshkaProjectile";

        // Casings
        public const string BasicCasing = "BasicCasing";
        public const string ScatterCasing = "ScatterCasing";
        public const string BurstCasing = "BurstCasing";
        public const string EnemyCasing = "EnemyCasing";
        public const string RapidFireCasing = "RapidFireCasing";

        // Propellants
        public const string BasicPropellant = "BasicPropellant";
        public const string HomingPropellant = "HomingPropellant";
        public const string HeavyPropellant = "HeavyPropellant";
        public const string BoomerangPropellant = "BoomerangPropellant";
    }
}
```

`BulletFaction` ports verbatim from `Src/PhysicalEntities/Bullets/BulletFaction.cs`.

`BulletStats` is rewritten from the original `record struct` (C# 10, banned — Task 0) to a plain struct, drops the `GameplayConfig`-based constructor (Config isn't ported — "Scope decisions" #5) and drops `Color`/`Position`/`Direction` (presentation + spawn-call args, not stats data — position/direction get passed separately to the spawn call in Task 5, matching how `BasicPropellant.OnSpawn` actually uses `bulletEntity.Stats.Direction`/`.Position`, which we instead pass explicitly). Default values are `Src/Config/GameplayConfig.cs`'s literals for `CannonProjectile*` (lines 100-105: Speed 800, Damage 50, Pierce 1, Size 12, Spread 0.2, Lifetime 5):

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletStats.cs
namespace Gamelab.PhysicalEntities.Bullets
{
    public struct BulletStats
    {
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Pierce { get; set; }
        public float Size { get; set; }
        public float Spread { get; set; }
        public float Lifetime { get; set; }

        public static BulletStats CannonDefault()
        {
            return new BulletStats
            {
                Speed = 800f,
                Damage = 50f,
                Pierce = 1f,
                Size = 12f,
                Spread = 0.2f,
                Lifetime = 5f
            };
        }
    }
}
```

`BulletRecipe` replaces the catalog-relevant slice of `Items/Bullets/BulletItem.cs` — an ordered list of component ids plus the aggregate `EComponentType` (needed by `EnemyAmmoDefinition` in Task 6). It intentionally does **not** port `BulletItem`'s combine/upgrade-badge/texture logic (out of scope — "Scope decisions" #6):

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletRecipe.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;

namespace Gamelab.PhysicalEntities.Bullets
{
    // ponytail: minimal stand-in for Items/Bullets/BulletItem.cs's ordered-component-id
    // shape. The full combine/upgrade-badge crafting logic (Workbench UI, Wave B) isn't
    // ported here — add it alongside that UI work if/when it's needed.
    public sealed class BulletRecipe
    {
        public EComponentType Type { get; }
        public IReadOnlyList<string> ComponentIds { get; }

        public BulletRecipe(EComponentType type, IEnumerable<string> componentIds)
        {
            if (componentIds == null)
            {
                throw new ArgumentNullException(nameof(componentIds));
            }

            string[] ids = componentIds.ToArray();
            if (ids.Length == 0)
            {
                throw new ArgumentException("A bullet recipe needs at least one component id.", nameof(componentIds));
            }

            Type = type;
            ComponentIds = ids;
        }
    }
}
```

`BulletTargetingMath.GetDistanceToLine` is the one piece of `Src/PhysicalEntities/Bullets/BulletTargetingHelper.cs` with zero physics-world/`Body` coupling — port it, engine-free, using `System.Numerics.Vector2`. The rest of `BulletTargetingHelper` (physics-world body enumeration, hostile-target queries) is **not** ported yet — it needs a physics world query API that doesn't exist until A1 lands (note this integration point in the final report, don't build a placeholder physics query API here):

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Bullets/BulletTargetingMath.cs
using System;
using System.Numerics;

namespace Gamelab.PhysicalEntities.Bullets
{
    public static class BulletTargetingMath
    {
        public static float GetDistanceToLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 dir = b - a;
            float lengthSquared = dir.LengthSquared();

            if (lengthSquared == 0)
            {
                return Vector2.Distance(p, a);
            }

            float t = Vector2.Dot(p - a, dir) / lengthSquared;
            if (t < 0)
            {
                return float.MaxValue;
            }

            Vector2 projection = a + t * dir;
            return Vector2.Distance(p, projection);
        }
    }
}
```

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/Bullets/BulletStatsTests.cs
using NUnit.Framework;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletStatsTests
    {
        [Test]
        public void CannonDefault_MatchesGameplayConfigLiterals()
        {
            BulletStats stats = BulletStats.CannonDefault();

            Assert.AreEqual(800f, stats.Speed);
            Assert.AreEqual(50f, stats.Damage);
            Assert.AreEqual(1f, stats.Pierce);
            Assert.AreEqual(12f, stats.Size);
            Assert.AreEqual(0.2f, stats.Spread);
            Assert.AreEqual(5f, stats.Lifetime);
        }
    }
}
```

```csharp
// Unity/Assets/Tests/EditMode/Bullets/BulletRecipeTests.cs
using System;
using NUnit.Framework;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletRecipeTests
    {
        [Test]
        public void Constructor_StoresOrderedComponentIds()
        {
            var recipe = new BulletRecipe(EComponentType.Bullet,
                new[] { ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant });

            Assert.AreEqual(EComponentType.Bullet, recipe.Type);
            CollectionAssert.AreEqual(
                new[] { ComponentIds.BasicProjectile, ComponentIds.BasicCasing, ComponentIds.BasicPropellant },
                recipe.ComponentIds);
        }

        [Test]
        public void Constructor_WithNoComponents_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BulletRecipe(EComponentType.Bullet, Array.Empty<string>()));
        }
    }
}
```

```csharp
// Unity/Assets/Tests/EditMode/Bullets/BulletTargetingMathTests.cs
using System.Numerics;
using NUnit.Framework;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Bullets
{
    public class BulletTargetingMathTests
    {
        [Test]
        public void GetDistanceToLine_PointOnLine_ReturnsZero()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 0));

            Assert.AreEqual(0f, distance, 0.0001f);
        }

        [Test]
        public void GetDistanceToLine_PointBeforeSegmentStart_ReturnsMaxValue()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(-5, 0));

            Assert.AreEqual(float.MaxValue, distance);
        }

        [Test]
        public void GetDistanceToLine_PointOffLine_ReturnsPerpendicularDistance()
        {
            float distance = BulletTargetingMath.GetDistanceToLine(
                new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 3));

            Assert.AreEqual(3f, distance, 0.0001f);
        }
    }
}
```

- [ ] **Step 2: Confirm the tests fail to compile, then create the six source files, then re-run**

Same compile-check / `-runTests` sequence as Task 1. Confirm all 5 test cases show `result="Passed"` in the results XML.

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Port bullet stats, faction, component ids and recipe to Gamelab.Core"
```

### Task 4: `RandomService`-backed spread sampling helper (shared by bullet + enemy spawn)

**Files:**
- Test: `Unity/Assets/Tests/EditMode/Bullets/SpreadSamplingTests.cs` (uses the existing `Gamelab.Services.Random.RandomService` from Phase 0 — no new production file needed)

This is a one-step verification task, not new production code: confirm the already-ported `IRandomService.SampleGaussian` (Phase 0, `Unity/Assets/Scripts/Core/Services/Random/RandomService.cs`) is what Task 5's `BulletRuntime.Spawn` and Task 7's enemy shoot-spread logic will use for `BasicPropellant.OnSpawn`'s `randomService.SampleGaussian(0, spread / 3f)` pattern, so later tasks don't reinvent it.

- [ ] **Step 1: Write and run a characterization test**

```csharp
// Unity/Assets/Tests/EditMode/Bullets/SpreadSamplingTests.cs
using NUnit.Framework;
using Gamelab.Services.Random;

namespace Gamelab.Tests.Bullets
{
    public class SpreadSamplingTests
    {
        [Test]
        public void SampleGaussian_WithZeroSpread_IsExactlyZero()
        {
            IRandomService random = new RandomService();

            double sample = random.SampleGaussian(0, 0f / 3f);

            Assert.AreEqual(0.0, sample);
        }
    }
}
```

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a2-task4-results.xml -logFile /tmp/a2-task4-tests.log
```

Confirm pass. No new production file — this only exists to pin the pattern before two later tasks both need it.

- [ ] **Step 2: Commit**

```bash
git add Unity/Assets/Tests
git commit -m "Pin RandomService.SampleGaussian usage pattern for bullet/enemy spread"
```

### Task 5: Bullet Runtime — `ScriptableObject` components, `BulletDefinitionAsset`, `BulletRuntime`, `IDamageable`

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IDamageable.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/IBulletEmitter.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletComponentAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Casings/BasicCasingAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Propellants/BasicPropellantAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Projectiles/BasicProjectileAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletDefinitionAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletRuntime.cs`
- Create: `Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef` (first PlayMode test in the project — `CONVENTIONS.md` says add this "the first time a subsystem needs a PlayMode test")
- Test: `Unity/Assets/Tests/PlayMode/Bullets/BulletRuntimeSpawnTests.cs`

**Interfaces:**
- Consumes: `Gamelab.PhysicalEntities.Bullets.BulletStats`/`BulletFaction` (Task 3), `Gamelab.Items.Bullets.EComponentType`/`ComponentIds` (Task 3), `Gamelab.Services.Random.IRandomService`/`RandomService` (Phase 0).
- Produces: `Gamelab.PhysicalEntities.Interfaces.IDamageable` (`void TakeDamage(float)`, `bool OnHit(BulletRuntime)`), `Gamelab.PhysicalEntities.Bullets.IBulletEmitter` (empty tag interface, ports verbatim), `.BulletComponentAsset` (abstract `ScriptableObject`), `.BulletDefinitionAsset` (`[CreateAssetMenu]` `ScriptableObject` composing one casing + one propellant + one projectile asset — **this is the "catalog-driven bullet (casing+propellant+projectile combination)" the Wave A gate asks for**), `.BulletRuntime` (`MonoBehaviour`, static `Spawn(...)` factory). Task 6/7 (enemy) consume `IDamageable`/`IBulletEmitter`/`BulletRuntime`.

**Design note on per-instance effect state:** `Src/`'s `IBulletEffect`/`AbstractComponent` are per-bullet-instance C# objects (cloned via `Copy()`, tracked by `Guid`/`IsRootEffect` so recursive effects like Matryoshka/Frangible can spawn distinguishable child bullets). `ScriptableObject` assets are shared config by default. This task's `BulletComponentAsset` is a **shared, stateless** behavior+config asset (the Unity-idiomatic "ability/item asset" pattern); any per-bullet mutable state (e.g. `BasicCasing`'s trail-emitter-per-bullet dictionary) is dropped for now since there's no VFX service to drive it yet (Scope decisions #4) and moves to the `BulletRuntime` instance, not the shared asset, whenever it's needed.

```csharp
// ponytail: dropped Guid/IsRootEffect/Copy() from the original IBulletEffect — nothing
// in this vertical slice recursively spawns child bullets yet. Add per-instance identity
// back (e.g. a struct carried by BulletRuntime, not the shared asset) when Matryoshka/
// Frangible projectiles are ported (breadth task).
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Interfaces/IDamageable.cs
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float damageAmount);
        bool OnHit(BulletRuntime bullet);
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/IBulletEmitter.cs
namespace Gamelab.PhysicalEntities.Bullets
{
    public interface IBulletEmitter
    {
        // just a tag to make it easier to find the emitter component in the bullet entity
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletComponentAsset.cs
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    public abstract class BulletComponentAsset : ScriptableObject
    {
        public abstract EComponentType Type { get; }
        public abstract string ComponentId { get; }

        public virtual void OnCreate(BulletRuntime bullet)
        {
        }

        public virtual void OnSpawn(BulletRuntime bullet)
        {
        }

        public virtual void OnUpdate(BulletRuntime bullet, float deltaTime)
        {
        }

        public virtual void OnHit(BulletRuntime bullet, IDamageable hitEntity)
        {
        }

        public virtual void OnCleanup(BulletRuntime bullet)
        {
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Casings/BasicCasingAsset.cs
using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Casings
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Casings/Basic Casing", fileName = "BasicCasing")]
    public class BasicCasingAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Casing;
        public override string ComponentId => ComponentIds.BasicCasing;

        // ponytail: Src/.../Components/Casings/BasicCasing.cs spawns a particle trail
        // emitter here via IVfxService, which isn't built in Unity yet (Scope decisions #4).
        // No behavior needed for the vertical slice beyond identifying as a casing.
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Propellants/BasicPropellantAsset.cs
using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Propellants
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Propellants/Basic Propellant", fileName = "BasicPropellant")]
    public class BasicPropellantAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Propellant;
        public override string ComponentId => ComponentIds.BasicPropellant;

        private static readonly IRandomService RandomService = new RandomService();

        public override void OnSpawn(BulletRuntime bullet)
        {
            // Mirrors Src/PhysicalEntities/Bullets/Components/Propellants/BasicPropellant.cs
            // OnSpawn: sample a gaussian spread angle, rotate the aim direction by it, then
            // set the Rigidbody2D velocity. bullet.Stats.Spread / 3f matches the original's
            // spread-to-standard-deviation scaling.
            float randomSpread = (float)RandomService.SampleGaussian(0, bullet.Stats.Spread / 3f);
            Vector2 direction = RotateVector(bullet.AimDirection, randomSpread);
            bullet.PhysicsBody.linearVelocity = direction * bullet.Stats.Speed;
        }

        private static Vector2 RotateVector(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/Projectiles/BasicProjectileAsset.cs
using Gamelab.Items.Bullets;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets.Projectiles
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Projectiles/Basic Projectile", fileName = "BasicProjectile")]
    public class BasicProjectileAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Projectile;
        public override string ComponentId => ComponentIds.BasicProjectile;
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletDefinitionAsset.cs
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // The catalog-driven casing+propellant+projectile combination the Wave A gate asks for.
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Bullet Definition", fileName = "BulletDefinition")]
    public class BulletDefinitionAsset : ScriptableObject
    {
        [SerializeField] private BasicCasingAsset casing;
        [SerializeField] private BasicPropellantAsset propellant;
        [SerializeField] private BasicProjectileAsset projectile;
        [SerializeField] private float speed = 800f;
        [SerializeField] private float damage = 50f;
        [SerializeField] private float pierce = 1f;
        [SerializeField] private float size = 12f;
        [SerializeField] private float spread = 0.2f;
        [SerializeField] private float lifetime = 5f;

        public BasicCasingAsset Casing => casing;
        public BasicPropellantAsset Propellant => propellant;
        public BasicProjectileAsset Projectile => projectile;

        public BulletStats ToStats()
        {
            return new BulletStats
            {
                Speed = speed,
                Damage = damage,
                Pierce = pierce,
                Size = size,
                Spread = spread,
                Lifetime = lifetime
            };
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Bullets/BulletRuntime.cs
using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BulletRuntime : MonoBehaviour
    {
        public BulletStats Stats { get; private set; }
        public BulletFaction Faction { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public bool IsActive { get; private set; } = true;
        public Rigidbody2D PhysicsBody { get; private set; }

        private BulletDefinitionAsset definition;

        public static BulletRuntime Spawn(
            BulletDefinitionAsset definition,
            Vector2 position,
            Vector2 aimDirection,
            BulletFaction faction)
        {
            var go = new GameObject($"Bullet_{definition.name}");
            go.transform.position = position;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var bullet = go.AddComponent<BulletRuntime>();
            bullet.PhysicsBody = rb;
            bullet.definition = definition;
            bullet.Stats = definition.ToStats();
            bullet.Faction = faction;
            bullet.AimDirection = aimDirection.normalized;

            definition.Casing.OnCreate(bullet);
            definition.Propellant.OnCreate(bullet);
            definition.Projectile.OnCreate(bullet);

            definition.Casing.OnSpawn(bullet);
            definition.Propellant.OnSpawn(bullet);
            definition.Projectile.OnSpawn(bullet);

            return bullet;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            definition.Casing.OnUpdate(this, deltaTime);
            definition.Propellant.OnUpdate(this, deltaTime);
            definition.Projectile.OnUpdate(this, deltaTime);
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
```

- [ ] **Step 1: Create `Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef`**

```json
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

- [ ] **Step 2: Write the failing PlayMode test**

```csharp
// Unity/Assets/Tests/PlayMode/Bullets/BulletRuntimeSpawnTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using Gamelab.PhysicalEntities.Bullets.Projectiles;

namespace Gamelab.Tests.Bullets
{
    public class BulletRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogDefinition_AppliesStatsAndVelocity()
        {
            var casing = ScriptableObject.CreateInstance<BasicCasingAsset>();
            var propellant = ScriptableObject.CreateInstance<BasicPropellantAsset>();
            var projectile = ScriptableObject.CreateInstance<BasicProjectileAsset>();

            var definition = ScriptableObject.CreateInstance<BulletDefinitionAsset>();
            typeof(BulletDefinitionAsset).GetField("casing",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(definition, casing);
            typeof(BulletDefinitionAsset).GetField("propellant",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(definition, propellant);
            typeof(BulletDefinitionAsset).GetField("projectile",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(definition, projectile);
            typeof(BulletDefinitionAsset).GetField("spread",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(definition, 0f); // zero spread => deterministic velocity direction

            BulletRuntime bullet = BulletRuntime.Spawn(
                definition, Vector2.zero, Vector2.right, BulletFaction.Player);

            yield return null; // let physics settle one frame

            Assert.AreEqual(800f, bullet.Stats.Speed);
            Assert.AreEqual(BulletFaction.Player, bullet.Faction);
            Assert.Greater(bullet.PhysicsBody.linearVelocity.x, 0f);
            Assert.AreEqual(0f, bullet.PhysicsBody.linearVelocity.y, 0.01f);

            Object.Destroy(bullet.gameObject);
        }
    }
}
```

- [ ] **Step 3: Confirm compile failure, then create the eight production files, recompile**

- [ ] **Step 4: Run PlayMode tests**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a2-task5-results.xml -logFile /tmp/a2-task5-tests.log
```

Confirm `<test-case ... name="...Spawn_FromCatalogDefinition_AppliesStatsAndVelocity" ... result="Passed" />` in the results XML. Also re-run the EditMode suite to confirm no regression.

- [ ] **Step 5: In the Unity Editor project (or via a one-off script), create the three asset instances and one `BulletDefinitionAsset` instance wiring them together, save under `Unity/Assets/Content/Bullets/`** — this is the actual catalog data asset, not just the test-created in-memory instances. Since this step needs the Editor GUI (asset creation menu) and this agent has no GUI access, do it via a small `[MenuItem]`-free approach: write a temporary EditMode test/one-off `AssetDatabase.CreateAsset` call, run it once via `-executeMethod`, then delete the throwaway script, OR skip this step and note in the final report that the runtime code path is proven by the PlayMode test's in-memory instances, and creating on-disk example assets is a follow-up for whoever first opens the Editor GUI on this branch.

- [ ] **Step 6: Commit**

```bash
git add Unity/Assets
git commit -m "Port bullet casing/propellant/projectile components and BulletRuntime spawn (catalog-driven bullet gate item)"
```

---

## Milestone 3: Enemies vertical slice

### Task 6: Enemy Core data (ids, type, definition, ammo definition, spawn option, movement profile)

**Files:**
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyType.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyIds.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyDefinition.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyAmmoIds.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyAmmoDefinition.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemySpawnOption.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyMovementProfile.cs`
- Test: `Unity/Assets/Tests/EditMode/Enemies/EnemyDefinitionTests.cs`
- Test: `Unity/Assets/Tests/EditMode/Enemies/EnemyAmmoDefinitionTests.cs`
- Test: `Unity/Assets/Tests/EditMode/Enemies/EnemyMovementProfileTests.cs`

**Interfaces:**
- Consumes: `Gamelab.PhysicalEntities.Bullets.BulletRecipe`, `Gamelab.Items.Bullets.EComponentType`/`ComponentIds` (Task 3).
- Produces: `Gamelab.Enemies.Core.EnemyType` (`Rifle`, `Dummy`, `TutorialRifle`), `.EnemyIds`, `.EnemyDefinition` (struct, `Type`→`Id` mapping + `Parse`), `.EnemyAmmoIds`, `.EnemyAmmoDefinition` (uses `BulletRecipe` instead of `BulletItem`), `.EnemySpawnOption`, `Gamelab.Enemies.Movement.EnemyMovementProfile`. Task 7 (enemy Runtime/catalog) consumes all of these.

`EnemyType`/`EnemyIds` port verbatim.

`EnemyDefinition` is rewritten from `readonly record struct` (banned) to a plain `readonly struct`:

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemyDefinition.cs
using System;

namespace Gamelab.Enemies.Core
{
    public readonly struct EnemyDefinition
    {
        public EnemyType Type { get; }

        public EnemyDefinition(EnemyType type)
        {
            Type = type;
        }

        public string Id => Type switch
        {
            EnemyType.Rifle => EnemyIds.Rifle,
            EnemyType.Dummy => EnemyIds.Dummy,
            EnemyType.TutorialRifle => EnemyIds.TutorialRifle,
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unknown enemy type.")
        };

        public static EnemyDefinition Parse(string id)
        {
            return id switch
            {
                EnemyIds.Rifle => new EnemyDefinition(EnemyType.Rifle),
                EnemyIds.Dummy => new EnemyDefinition(EnemyType.Dummy),
                EnemyIds.TutorialRifle => new EnemyDefinition(EnemyType.TutorialRifle),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown enemy id.")
            };
        }
    }
}
```

`EnemyAmmoDefinition` ports from `Src/Enemies/Core/EnemyAmmoDefinition.cs`, rewritten for C# 9 (`ArgumentException.ThrowIfNullOrWhiteSpace`/`ArgumentNullException.ThrowIfNull` → manual checks) and returning `BulletRecipe` instead of `BulletItem`:

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemyAmmoDefinition.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Enemies.Core
{
    public sealed class EnemyAmmoDefinition
    {
        public string Id { get; }
        public IReadOnlyList<string> OrderedComponentIds { get; }
        public int AmmoSurcharge { get; }
        public int MinLevel { get; }
        public float ProceduralWeight { get; }

        public EnemyAmmoDefinition(
            string id,
            IEnumerable<string> orderedComponentIds,
            int ammoSurcharge,
            int minLevel,
            float proceduralWeight)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Enemy ammo id must not be blank.", nameof(id));
            }

            if (orderedComponentIds == null)
            {
                throw new ArgumentNullException(nameof(orderedComponentIds));
            }

            string[] normalizedComponentIds = orderedComponentIds.ToArray();
            if (normalizedComponentIds.Length == 0)
            {
                throw new ArgumentException("Enemy ammo recipes must contain at least one component.",
                    nameof(orderedComponentIds));
            }

            if (!normalizedComponentIds.Contains(ComponentIds.EnemyCasing))
            {
                throw new ArgumentException("Enemy ammo recipes must include EnemyCasing.",
                    nameof(orderedComponentIds));
            }

            Id = id;
            OrderedComponentIds = normalizedComponentIds;
            AmmoSurcharge = Math.Max(0, ammoSurcharge);
            MinLevel = Math.Max(1, minLevel);
            ProceduralWeight = Math.Max(0f, proceduralWeight);
        }

        public bool IsUnlockedAtLevel(int levelNumber)
        {
            return Math.Max(1, levelNumber) >= MinLevel;
        }

        public BulletRecipe BuildRecipe()
        {
            return new BulletRecipe(EComponentType.Bullet, OrderedComponentIds);
        }
    }
}
```

`EnemySpawnOption` rewritten from `readonly record struct` to `readonly struct`:

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemySpawnOption.cs
namespace Gamelab.Enemies.Core
{
    public readonly struct EnemySpawnOption
    {
        public EnemyType Type { get; }
        public EnemyAmmoDefinition AmmoDefinition { get; }

        public EnemySpawnOption(EnemyType type, EnemyAmmoDefinition ammoDefinition)
        {
            Type = type;
            AmmoDefinition = ammoDefinition;
        }
    }
}
```

`EnemyMovementProfile` rewritten from `readonly record struct` with named-parameter factory to `readonly struct`, values copied verbatim from `Src/Enemies/Movement/EnemyMovementProfile.cs`:

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemyMovementProfile.cs
namespace Gamelab.Enemies.Movement
{
    public readonly struct EnemyMovementProfile
    {
        public float MaxForwardSpeed { get; }
        public float MaxReverseSpeed { get; }
        public float MaxLateralSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }
        public float ArrivalRadius { get; }
        public float BrakeRadius { get; }

        public EnemyMovementProfile(
            float maxForwardSpeed,
            float maxReverseSpeed,
            float maxLateralSpeed,
            float acceleration,
            float deceleration,
            float arrivalRadius,
            float brakeRadius)
        {
            MaxForwardSpeed = maxForwardSpeed;
            MaxReverseSpeed = maxReverseSpeed;
            MaxLateralSpeed = maxLateralSpeed;
            Acceleration = acceleration;
            Deceleration = deceleration;
            ArrivalRadius = arrivalRadius;
            BrakeRadius = brakeRadius;
        }

        public static EnemyMovementProfile CreateDefault(float maxForwardSpeed)
        {
            return new EnemyMovementProfile(
                maxForwardSpeed: maxForwardSpeed,
                maxReverseSpeed: maxForwardSpeed * 0.35f,
                maxLateralSpeed: maxForwardSpeed * 0.8f,
                acceleration: maxForwardSpeed * 2.4f,
                deceleration: maxForwardSpeed * 3.2f,
                arrivalRadius: 18f,
                brakeRadius: 120f);
        }
    }
}
```

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/Enemies/EnemyDefinitionTests.cs
using NUnit.Framework;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemyDefinitionTests
    {
        [Test]
        public void Id_ForDummy_ReturnsDummyId()
        {
            var definition = new EnemyDefinition(EnemyType.Dummy);

            Assert.AreEqual(EnemyIds.Dummy, definition.Id);
        }

        [Test]
        public void Parse_RoundTripsWithId()
        {
            EnemyDefinition parsed = EnemyDefinition.Parse(EnemyIds.Rifle);

            Assert.AreEqual(EnemyType.Rifle, parsed.Type);
        }
    }
}
```

```csharp
// Unity/Assets/Tests/EditMode/Enemies/EnemyAmmoDefinitionTests.cs
using System;
using NUnit.Framework;
using Gamelab.Enemies.Core;
using Gamelab.Items.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyAmmoDefinitionTests
    {
        [Test]
        public void Constructor_WithoutEnemyCasing_Throws()
        {
            Assert.Throws<ArgumentException>(() => new EnemyAmmoDefinition(
                "Basic", new[] { ComponentIds.BasicProjectile }, 0, 1, 1f));
        }

        [Test]
        public void IsUnlockedAtLevel_BelowMinLevel_ReturnsFalse()
        {
            var ammo = new EnemyAmmoDefinition(
                "Heavy",
                new[] { ComponentIds.BasicProjectile, ComponentIds.EnemyCasing, ComponentIds.HeavyPropellant },
                ammoSurcharge: 1, minLevel: 3, proceduralWeight: 0.8f);

            Assert.IsFalse(ammo.IsUnlockedAtLevel(2));
            Assert.IsTrue(ammo.IsUnlockedAtLevel(3));
        }

        [Test]
        public void BuildRecipe_ReturnsRecipeWithSameComponentIds()
        {
            var ammo = new EnemyAmmoDefinition(
                "Basic",
                new[] { ComponentIds.BasicProjectile, ComponentIds.EnemyCasing },
                0, 1, 1f);

            var recipe = ammo.BuildRecipe();

            CollectionAssert.AreEqual(ammo.OrderedComponentIds, recipe.ComponentIds);
        }
    }
}
```

```csharp
// Unity/Assets/Tests/EditMode/Enemies/EnemyMovementProfileTests.cs
using NUnit.Framework;
using Gamelab.Enemies.Movement;

namespace Gamelab.Tests.Enemies
{
    public class EnemyMovementProfileTests
    {
        [Test]
        public void CreateDefault_ScalesFromMaxForwardSpeed()
        {
            EnemyMovementProfile profile = EnemyMovementProfile.CreateDefault(700f);

            Assert.AreEqual(700f, profile.MaxForwardSpeed);
            Assert.AreEqual(245f, profile.MaxReverseSpeed, 0.01f);
            Assert.AreEqual(560f, profile.MaxLateralSpeed, 0.01f);
        }
    }
}
```

- [ ] **Step 2: Confirm compile failure, create the seven source files, recompile, run EditMode tests, confirm all pass**

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Port enemy catalog data (type, definition, ammo definition, movement profile) to Gamelab.Core"
```

### Task 7: Enemy Runtime — `EnemyCatalogAsset`, `EnemyRuntime`, `DummyEnemyRuntime`, movement + slots

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/Enemies/EnemyCatalogAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/Enemies/EnemyRuntime.cs`
- Create: `Unity/Assets/Scripts/Runtime/Enemies/DummyEnemyRuntime.cs`
- Create: `Unity/Assets/Scripts/Runtime/Enemies/EnemyMovementController.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemyTrainSlot.cs`
- Create: `Unity/Assets/Scripts/Core/Enemies/EnemySlotManager.cs`
- Test: `Unity/Assets/Tests/EditMode/Enemies/EnemySlotManagerTests.cs`
- Test: `Unity/Assets/Tests/PlayMode/Enemies/EnemyRuntimeSpawnTests.cs`

**Interfaces:**
- Consumes: `IDamageable`, `IBulletEmitter`, `BulletRuntime` (Task 5); `EnemyType`, `EnemyDefinition`, `EnemyMovementProfile` (Task 6).
- Produces: `Gamelab.Enemies.Core.EnemySlotSide`, `.EnemyTrainSlot` (Core — no `GamelabGame.Instance`/map dependency in the slot's own data shape, only `GetAnchor`'s original implementation needed the map; ported minus `GetAnchor`, see below), `.EnemySlotManager` (Core, pure), `Gamelab.Enemies.Movement.EnemyMovementController` (Runtime, `Rigidbody2D`-based), `Gamelab.Enemies.EnemyCatalogAsset` (`ScriptableObject` catalog — **the "one enemy" catalog the Wave A gate asks for**), `.EnemyRuntime` (`MonoBehaviour` implementing `IDamageable`, `IBulletEmitter`), `.DummyEnemyRuntime` (concrete `EnemyRuntime` subclass, the vertical-slice enemy).

`EnemyTrainSlot.GetAnchor` in `Src/` calls `gameplayContext.Map.GetBounds()` — Levels/Map is Wave B (B1), doesn't exist. Port everything except `GetAnchor`; note this as the map integration point (B1) in the final report:

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemyTrainSlot.cs
namespace Gamelab.Enemies.Core
{
    public enum EnemySlotSide
    {
        Top,
        Bottom
    }

    public readonly struct EnemyTrainSlot
    {
        public EnemySlotSide Side { get; }
        public float PositionRatio { get; }

        public EnemyTrainSlot(EnemySlotSide side, float positionRatio)
        {
            Side = side;
            PositionRatio = positionRatio;
        }

        // GetAnchor(float distanceFromTrain) is not ported yet: it needs the level/map
        // bounds (Src/Map/), which is Wave B (B1)'s subsystem and doesn't exist yet.
        // Add it back here once B1's map bounds API exists.
    }
}
```

`EnemySlotManager` ports near-verbatim (primary constructor and collection expressions rewritten for C# 9):

```csharp
// Unity/Assets/Scripts/Core/Enemies/EnemySlotManager.cs
using System;
using System.Collections.Generic;

namespace Gamelab.Enemies.Core
{
    public class EnemySlotManager
    {
        private static readonly EnemyTrainSlot[] SideAttackSlots =
        {
            new EnemyTrainSlot(EnemySlotSide.Top, 0.15f),
            new EnemyTrainSlot(EnemySlotSide.Top, 0.5f),
            new EnemyTrainSlot(EnemySlotSide.Top, 0.85f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.15f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.5f),
            new EnemyTrainSlot(EnemySlotSide.Bottom, 0.85f)
        };

        private readonly Random random = new Random();
        private readonly HashSet<EnemyTrainSlot> occupiedSlots = new HashSet<EnemyTrainSlot>();

        public bool TryReserveSideAttackSlot(out EnemyTrainSlot slot)
        {
            return TryReserveSlot(SideAttackSlots, out slot);
        }

        public void ReleaseSlot(EnemyTrainSlot slot)
        {
            occupiedSlots.Remove(slot);
        }

        public void Clear()
        {
            occupiedSlots.Clear();
        }

        private bool TryReserveSlot(IReadOnlyList<EnemyTrainSlot> candidateSlots, out EnemyTrainSlot slot)
        {
            var availableSlots = new List<EnemyTrainSlot>();

            foreach (EnemyTrainSlot candidate in candidateSlots)
            {
                if (!occupiedSlots.Contains(candidate))
                {
                    availableSlots.Add(candidate);
                }
            }

            if (availableSlots.Count == 0)
            {
                slot = default;
                return false;
            }

            slot = availableSlots[random.Next(availableSlots.Count)];
            occupiedSlots.Add(slot);
            return true;
        }
    }
}
```

Note: `EnemyTrainSlot` needs value equality for `HashSet<EnemyTrainSlot>.Contains`/`.Add`/`.Remove` to work correctly — a plain `readonly struct` gets member-wise `Equals`/`GetHashCode` from `ValueType` by default (reflection-based, slower but correct), which is fine here (small struct, infrequent calls, matches the original's behavior since the original was also just relying on record-struct-generated equality over the same two fields). Don't hand-roll `Equals`/`GetHashCode` for this — that would be solving a performance problem nothing has.

`EnemyMovementController` ports from `Src/Enemies/Movement/EnemyMovementController.cs`, dropping the `gameplayContext`/train-drift dependency (Levels/Map, Wave B — not ported, `includeTrainDrift` parameter kept but drift is always treated as 0 for now) and using `Rigidbody2D`:

```csharp
// Unity/Assets/Scripts/Runtime/Enemies/EnemyMovementController.cs
using Gamelab.Enemies.Movement;
using UnityEngine;

namespace Gamelab.Enemies
{
    // ponytail: train-frame drift (Src/Enemies/Movement/EnemyMovementController.cs's
    // GetTrainFrameDrift, from Gamelab.Map.Train.State) is dropped — Levels/Map is
    // Wave B (B1)'s subsystem and doesn't exist yet. Add it back once B1 exposes train
    // scroll speed.
    public class EnemyMovementController
    {
        private readonly Rigidbody2D physicsBody;
        public EnemyMovementProfile Profile { get; }

        public EnemyMovementController(Rigidbody2D physicsBody, EnemyMovementProfile profile)
        {
            this.physicsBody = physicsBody;
            Profile = profile;
        }

        public void UpdateTowardPoint(Vector2 targetPosition, float deltaTime)
        {
            Vector2 currentPosition = physicsBody.position;
            Vector2 toTarget = targetPosition - currentPosition;
            float distance = toTarget.magnitude;

            if (distance <= Profile.ArrivalRadius)
            {
                UpdateStop(deltaTime);
                return;
            }

            Vector2 direction = toTarget / distance;
            float desiredSpeed = Profile.MaxForwardSpeed;
            if (distance < Profile.BrakeRadius)
            {
                float t = Mathf.Clamp01(distance / Profile.BrakeRadius);
                desiredSpeed = Mathf.Lerp(0f, Profile.MaxForwardSpeed, t);
            }

            UpdateTowardDirection(direction, desiredSpeed, deltaTime);
        }

        public void UpdateTowardDirection(Vector2 direction, float desiredSpeed, float deltaTime)
        {
            if (direction == Vector2.zero || desiredSpeed <= 0f)
            {
                UpdateStop(deltaTime);
                return;
            }

            direction.Normalize();
            Vector2 desiredVelocity = direction * desiredSpeed;
            Vector2 nextVelocity = MoveVelocityTowards(desiredVelocity, deltaTime);
            physicsBody.linearVelocity = nextVelocity;
        }

        public void UpdateStop(float deltaTime)
        {
            Vector2 nextVelocity = MoveVelocityTowards(Vector2.zero, deltaTime);
            physicsBody.linearVelocity = nextVelocity;
        }

        private Vector2 MoveVelocityTowards(Vector2 desiredVelocity, float deltaTime)
        {
            Vector2 current = physicsBody.linearVelocity;
            float maxDelta = desiredVelocity.sqrMagnitude > current.sqrMagnitude
                ? Profile.Acceleration * deltaTime
                : Profile.Deceleration * deltaTime;

            Vector2 next = Vector2.MoveTowards(current, desiredVelocity, maxDelta);
            return Clamp(next);
        }

        private Vector2 Clamp(Vector2 velocity)
        {
            float x = Mathf.Clamp(velocity.x, -Profile.MaxReverseSpeed, Profile.MaxForwardSpeed);
            float y = Mathf.Clamp(velocity.y, -Profile.MaxLateralSpeed, Profile.MaxLateralSpeed);
            return new Vector2(x, y);
        }
    }
}
```

`EnemyCatalogAsset`/`EnemyRuntime`/`DummyEnemyRuntime` — the ScriptableObject catalog and its minimal concrete enemy. `AbstractEnemy`'s sound-service calls (`Src/Enemies/Core/AbstractEnemy.cs`) are dropped per Scope decisions #4; health/damage/hit logic ports:

```csharp
// Unity/Assets/Scripts/Runtime/Enemies/EnemyCatalogAsset.cs
using System;
using System.Collections.Generic;
using Gamelab.Enemies.Core;
using UnityEngine;

namespace Gamelab.Enemies
{
    [Serializable]
    public class EnemyCatalogEntry
    {
        public EnemyType type;
        public float health = 100f; // Src/Config/GameplayConfig.cs EnemyHealth default
        public float size = 72f;    // Src/Config/GameplayConfig.cs EnemySize default
        public EnemyRuntime prefab;
    }

    [CreateAssetMenu(menuName = "Gamelab/Enemies/Enemy Catalog", fileName = "EnemyCatalog")]
    public class EnemyCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<EnemyCatalogEntry> entries = new List<EnemyCatalogEntry>();

        public EnemyCatalogEntry Get(EnemyType type)
        {
            foreach (EnemyCatalogEntry entry in entries)
            {
                if (entry.type == type)
                {
                    return entry;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type in catalog.");
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/Enemies/EnemyRuntime.cs
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyRuntime : MonoBehaviour, IDamageable, IBulletEmitter
    {
        public float Health { get; protected set; }
        public bool IsAlive => Health > 0;
        public bool ShouldRemove { get; protected set; }
        public Rigidbody2D PhysicsBody { get; private set; }

        public virtual void Initialize(EnemyCatalogEntry catalogEntry)
        {
            Health = catalogEntry.health;
            PhysicsBody = GetComponent<Rigidbody2D>();
            PhysicsBody.gravityScale = 0f;
            PhysicsBody.freezeRotation = true;
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                ShouldRemove = true;
            }
        }

        // ponytail: Src/Enemies/Core/AbstractEnemy.cs's OnHit also checks
        // "bullet.InitialShooter is CannonStation or CannonSlot" — the station-specific
        // shooter check is dropped here since it needs the concrete station types
        // (Task 10/13). Any bullet with Faction == Player currently damages any enemy.
        public virtual bool OnHit(BulletRuntime bullet)
        {
            if (bullet.Faction != BulletFaction.Player || !IsAlive || ShouldRemove)
            {
                return false;
            }

            TakeDamage(bullet.Stats.Damage);
            return true;
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/Enemies/DummyEnemyRuntime.cs
namespace Gamelab.Enemies
{
    // Vertical-slice concrete enemy: Src/Enemies/Types/DummyEnemy.cs has no sound/
    // animation/VFX coupling (unlike Enemy.cs's horse+rider state machine), so it's
    // the one this agent ports behaviorally. Visual representation (the original draws
    // a red filled rectangle) is left to whichever agent wires up SpriteRenderer/prefabs.
    public class DummyEnemyRuntime : EnemyRuntime
    {
    }
}
```

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/Enemies/EnemySlotManagerTests.cs
using NUnit.Framework;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemySlotManagerTests
    {
        [Test]
        public void TryReserveSideAttackSlot_TwiceForAllSlots_EventuallyFails()
        {
            var manager = new EnemySlotManager();
            int reserved = 0;
            while (manager.TryReserveSideAttackSlot(out _))
            {
                reserved++;
                Assert.Less(reserved, 100, "Should not be able to reserve unboundedly.");
            }

            Assert.AreEqual(6, reserved);
        }

        [Test]
        public void ReleaseSlot_MakesSlotReservableAgain()
        {
            var manager = new EnemySlotManager();
            manager.TryReserveSideAttackSlot(out EnemyTrainSlot slot);
            manager.ReleaseSlot(slot);

            manager.Clear();
            Assert.IsTrue(manager.TryReserveSideAttackSlot(out _));
        }
    }
}
```

```csharp
// Unity/Assets/Tests/PlayMode/Enemies/EnemyRuntimeSpawnTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;

namespace Gamelab.Tests.Enemies
{
    public class EnemyRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogEntry_AppliesHealthAndTakesDamageFromPlayerBullet()
        {
            var catalog = ScriptableObject.CreateInstance<EnemyCatalogAsset>();
            var entry = new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f };
            typeof(EnemyCatalogAsset).GetField("entries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(catalog, new System.Collections.Generic.List<EnemyCatalogEntry> { entry });

            var go = new GameObject("DummyEnemy");
            go.AddComponent<Rigidbody2D>();
            var enemy = go.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(catalog.Get(EnemyType.Dummy));

            yield return null;

            Assert.AreEqual(100f, enemy.Health);
            Assert.IsTrue(enemy.IsAlive);

            var bulletGo = new GameObject("FakeBullet");
            bulletGo.AddComponent<Rigidbody2D>();
            var bullet = bulletGo.AddComponent<BulletRuntime>();
            // Reflection is used here only to arrange a BulletRuntime with a known Faction/
            // Stats for the test, since BulletRuntime's real constructor path is Spawn()
            // (Task 5), which needs a BulletDefinitionAsset this test doesn't need.
            typeof(BulletRuntime).GetProperty("Faction").SetValue(bullet, BulletFaction.Player);
            typeof(BulletRuntime).GetProperty("Stats").SetValue(bullet, new BulletStats { Damage = 30f });

            bool hit = enemy.OnHit(bullet);

            Assert.IsTrue(hit);
            Assert.AreEqual(70f, enemy.Health);

            Object.Destroy(go);
            Object.Destroy(bulletGo);
        }
    }
}
```

If `BulletRuntime.Faction`/`.Stats` don't have accessible setters for the reflection call (they're `{ get; private set; }`, which reflection's `SetValue` can still reach — confirm this works; if the Unity/Mono reflection stack rejects it, add an `internal` test-only setter method on `BulletRuntime` instead, e.g. `internal void SetForTest(BulletFaction faction, BulletStats stats)`, guarded by nothing special since `internal` is already invisible outside the assembly — prefer this over loosening the public API).

- [ ] **Step 2: Confirm compile failure, create the six production files, recompile**

- [ ] **Step 3: Run EditMode then PlayMode tests, confirm all pass** (same `-runTests` invocations as before, separate `-testPlatform EditMode` / `PlayMode` runs)

- [ ] **Step 4: Commit**

```bash
git add Unity/Assets
git commit -m "Port enemy movement, slots, catalog and DummyEnemy runtime (one-enemy gate item)"
```

### Task 8: Combined PlayMode smoke test — catalog-driven bullet + enemy spawn together (Wave A gate)

**Files:**
- Test: `Unity/Assets/Tests/PlayMode/Gate/CatalogDrivenSpawnSmokeTest.cs`

**Interfaces:**
- Consumes: `BulletRuntime.Spawn` (Task 5), `EnemyCatalogAsset`/`DummyEnemyRuntime` (Task 7).

This is the literal Wave A gate item: "catalog-driven bullet (casing+propellant+projectile combination) and one enemy spawn correctly from data, verified via a PlayMode smoke test." Tasks 5 and 7 already have their own PlayMode tests proving each half; this task adds one more test that does both in the same scene/frame, so there's a single test the coordinator can point to as gate evidence.

- [ ] **Step 1: Write the test**

```csharp
// Unity/Assets/Tests/PlayMode/Gate/CatalogDrivenSpawnSmokeTest.cs
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Casings;
using Gamelab.PhysicalEntities.Bullets.Propellants;
using Gamelab.PhysicalEntities.Bullets.Projectiles;

namespace Gamelab.Tests.Gate
{
    // Wave A verification gate (docs/superpowers/plans/2026-09-18-unity-port-plan.md,
    // "Wave A verification gate", A2 row): a catalog-driven bullet (casing+propellant+
    // projectile combination) and one enemy spawn correctly from data.
    public class CatalogDrivenSpawnSmokeTest
    {
        [UnityTest]
        public IEnumerator CatalogDrivenBulletAndEnemy_BothSpawnFromData()
        {
            // --- Bullet: casing + propellant + projectile combination from a catalog asset ---
            var definition = ScriptableObject.CreateInstance<BulletDefinitionAsset>();
            SetPrivate(definition, "casing", ScriptableObject.CreateInstance<BasicCasingAsset>());
            SetPrivate(definition, "propellant", ScriptableObject.CreateInstance<BasicPropellantAsset>());
            SetPrivate(definition, "projectile", ScriptableObject.CreateInstance<BasicProjectileAsset>());
            SetPrivate(definition, "spread", 0f);

            BulletRuntime bullet = BulletRuntime.Spawn(definition, Vector2.zero, Vector2.right, BulletFaction.Player);

            // --- Enemy: spawned from a catalog entry ---
            var catalog = ScriptableObject.CreateInstance<EnemyCatalogAsset>();
            var entry = new EnemyCatalogEntry { type = EnemyType.Dummy, health = 100f, size = 72f };
            SetPrivate(catalog, "entries", new List<EnemyCatalogEntry> { entry });

            var enemyGo = new GameObject("DummyEnemy");
            enemyGo.AddComponent<Rigidbody2D>();
            var enemy = enemyGo.AddComponent<DummyEnemyRuntime>();
            enemy.Initialize(catalog.Get(EnemyType.Dummy));

            yield return null;

            Assert.IsNotNull(bullet);
            Assert.AreEqual(800f, bullet.Stats.Speed, "Bullet stats should come from the catalog definition.");
            Assert.Greater(bullet.PhysicsBody.linearVelocity.x, 0f, "Propellant should apply velocity on spawn.");

            Assert.IsNotNull(enemy);
            Assert.AreEqual(100f, enemy.Health, "Enemy health should come from the catalog entry.");
            Assert.IsTrue(enemy.IsAlive);

            bool tookDamage = enemy.OnHit(bullet);
            Assert.IsTrue(tookDamage, "The catalog-spawned player bullet should be able to damage the catalog-spawned enemy.");
            Assert.AreEqual(50f, enemy.Health, "Damage should come from the bullet's catalog-driven stats.");

            Object.Destroy(bullet.gameObject);
            Object.Destroy(enemyGo);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }
    }
}
```

- [ ] **Step 2: Run PlayMode tests, confirm this test and all prior PlayMode/EditMode tests pass**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/a2-task8-playmode.xml -logFile /tmp/a2-task8-playmode.log

/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/a2-task8-editmode.xml -logFile /tmp/a2-task8-editmode.log
```

Confirm `CatalogDrivenBulletAndEnemy_BothSpawnFromData` shows `result="Passed"`. This XML is the gate evidence to cite in the final report.

- [ ] **Step 3: Verify the MonoGame reference build is still unaffected** (this plan never touches `Src/`, but confirm anyway since it's a standing constraint)

```bash
dotnet build Src/Gamelab.csproj
```

Expected: builds with no errors (a full `dotnet run` isn't needed for a headless check — build success is sufficient evidence nothing under `Src/` changed in a way that breaks compilation; note in the final report that a human should still do a quick interactive run before this branch integrates).

- [ ] **Step 4: Commit**

```bash
git add Unity/Assets
git commit -m "Add combined catalog-driven bullet+enemy PlayMode smoke test (Wave A gate)"
```

---

## Milestone 4: Stations vertical slice

### Task 9: Station Core data (ids, definition)

**Files:**
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationIds.cs`
- Create: `Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationDefinition.cs`
- Test: `Unity/Assets/Tests/EditMode/Stations/StationIdsTests.cs`

**Interfaces:**
- Produces: `Gamelab.PhysicalEntities.Stations.StationIds` (string constants, ports verbatim from `Src/PhysicalEntities/Stations/StationIds.cs`, already C# 9-safe), `.StationDefinition` (Core data: id, display name, description — the catalog-relevant subset of the original `StationConfig`, which lived in `Gamelab.Config`/JSON and isn't ported per Scope decisions #5).

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationIds.cs
namespace Gamelab.PhysicalEntities.Stations
{
    public static class StationIds
    {
        public const string Cannon = "Cannon";
        public const string BulletRack = "BulletRack";
        public const string Conveyor = "Conveyor";
        public const string UpgradedComponentConveyor = "UpgradedComponentConveyor";
        public const string BulletConveyor = "BulletConveyor";
        public const string Workbench = "Workbench";
        public const string AutoWorkbench = "AutoWorkbench";
        public const string Counter = "Counter";
        public const string SpeedLever = "SpeedLever";
        public const string Resource = "Resource";
        public const string Component = "Component";

        public static string GetComponentResourceId(string componentId)
        {
            return Component + componentId;
        }

        public static string GetResourceStationId(string resourceId)
        {
            return Resource + resourceId;
        }

        public static string GetStationResourceId(string stationId)
        {
            return stationId.Split(Resource)[1];
        }

        public static string GetStationComponentId(string stationId)
        {
            return stationId.Split(Component)[1];
        }

        public static bool IsResourceStationId(string stationId)
        {
            return stationId.StartsWith(Resource);
        }

        public static bool IsComponentStationId(string stationId)
        {
            return stationId.StartsWith(Resource + Component);
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Core/PhysicalEntities/Stations/StationDefinition.cs
namespace Gamelab.PhysicalEntities.Stations
{
    // Catalog-relevant subset of the original Gamelab.Config.StationConfig (JSON-loaded,
    // not ported — Scope decisions #5). Shop pricing/icon fields aren't included since
    // Src/Items/ (inventory) and Src/Services/Shop/ aren't ported either.
    public sealed class StationDefinition
    {
        public string StationId { get; }
        public string Name { get; }
        public string Description { get; }

        public StationDefinition(string stationId, string name, string description)
        {
            StationId = stationId;
            Name = name;
            Description = description;
        }
    }
}
```

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/EditMode/Stations/StationIdsTests.cs
using NUnit.Framework;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class StationIdsTests
    {
        [Test]
        public void GetComponentResourceId_PrefixesWithResourceAndComponent()
        {
            string id = StationIds.GetComponentResourceId("BasicCasing");

            Assert.AreEqual("ResourceComponentBasicCasing", id);
            Assert.IsTrue(StationIds.IsComponentStationId(id));
        }

        [Test]
        public void GetResourceStationId_PrefixesWithResource()
        {
            string id = StationIds.GetResourceStationId("Coal");

            Assert.AreEqual("ResourceCoal", id);
            Assert.IsTrue(StationIds.IsResourceStationId(id));
            Assert.AreEqual("Coal", StationIds.GetStationResourceId(id));
        }
    }
}
```

- [ ] **Step 2: Confirm compile failure, create the two source files, recompile, run tests, confirm pass**

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Port station ids and definition data to Gamelab.Core"
```

### Task 10: Station Runtime — `StationCatalogAsset`, `StationRuntime` base, `CounterRuntime`

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationCatalogAsset.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationRuntime.cs`
- Create: `Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/CounterRuntime.cs`
- Test: `Unity/Assets/Tests/PlayMode/Stations/StationRuntimeSpawnTests.cs`

**Interfaces:**
- Consumes: `IInteractable`, `IPickable`, `IItemProvider`, `IItemReceiver`, `IUpdatable` (Tasks 1-2), `Item` (Task 1), `StationDefinition`/`StationIds` (Task 9).
- Produces: `Gamelab.PhysicalEntities.Stations.StationCatalogAsset` (`ScriptableObject` catalog — the "one station" the work order's deliverable list asks for, matching `CONVENTIONS.md`'s "`StationRegistry` becomes a `ScriptableObject`-based catalog"), `.StationRuntime` (abstract `MonoBehaviour` base implementing `IInteractable`, `IPickable`, `IItemProvider`, `IItemReceiver`, `IUpdatable`), `.CounterRuntime` (concrete, the vertical-slice station — simplest concrete station in `Src/`, zero sound/VFX/shop coupling unlike `Workbench`/`CannonStation`).

`AbstractStation` in `Src/` creates its own physics body in its constructor and loads sounds; this port drops both (physics body creation moves to a `RequireComponent`/`Initialize` pattern like `BulletRuntime`/`EnemyRuntime`, sound calls dropped per Scope decisions #4). `AbstractGrabbable`/grab behavior is **not** ported (`AbstractStation` in `Src/` extends `AbstractGrabbable`, which lives in `Src/PhysicalEntities/Structures/` — not this agent's folder, and not touched per the "do not touch" list's spirit of leaving physics/movement to A1). `StationRuntime` here implements `IInteractable`/`IPickable`/`IItemProvider`/`IItemReceiver`/`IUpdatable` directly without a grab dependency:

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationCatalogAsset.cs
using System;
using System.Collections.Generic;
using Gamelab.PhysicalEntities.Stations;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    [Serializable]
    public class StationCatalogEntry
    {
        public string stationId;
        public string displayName;
        public string description;
        public StationRuntime prefab;
    }

    [CreateAssetMenu(menuName = "Gamelab/Stations/Station Catalog", fileName = "StationCatalog")]
    public class StationCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<StationCatalogEntry> entries = new List<StationCatalogEntry>();

        public StationCatalogEntry Get(string stationId)
        {
            foreach (StationCatalogEntry entry in entries)
            {
                if (entry.stationId == stationId)
                {
                    return entry;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(stationId), stationId, "Unknown station id in catalog.");
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationRuntime.cs
using System.Collections.Generic;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class StationRuntime : MonoBehaviour, IInteractable, IPickable, IItemProvider, IItemReceiver, IUpdatable
    {
        private const float TimeUntilKick = 0.5f;

        public string StationId { get; private set; }
        public Item HeldItem { get; set; }
        public Rigidbody2D PhysicsBody { get; private set; }

        public Vector2 Position
        {
            get => PhysicsBody.position;
            set => PhysicsBody.position = value;
        }

        protected readonly List<ProviderTicket> providerQueue = new List<ProviderTicket>();
        protected readonly List<ConsumerTicket> consumerQueue = new List<ConsumerTicket>();

        public virtual void Initialize(StationCatalogEntry catalogEntry)
        {
            StationId = catalogEntry.stationId;
            PhysicsBody = GetComponent<Rigidbody2D>();
            PhysicsBody.bodyType = RigidbodyType2D.Static;
        }

        public virtual Item PeekNextItem() => HeldItem;

        public virtual bool CanReceiveItem(Item item, IItemProvider source) => false;

        public virtual bool TryProvideItem(out Item item, IItemReceiver consumer = null)
        {
            item = HeldItem;
            if (HeldItem != null && CanProvideItem(consumer))
            {
                HeldItem = null;
                if (consumer != null)
                {
                    consumerQueue.RemoveAll(t => t.Consumer == consumer);
                }

                return true;
            }

            item = null;
            return false;
        }

        public virtual void ReceiveItem(Item item, IItemProvider source)
        {
            HeldItem = item;
        }

        public virtual bool CanProvideItem(IItemReceiver consumer)
        {
            return HeldItem != null && IsConsumerFirstInLine(consumer);
        }

        public void PingPushIntent(IItemProvider source, float dt)
        {
            ProviderTicket existing = providerQueue.Find(t => t.Provider == source);
            if (existing != null)
            {
                existing.TimeSinceLastPing = 0f;
                return;
            }

            providerQueue.Add(new ProviderTicket(source));
        }

        public void PingPullIntent(IItemReceiver consumer, float dt)
        {
            if (consumer == null)
            {
                return;
            }

            ConsumerTicket existing = consumerQueue.Find(t => t.Consumer == consumer);
            if (existing != null)
            {
                existing.TimeSinceLastPing = 0f;
                return;
            }

            consumerQueue.Add(new ConsumerTicket(consumer));
        }

        protected bool IsConsumerFirstInLine(IItemReceiver consumer)
        {
            // Src/PhysicalEntities/Stations/AbstractStation.cs special-cases
            // "consumer is Player" to always be first in line — Player isn't ported
            // here (Scope decisions #2), so that carve-out is dropped; a real Player
            // implementing IPlayerActor doesn't participate in this FIFO queue check
            // at all in this port, it's purely for provider/receiver stations.
            return consumerQueue.Count == 0 || consumerQueue[0].Consumer == consumer;
        }

        public virtual void Update(float dt)
        {
            for (int i = providerQueue.Count - 1; i >= 0; i--)
            {
                providerQueue[i].TimeSinceLastPing += dt;
                if (providerQueue[i].TimeSinceLastPing > TimeUntilKick)
                {
                    providerQueue.RemoveAt(i);
                }
            }

            for (int i = consumerQueue.Count - 1; i >= 0; i--)
            {
                consumerQueue[i].TimeSinceLastPing += dt;
                if (consumerQueue[i].TimeSinceLastPing > TimeUntilKick)
                {
                    consumerQueue.RemoveAt(i);
                }
            }
        }
    }
}
```

```csharp
// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/CounterRuntime.cs
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.PhysicalEntities.Stations
{
    // Vertical-slice concrete station: Src/PhysicalEntities/Stations/Counter.cs is the
    // simplest concrete station (one override, no sound/particle/shop coupling unlike
    // Workbench/CannonStation), so it's the one this agent ports behaviorally.
    public class CounterRuntime : StationRuntime
    {
        public override bool CanReceiveItem(Item item, IItemProvider source)
        {
            return HeldItem == null;
        }
    }
}
```

- [ ] **Step 1: Write the failing PlayMode test**

```csharp
// Unity/Assets/Tests/PlayMode/Stations/StationRuntimeSpawnTests.cs
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class StationRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogEntry_CounterAcceptsOneItemAtATime()
        {
            var catalog = ScriptableObject.CreateInstance<StationCatalogAsset>();
            var entry = new StationCatalogEntry { stationId = StationIds.Counter, displayName = "Counter" };
            typeof(StationCatalogAsset).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(catalog, new List<StationCatalogEntry> { entry });

            var go = new GameObject("Counter");
            go.AddComponent<Rigidbody2D>();
            var counter = go.AddComponent<CounterRuntime>();
            counter.Initialize(catalog.Get(StationIds.Counter));

            yield return null;

            Assert.AreEqual(StationIds.Counter, counter.StationId);
            Assert.IsTrue(counter.CanReceiveItem(new Item("Coal"), null));

            counter.ReceiveItem(new Item("Coal"), null);
            Assert.IsFalse(counter.CanReceiveItem(new Item("Coal"), null), "Counter should hold at most one item.");

            Assert.IsTrue(counter.TryProvideItem(out Item provided));
            Assert.AreEqual("Coal", provided.Id);

            Object.Destroy(go);
        }
    }
}
```

- [ ] **Step 2: Confirm compile failure, create the three production files, recompile, run PlayMode tests, confirm pass**

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets
git commit -m "Port station catalog and Counter runtime (one-station vertical slice)"
```

---

## Milestone 5: Breadth (stretch — time/budget permitting, in this priority order)

The Wave A gate only requires Tasks 1-10 above (interfaces, one bullet combo, one enemy, plus the station slice the work order's deliverable list asks for). These remaining tasks widen coverage toward the ~60-file full breadth the work order calls "the eventual goal." Each is independently gate-safe to stop after — do them in order, commit after each, and stop whenever time runs out; report exactly how far breadth got.

### Task 11: Remaining bullet casings/propellants/projectiles as `ScriptableObject` assets

**Files:** one `BulletComponentAsset` subclass per remaining `Src/PhysicalEntities/Bullets/Components/{Casings,Propellants,Projectiles}/*.cs` file (`BurstCasing`, `EnemyCasing`, `RapidFireCasing`, `ScatterCasing`, `FrangibleProjectile`, `MatryoshkaProjectile`, `PiercingProjectile`, `BoomerangPropellant`, `HeavyPropellant`, `HomingPropellant`), following the exact pattern established in Task 5 (`BasicCasingAsset`/`BasicPropellantAsset`/`BasicProjectileAsset`). Read each `Src/` file; port only the parts that don't need sound/VFX/physics-world-query services that don't exist yet (same rule as Task 5's `BasicCasingAsset`); note any dropped behavior with a `ponytail:` comment the same way Task 5 does. `HomingPropellant`/`BoomerangPropellant` need the physics-world target query this plan explicitly parked in Task 3 (`BulletTargetingHelper`'s non-ported half) — port their data/config shape (component id, tunable fields) but leave `OnUpdate`'s actual homing/boomerang steering as a documented gap until that query API exists.

- [ ] For each of the 10 components: write an EditMode or PlayMode test asserting its `Type`/`ComponentId` and any ported behavior, confirm it fails, implement, confirm it passes, commit individually (10 separate small commits, not one giant one — keeps this stretch task abandonable mid-way without losing a clean history).

### Task 12: Remaining enemy type data (`Rifle`/`TutorialRifle` stats only, not the horse/rider animation state machine)

**Files:** `Unity/Assets/Scripts/Runtime/Enemies/RifleEnemyRuntime.cs`, `TutorialRifleEnemyRuntime.cs`.

Per Scope decisions #4, `Enemy.cs`'s horse sprite/animation/sound state machine is not ported. What's portable without those services: the shoot-cooldown timer, `ExecuteFire`'s spread/angle math (pure), and `TryShoot`'s state gating simplified to "not on cooldown." Read `Src/Enemies/Types/Enemy.cs` and `TutorialEnemy.cs`, port the cooldown/fire-angle math only, as `RifleEnemyRuntime : EnemyRuntime` / `TutorialRifleEnemyRuntime : EnemyRuntime`, each with a test proving cooldown gating and fire-angle computation (not sprite/sound). `Src/Config/GameplayConfig.cs` defaults: `RifleMaxSpeed` 700, `RiflePreferredDistance` 30, `EnemyShootCooldown` 4, `TutorialEnemyShootCooldown` 6, `TutorialEnemyHealth` 1, `EnemyShootSpread` 0.2, `EnemyFleeDelay` 0.75.

- [ ] Write failing tests for cooldown gating and fire-angle math for both types, confirm fail, implement, confirm pass, commit per enemy type.

### Task 13: Remaining stations as catalog entries

**Files:** one `StationRuntime` subclass per remaining `Src/PhysicalEntities/Stations/*.cs` concrete station (`Workbench`, `AutoWorkbench`, `SpeedLever`, `Cannon/CannonStation`, `Cannon/AmmoRack`, `Conveyors/*`, `Resources/*`), following the `CounterRuntime` pattern from Task 10. Port only the item-provider/receiver/interact contract logic, dropping sound/VFX/shop calls per Scope decisions #3/#4 (each with its own `ponytail:` note pointing at what's dropped and why, same style as Task 5's `BasicCasingAsset`). `CannonStation` additionally implements `ICannonSeat`/`IBulletEmitter` (Task 2/5's interfaces) — port its cooldown/fire-trigger logic using `BulletRuntime.Spawn` from Task 5, but not its sound calls or `SeatedPlayer` tracking (needs A4's `Player`/`IPlayerActor` wiring, which is a stub until A4 lands).

- [ ] For each remaining station: write a test for its provider/receiver contract (or fire logic for `CannonStation`), confirm fail, implement, confirm pass, commit per station.

---

## Final task: Whole-branch review and Wave A gate write-up

### Task 14: Final review pass

- [ ] **Step 1: Run the full EditMode and PlayMode suites one more time from a clean state**, confirm every test (including all breadth-task tests if Milestone 5 ran) shows `result="Passed"` in the results XML — don't rely on exit code or log tail alone, per `CONVENTIONS.md`'s warning.
- [ ] **Step 2: `dotnet build Src/Gamelab.csproj`** to reconfirm the MonoGame reference build is unaffected.
- [ ] **Step 3: `git status` and `git log --oneline` across the whole branch** — confirm no untracked `.meta` files were missed (per `CONVENTIONS.md`'s `git add Unity/Assets` guidance) and the commit history tells a coherent story task-by-task.
- [ ] **Step 4: Use `superpowers:requesting-code-review`** for a whole-branch review of everything committed on `port/domain` since it diverged from the Phase 0 merge commit (`6c13b36`). Address any findings with the same task-reviewer/fix-loop pattern used per-task; if a finding is a deliberate scope cut already documented above, rule it "as designed" in the SDD ledger rather than re-litigating it.
- [ ] **Step 5: Do not merge.** Report back to the coordinator per the work order's "Report back" instructions — this plan's execution doesn't include integration into `main`.
