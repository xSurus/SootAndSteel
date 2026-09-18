# Unity Port Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port Team Snow from MonoGame/.NET to a Unity project that builds, passes its ported test suite, and matches the current game's feature parity (per `GameInstructions.md`), using a wave of subagents working in parallel git worktrees.

**Architecture:** A new `Unity/` project lives alongside the existing `Src/` MonoGame project in this same repo, so the MonoGame build stays runnable as a live reference throughout the port. Phase 0 (below, fully detailed) builds the empty Unity project, its assembly/test scaffolding, and one proven vertical slice, so every later subagent inherits real conventions instead of guessing them. Phases 1-3 are subsystem work orders dispatched to subagents in three dependency-ordered waves; each subagent runs `superpowers:writing-plans` against its own work order to produce its own bite-sized task list before implementing.

**Tech Stack:** Unity 6 LTS, C#, URP 2D renderer, Unity Input System, Unity Test Framework (NUnit), official Unity FMOD integration, Unity Tilemap.

**Spec:** `docs/superpowers/specs/2026-09-18-unity-port-design.md`

## Global Constraints

- Unity 6 LTS, C#, URP 2D renderer (per spec "Target")
- Desktop build targets only: Windows, macOS, Linux (per spec "Target")
- FMOD stays the audio middleware; use the official Unity FMOD integration, reuse `FmodProject/` and `.bank` files unchanged (per spec "Target")
- No online multiplayer in this plan; don't design it away, but don't build it (per spec "Non-goals")
- No gameplay rebalancing, no storefront work (per spec "Non-goals")
- Local co-op (join screen, multi-controller, dynamic player spawn) must match or beat current behavior before a wave is considered done (per spec "Risk areas" #5)
- The MonoGame build (`Src/Gamelab.csproj`) stays runnable throughout the port as the side-by-side reference (per spec "Testing & verification strategy")
- `GameInstructions.md` is the canonical manual parity checklist (per spec "Testing & verification strategy")
- Pixel art must use point filtering and correct pixels-per-unit on import, not Unity's blurry defaults (per spec "Risk areas" #4)

---

## Repo & hosting decision (action needed from you before Phase 0)

This repo's remote is ETH's course GitLab (`gitlab.inf.ethz.ch`), and its CI (`.gitlab-ci.yml`) is locked course infrastructure that disappears when the course ends. This plan assumes the Unity port develops in a new top-level `Unity/` folder in *this same repo* for now, so history and the MonoGame reference build stay together. Before Wave A starts, you need to decide and set up the long-term home (e.g. a new GitHub repo you or the team owns) — creating that account/repo is a real-world action outside what an agent should do unprompted. Once it exists, Phase 0's CI task targets it.

---

## Phase 0: Foundation (fully detailed, do this yourself or with one subagent, not parallelized)

### Task 1: Install Unity and create the project

**Files:**
- Create: `Unity/` (new Unity project root, sibling to `Src/`)

This step needs a human at a GUI once (Unity Editor requires interactive license acceptance on first install); it is not agent-automatable.

- [ ] **Step 1: Install Unity Hub and Unity 6 LTS**

Download Unity Hub from unity.com, sign in, install the latest Unity 6 LTS release through Hub.

- [ ] **Step 2: Create the project**

In Unity Hub: New Project → "Universal 2D" template → name `Unity`, location = repo root (so it creates `<repo-root>/Unity/`).

- [ ] **Step 3: Verify it opens**

Open the project in the Editor once, confirm the default scene loads with no console errors, then close the Editor.

- [ ] **Step 4: Commit the raw scaffold**

```bash
git add Unity/Assets Unity/Packages Unity/ProjectSettings Unity/Unity.sln 2>/dev/null
git commit -m "Scaffold empty Unity project for the port"
```

(`git add` will silently skip any paths that don't exist yet depending on Unity's exact output — that's fine, Task 2 adds the `.gitignore` that keeps `Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/` out.)

### Task 2: Configure project settings for pixel art and reviewable diffs

**Files:**
- Create: `Unity/.gitignore`
- Modify: `Unity/ProjectSettings/EditorSettings.asset`
- Modify: `Unity/ProjectSettings/ProjectSettings.asset`

- [ ] **Step 1: Add the Unity gitignore**

```gitignore
# Unity/.gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]serSettings/
*.csproj
*.sln
*.tmp
.vs/
```

- [ ] **Step 2: Force text serialization and visible meta files**

In the Editor: Edit → Project Settings → Editor → Asset Serialization Mode = "Force Text"; Version Control Mode = "Visible Meta Files". Save (this writes `EditorSettings.asset`).

- [ ] **Step 3: Set default sprite import to pixel-perfect**

Edit → Project Settings → Editor → Default Behavior Mode stays 2D (already set by the template). Then, for the texture import defaults: Project Settings → Graphics is not where sprite defaults live in Unity — instead, when Task 5+ agents import Team Snow's actual sprite sheets, each import must be set to Filter Mode = Point (no filter), Compression = None, and Pixels Per Unit matching the sprite's tile size (e.g. 16 or 32, matching `Content/Ice_Tile.png` etc.). Record this rule in `CONVENTIONS.md` (Task 6) rather than trying to set a single global default, since different sprite sheets in this project may use different tile sizes.

- [ ] **Step 4: Commit**

```bash
git add Unity/.gitignore Unity/ProjectSettings
git commit -m "Configure Unity project for pixel art and reviewable diffs"
```

### Task 3: Verify the reference MonoGame build still runs unmodified

**Files:** none (verification only)

- [ ] **Step 1: Build and run the existing MonoGame game**

```bash
dotnet run --project Src/Gamelab.csproj
```

Expected: the current game launches exactly as before Phase 0 started. This confirms the Unity scaffold hasn't disturbed the reference build — the reference build's continued health is a Global Constraint, not a one-time check, so re-run this after every later wave's integration too.

- [ ] **Step 2: Close the game, no commit needed (nothing changed)**

### Task 4: Assembly definitions

**Files:**
- Create: `Unity/Assets/Scripts/Core/Gamelab.Core.asmdef`
- Create: `Unity/Assets/Scripts/Runtime/Gamelab.Runtime.asmdef`
- Create: `Unity/Assets/Tests/EditMode/Gamelab.Tests.EditMode.asmdef`

`Core` holds engine-agnostic logic (services, catalogs, domain interfaces — the code that ports from `Src/Services`, `Src/Enemies/Core`, `Src/PhysicalEntities/Interfaces`, etc. with light adaptation). `Runtime` holds MonoBehaviours and anything that touches `UnityEngine` types directly. `Tests.EditMode` references `Core` only, since pure-logic tests don't need a scene.

- [ ] **Step 1: Create the folders and asmdef files**

`Unity/Assets/Scripts/Core/Gamelab.Core.asmdef`:
```json
{
    "name": "Gamelab.Core",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

`Unity/Assets/Scripts/Runtime/Gamelab.Runtime.asmdef`:
```json
{
    "name": "Gamelab.Runtime",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": []
}
```

`Unity/Assets/Tests/EditMode/Gamelab.Tests.EditMode.asmdef`:
```json
{
    "name": "Gamelab.Tests.EditMode",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
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

- [ ] **Step 2: Verify the Editor compiles with no errors**

Open Unity, let it recompile, confirm the Console shows zero errors.

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets/Scripts Unity/Assets/Tests
git commit -m "Add Core/Runtime/Tests assembly definitions"
```

### Task 5: Vertical slice — port `RandomService` and its test

This proves the whole pipeline (asmdefs, Test Framework, NUnit compatibility) on the smallest real piece of the codebase: `Src/Services/Random/` has zero MonoGame dependencies, so it's a near-verbatim copy.

**Files:**
- Create: `Unity/Assets/Scripts/Core/Services/Random/IRandomService.cs`
- Create: `Unity/Assets/Scripts/Core/Services/Random/RandomService.cs`
- Test: `Unity/Assets/Tests/EditMode/RandomServiceTests.cs`

**Interfaces:**
- Produces: `Gamelab.Services.Random.IRandomService` with `double SampleGaussian(double mu, double sigma)`, and its implementation `Gamelab.Services.Random.RandomService`. Later agents porting anything that consumes `IRandomService` (e.g. enemy spawn jitter) depend on this exact signature.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/EditMode/RandomServiceTests.cs
using NUnit.Framework;
using Gamelab.Services.Random;

public class RandomServiceTests
{
    [Test]
    public void SampleGaussian_WithZeroSigma_ReturnsMu()
    {
        var service = new RandomService();

        double result = service.SampleGaussian(5.0, 0.0);

        Assert.AreEqual(5.0, result, 0.0001);
    }
}
```

(Sigma = 0 collapses the Gaussian sample to exactly `mu` regardless of the random draw — this makes the test deterministic instead of flaky.)

- [ ] **Step 2: Run it and confirm it fails**

Window → General → Test Runner → EditMode tab → Run All.
Expected: compile error, `RandomService`/`IRandomService` don't exist yet.

- [ ] **Step 3: Port the implementation**

```csharp
// Unity/Assets/Scripts/Core/Services/Random/IRandomService.cs
namespace Gamelab.Services.Random;

public interface IRandomService
{
    public double SampleGaussian(double mu, double sigma);
}
```

```csharp
// Unity/Assets/Scripts/Core/Services/Random/RandomService.cs
using System;

namespace Gamelab.Services.Random;

public class RandomService : IRandomService
{
    private System.Random random = System.Random.Shared;

    public double SampleGaussian(double mu, double sigma)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mu + sigma * (float)randStdNormal;
    }
}
```

This is a direct copy of `Src/Services/Random/IRandomService.cs` and `Src/Services/Random/RandomService.cs` — confirmation that MonoGame-independent logic ports with zero changes.

- [ ] **Step 4: Run it and confirm it passes**

Test Runner → EditMode → Run All. Expected: green.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets/Scripts/Core/Services/Random Unity/Assets/Tests/EditMode/RandomServiceTests.cs
git commit -m "Port RandomService as the Unity pipeline vertical slice"
```

### Task 6: Write `CONVENTIONS.md` for subsequent agents

**Files:**
- Create: `Unity/CONVENTIONS.md`

- [ ] **Step 1: Write the file**

```markdown
# Unity port conventions

Established by Phase 0 (docs/superpowers/plans/2026-09-18-unity-port-plan.md).
Every subsystem agent follows these so work merges without renegotiating basics.

## Assemblies
- `Gamelab.Core` (`Assets/Scripts/Core/`): engine-agnostic logic, `noEngineReferences: true`.
  Anything that can be tested without a scene goes here: services, catalogs,
  domain interfaces, pure data/calculation types.
- `Gamelab.Runtime` (`Assets/Scripts/Runtime/`): MonoBehaviours and anything
  touching `UnityEngine` directly (physics, input, rendering).
- `Gamelab.Tests.EditMode` (`Assets/Tests/EditMode/`): NUnit tests for `Core`.
  Add a matching `Gamelab.Tests.PlayMode` (references `Runtime`) the first
  time a subsystem needs a PlayMode test.

## Namespaces
Keep the original `Gamelab.*` namespace roots from `Src/` unchanged where the
type ports directly (e.g. `Gamelab.Services.Random`). This keeps side-by-side
diffing against the MonoGame source easy.

## Sprite import
Every sprite sheet import: Filter Mode = Point (no filter), Compression = None,
Pixels Per Unit = the sheet's native tile size in pixels (check the source
tile dimensions in `Src/Content/` before setting this — they are not all the
same size).

## Catalog-driven data (bullets, enemies, stations)
Port hand-registered catalogs (`EnemyCatalog`, `StationRegistry`, bullet
casing/propellant/projectile combinators) to `ScriptableObject` assets rather
than JSON + factory registration. Keep the same interface names
(`IInteractable`, `IDamageable`, etc.) from `Src/PhysicalEntities/Interfaces/`.

## Verification
Before merging any subsystem worktree: `Gamelab.Tests.EditMode` (and
`PlayMode` if present) must be green, and the relevant `GameInstructions.md`
checklist items for that subsystem must be manually re-checked against the
integrated build.
```

- [ ] **Step 2: Commit**

```bash
git add Unity/CONVENTIONS.md
git commit -m "Document Unity port conventions for subsystem agents"
```

### Task 7: CI for the Unity project

**Files:**
- Create: `.github/workflows/unity-tests.yml` (targets the new repo home from the "Repo & hosting decision" section above — adjust the path/trigger if you end up mirroring instead of moving)

- [ ] **Step 1: Write the workflow**

```yaml
# .github/workflows/unity-tests.yml
name: Unity Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      - uses: actions/cache@v4
        with:
          path: Unity/Library
          key: Library-${{ hashFiles('Unity/Assets/**', 'Unity/Packages/**', 'Unity/ProjectSettings/**') }}
          restore-keys: Library-
      - uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          projectPath: Unity
          testMode: EditMode
```

This uses `game-ci/unity-test-runner` (game.ci), the standard community action for headless Unity testing. It needs a Unity license activated as GitHub secrets — that's an account-level action for you to do once in the new repo's settings, not something an agent can do.

- [ ] **Step 2: Commit**

```bash
git add .github/workflows/unity-tests.yml
git commit -m "Add CI workflow for Unity EditMode tests"
```

### Phase 0 verification gate

- [ ] `Gamelab.Tests.EditMode` passes locally in the Unity Test Runner
- [ ] The MonoGame reference build still runs (`dotnet run --project Src/Gamelab.csproj`)
- [ ] `Unity/CONVENTIONS.md` exists and is committed
- [ ] CI workflow file is committed (activation of secrets is a manual follow-up once the repo's new home exists)

---

## Phase 1-3: Subsystem waves (multi-agent)

Each subsystem below is a **work order**, not a bite-sized task list — the
assigned subagent runs `superpowers:writing-plans` against its own work order
(referencing this plan and `Unity/CONVENTIONS.md`) to produce its detailed
task list, then implements via `superpowers:subagent-driven-development`.
This mirrors the repo's existing pattern of per-agent git worktrees under
`.claude/worktrees/`.

**Orchestration mechanics:**
- One worktree per subsystem, branched from the Phase 0 commit (or from the
  latest merged wave for Wave B/C agents): `git worktree add ../port-<subsystem> -b port/<subsystem>`
- Subsystems are scoped to avoid two agents touching the same folder in the
  same wave (see table below).
- After each agent finishes: `superpowers:requesting-code-review`, then merge
  to the port's integration branch, then run that wave's verification gate
  (below) against the merged result before starting the next wave.
- If two agents' worktrees conflict at merge time (e.g. both touched a shared
  `Core` interface), the coordinating session resolves it before either
  proceeds to sign-off — don't let conflict resolution happen inside a
  single agent's unreviewed worktree.

### Wave A (parallel, depends only on Phase 0)

| Agent | Subsystem | Source folders | Produces | Depends on |
|---|---|---|---|---|
| A1 | Physics & movement | `Src/PhysicalEntities/AbstractPhysicalEntity.cs`, movement-related `Src/Components/` | Rigidbody2D/Collider2D-based movement matching Aether.Physics2D behavior, retuned constants | Phase 0 only |
| A2 | Domain/catalog layer | `Src/PhysicalEntities/Bullets/`, `Src/Enemies/`, `Src/PhysicalEntities/Stations/`, `Src/PhysicalEntities/Interfaces/` | ScriptableObject-based catalogs for bullets/enemies/stations, `Gamelab.Core` interfaces ported | Phase 0 only |
| A3 | Audio | `Src/Services/Sound/` | `ISoundService` on the official Unity FMOD integration, soundbanks wired up | Phase 0 only |
| A4 | Input & local co-op | `Src/Input/`, `Src/Players/`, join-screen parts of `Src/Screens/` | Unity Input System + `PlayerInputManager`-based join/split/dynamic-spawn flow | Phase 0 only |

**Wave A verification gate** (each agent's own subsystem, plus the combined check once all four land):
- [ ] Each agent's `EditMode`/`PlayMode` tests pass
- [ ] A1: a test scene shows a physics body moving/colliding with parity to the MonoGame reference (side-by-side visual check)
- [ ] A2: catalog-driven bullet (casing+propellant+projectile combination) and one enemy spawn correctly from data, verified via a PlayMode smoke test
- [ ] A3: at least one sound event and one music/parameter binding plays correctly
- [ ] A4: two simulated controllers can join, split-screen assigns correctly, matches `GameInstructions.md`'s join-screen section
- [ ] MonoGame reference build still runs unmodified

### Wave B (depends on Wave A landing and being merged)

| Agent | Subsystem | Source folders | Produces | Depends on |
|---|---|---|---|---|
| B1 | Levels & map | `Src/Levels/`, `Src/Map/` | Unity Tilemap-based levels using the ported tile assets | A1 (physics colliders must exist for level geometry) |
| B2 | UI | `Src/Screens/`, `Src/UI/`, `Src/Components/*.cs` and `*.Generated.cs` (~23 files: HubOverlay, PauseOverlay, OptionsMenu, ToolTip, Slider, PostDeathDisplay, etc.) | Unity UI Toolkit rebuild of every screen/overlay, wired to real data | A2 (needs real data to bind to), A4 (needs input for navigation) |

This is the highest-risk wave (spec "Risk areas" #1) — expect it to take longest and to surface gaps in A2's data shape; treat those as normal integration feedback, not a Wave A failure.

**Wave B verification gate:**
- [ ] Each agent's tests pass
- [ ] B1: a full level loads with correct tile placement and working collision, matching the reference build's layout
- [ ] B2: every screen/overlay in `GameInstructions.md` is reachable and functionally matches the reference (data displayed correctly, buttons wired, tooltips show)
- [ ] Re-run Wave A's checklist items against the now-larger integrated build (regression check, not just new features)
- [ ] MonoGame reference build still runs unmodified

### Wave C: Integration, test parity, and playtest (single agent or you directly, not parallelized)

**Files:**
- Port remaining logic from `Tests/CICD.cs`, `Tests/Utils.cs`, `Tests/Example.cs` to `Unity/Assets/Tests/` (both EditMode and PlayMode as appropriate — NUnit syntax already matches, per Phase 0 Task 5's proof)
- Wire all Wave A/B subsystems into one playable build

- [ ] **Step 1: Merge all Wave B branches into the integration branch, resolve any conflicts**
- [ ] **Step 2: Port the remaining `Tests/` suite to Unity Test Framework**
- [ ] **Step 3: Run the full `GameInstructions.md` checklist end-to-end against the integrated Unity build**
- [ ] **Step 4: Run the MonoGame reference build side-by-side and diff any behavior that doesn't match; for each mismatch, decide fix-vs-intentional-deviation and record the decision**
- [ ] **Step 5: Full team playtest** — the original team plays the Unity build with real controllers, using `GameInstructions.md` as the test script, specifically confirming local co-op parity (spec "Risk areas" #5)
- [ ] **Step 6: Fix issues found in playtest, re-run the checklist**
- [ ] **Step 7: Commit, tag the port as complete** (`git tag unity-port-v1`)

### Final verification gate (port complete)

- [ ] All `Gamelab.Tests.EditMode` and `Gamelab.Tests.PlayMode` tests pass in CI
- [ ] Every `GameInstructions.md` item verified working in the Unity build
- [ ] Full-team playtest completed with no unresolved parity issues
- [ ] MonoGame reference build (kept alive this whole time) can now be archived/retired, since the Unity build is the new source of truth

---

## Self-review notes

- **Spec coverage:** every spec subsystem (physics, domain/catalogs, audio, UI, content pipeline, input, levels/map, config, tests) has an owning task or agent above; risk areas 1-5 each have an explicit verification checklist item; non-goals (online multiplayer, rebalancing, storefront) are excluded by the Global Constraints and not scheduled anywhere.
- **Why Phase 0 is fully detailed but Waves A-C are work orders:** bite-sized TDD steps need real file paths and signatures to point at. Those don't exist yet for UI/physics/etc. until Phase 0's conventions (asmdefs, ScriptableObject-vs-JSON decision, sprite import rule) are real — writing fake signatures now would violate the "no placeholders" rule harder than deferring detailed planning to each subsystem agent, which is also exactly what `subagent-driven-development` expects (each agent plans its own task).
