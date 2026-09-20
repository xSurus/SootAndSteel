# A2 follow-up: domain/catalog layer gap closure

Branch `port/domain2` (from main @ 5d8d24d). Closes the gaps left by the first-pass plan
(`2026-09-18-a2-domain-catalog-layer-plan.md`). `Src/` is read-only. Conventions: `Unity/CONVENTIONS.md`.
Verify with the headless Unity CLI (never `-quit` with `-runTests`; read the results XML).

## Design rulings (made up front)

1. **Units.** Bullet/enemy stats and movement profiles stay in Src pixel units so the tables match Src
   side by side. `Gamelab.Core.WorldUnits` (`PixelsPerMeter = 100`, `ToMeters`, `ToPixels`, Src
   `GameplayConfig.PixelsPerMeter`) is the single conversion point. Conversion happens only where a
   value crosses into Unity physics: collider radius, `linearVelocity`, `EnemyMovementProfile.ToMeters()`.
2. **Per-bullet state.** Components stay stateless ScriptableObjects (shared assets). Per-bullet state lives
   on `BulletRuntime`: `GetState<T>(component)` returns a lazily created per-bullet `T` keyed by component
   asset. "Root effect" (Src `IsRootEffect`) becomes `bullet.IsRoot(component)`; a child bullet spawned by
   component X carries X in its non-root set. Children are fresh spawns from the same definition
   (`BulletRuntime.SpawnChild`), matching Src's `ChildTemplate` (a copy taken before any OnCreate).
3. **Spawn phases.** `Spawn` = create phase (OnCreate of all three components, stats final) then spawn phase
   (OnSpawn). A child with `delay > 0` runs only the create phase, has its body `simulated=false`, and runs
   the spawn phase from `Tick` once the delay elapses (Src `PendingBullet`).
4. **Ticking.** `BulletRuntime.FixedUpdate` calls `Tick(Time.fixedDeltaTime)`; `StationRuntime` gets a
   parameterless `Update()` calling `Update(Time.deltaTime)`. Lifetime (Src BasicPropellant.OnUpdate) is
   enforced in `BulletRuntime.Tick`, not in a component, so it holds for any propellant.
5. **Collisions.** Bullets and enemies are Rigidbody2D + trigger `CircleCollider2D` built through the existing
   `PhysicalEntity.ConfigureAsDynamicCircle`. Src enemy fixtures are sensors, so the trigger model matches.
   `BulletRuntime.OnTriggerEnter2D` mirrors Src `BulletEntity.OnCollision` (IDamageable, per-target hit
   cooldown, pierce count).
6. **Dropped behaviour policy.** Record in code comments plus the final report.
   `BulletStats.Color` (draw-only), off-screen enemy removal (needs the map/camera origin of B1),
   `TryReserveSideAttackSlotOnSide` (decided in the breadth task).

## Tasks (one commit each, review after each)

- T1 Units: `WorldUnits` + tests; `EnemyMovementProfile.ToMeters()`; `EnemyMovementController` works in
  meters; PlayMode tests for the controller (item 5, 6a).
- T2 Bullet core: retype slots to `BulletComponentAsset`, validation with a clear error, units, FixedUpdate
  tick, lifetime, `PhysicalEntity` reuse, Destroy on deactivate (items 1a, 3, 5, 7).
- T3 Collisions: bullet + enemy colliders, `OnTriggerEnter2D`, pierce, hit cooldown; PlayMode test with real
  physics (item 4). Off-screen enemy removal decision.
- T4 Per-bullet state and child spawning: `GetState`, `IsRoot`, `SpawnChild`, delayed spawn (item 2).
- T5 Bullet parts, simple: Enemy/Scatter/Burst/RapidFire casings, Heavy propellant, Piercing/Frangible
  projectiles (item 1b).
- T6 Bullet parts, stateful: Boomerang, Homing, Matryoshka + targeting helper (item 1b).
- T7 Stations: tick wiring, PlayMode tests for queue kick and FIFO (items 3, 6b).
- T8 Enemy ammo data: 9 definitions + level-gated spawn options in Core (item 8).
- T9 Breadth (best effort): remaining enemy/station types, one category at a time.
- Final: whole-branch review (most capable model), one fix wave, scoped re-review, full EditMode + PlayMode
  runs, `git diff main -- Src/` empty, `dotnet build Src/Gamelab.csproj`.
