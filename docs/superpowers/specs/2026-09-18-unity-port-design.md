# Unity port: design

## Goal

Move Team Snow off MonoGame/.NET onto Unity so the team can iterate faster
(visual editor, scene tooling) and has a real path to Steam. Local co-op
(join screen, multi-controller, dynamic player spawn) must keep working at
least as well as it does today. Online co-op is planned as a big follow-up
project; this port should not make that harder, but does not implement it.

## Non-goals

- Online multiplayer / netcode (future work)
- Gameplay rebalancing or new features
- Steam store page, achievements, or other storefront work
- Rewriting content that survives the port unchanged (sprites, soundbanks,
  the FMOD project)

## Target

- Unity 6 LTS, C#, URP 2D renderer
- Desktop builds: Windows, macOS, Linux
- FMOD stays the audio middleware: swap the MonoGame wrapper
  (`FmodForFoxes`) for the official Unity FMOD integration; the
  `FmodProject/` and `.bank` files carry over unchanged

## Current architecture (what we're porting from)

Not an ECS: a hand-rolled OOP layer of abstract base classes
(`AbstractPhysicalEntity`, `AbstractStation`, `AbstractEnemy`) and domain
interfaces (`IInteractable`, `IDamageable`, `IGrabbable`, `IPickable`,
`IItemProvider`/`IItemReceiver`, `ICannonSeat`, ...), populated through
factories and catalogs (`EnemyFactory`/`EnemyCatalog`, `StationFactory`),
driven by a handful of injected services (`IShopService`, `ISoundService`,
`IBulletService`, `IVfxService`, `IAnimationService`, `IRandomService`).
Bullets are built by composing small interchangeable parts (casing +
propellant + projectile), which is effectively already the shape of a
Unity `ScriptableObject` catalog. UI is Gum.MonoGame + Myra, with codegen
(`*.Generated.cs`) for ~13 screens/overlays. ~19k lines across 247 files.

## What ports directly vs what gets rebuilt

| Area | Verdict | Notes |
|---|---|---|
| Domain logic/interfaces (bullets, enemies, stations, services) | Ports with light adaptation | Swap `Vector2`/`GameTime`/etc. for `UnityEngine` equivalents; business logic and interfaces stay |
| Physics (Aether.Physics2D) | Ports conceptually, retune constants | Both Aether and Unity 2D physics are Box2D-based; forces/restitution/friction need re-tuning, not redesign |
| Audio (FmodForFoxes) | Rebuild call sites, reuse content | Official Unity FMOD plugin has a different API surface; `ISoundService`/`ParameterBinding` shape carries over |
| UI (Gum.MonoGame + Myra) | Full rebuild | No migration path; ~23 files (screens, overlays, tooltips, sliders) redesigned in Unity UI Toolkit. Highest-risk, highest-effort area. |
| Content pipeline (.mgcb/Contentless) | Deleted, net simplification | Unity's native importers replace the whole pipeline step |
| Input (custom multi-controller/join-screen) | Rebuilt on Unity Input System | `PlayerInputManager` is built for exactly this (local split-join); expect less code than today, not more |
| Levels/Map (tile-based) | Ports to Unity Tilemap | Existing tile assets (Ice/Rail/Snow/Train) reused |
| Config/catalogs (JSON) | Recommend migrating to ScriptableObjects | Better fit for Unity's editor workflow than hand-maintained JSON + factory registration; Newtonsoft.Json remains available if a JSON path is preferred instead |
| Tests (`Tests.csproj`) | Ports to Unity Test Framework | Pure-logic tests (bullet stats, catalogs) port near-directly; anything coupled to the MonoGame game loop needs an adapter rewrite |

## Risk areas, ranked

1. **UI rebuild** — no automatic migration, ~23 files, biggest unknown in scope and time.
2. **Physics parity** — same underlying model (Box2D) but constants will drift; risk of "feels different" from playtesters even when nothing is logically wrong.
3. **FMOD Unity integration** — different API surface than `FmodForFoxes`; parameter bindings and event triggers get rewritten.
4. **Pixel-art import settings** — Unity defaults to bilinear filtering and arbitrary PPU; must be set to point filtering / correct pixels-per-unit per sprite sheet or art will look blurred/misscaled.
5. **Local co-op input parity** — split-join and controller assignment must match or beat current behavior before this is called done.

## Testing & verification strategy

- **Reference build stays alive.** The MonoGame build keeps running throughout the port. A ported feature isn't done until it's checked side-by-side against the reference and matches (or an intentional deviation is called out and approved).
- **Golden-path checklist.** `GameInstructions.md` is the existing feature list; it becomes the manual parity checklist, run after every integration checkpoint.
- **Automated tests.** Port `Tests/Tests.csproj` to Unity Test Framework (NUnit-based, EditMode + PlayMode). Pure-logic tests move first since they're highest-value/lowest-effort; add PlayMode tests for input, physics triggers, and UI flows as those subsystems land.
- **Final playtest.** Once all subsystems are integrated, a full playtest with the original team using `GameInstructions.md` as the test script, played on real hardware with multiple controllers to confirm local co-op parity.

## Multi-agent execution model

The repo already has a precedent for this: `.claude/worktrees/` holds prior
per-agent worktrees. Reuse that pattern (`superpowers:using-git-worktrees`,
`superpowers:subagent-driven-development`).

- One coordinating session (this one) plans, assigns, integrates, and runs
  cross-subsystem verification.
- Each subagent owns one subsystem in its own git worktree/branch, ports it,
  writes/updates its Unity tests, and reports back for review before merge.
- Subsystems are chosen to minimize cross-agent file contention: split along
  the existing top-level `Src/` folders, since those are already
  the project's real module boundaries.
- Integration checkpoints after each wave merge the finished worktrees back,
  then run the parity checklist against the combined build before the next
  wave starts.

Full phase breakdown, agent assignments, and task list are in the
implementation plan (`docs/superpowers/plans/`).
