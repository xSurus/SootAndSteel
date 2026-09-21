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

New test classes go under `Gamelab.Tests.*` (e.g. `Gamelab.Tests.Services`),
mirroring the type under test. Several Wave A agents are adding EditMode
tests in parallel; the shared namespace prefix keeps test class names from
colliding.

## C# language version constraints

This project's Unity assemblies are pinned to C# 9.0 targeting netstandard2.1
(confirmed via the generated `Gamelab.Core.csproj`/`Gamelab.Tests.EditMode.csproj`
and their `.rsp` files). This differs from the MonoGame source in `Src/`, which
targets net9.0 and uses modern C# features. When porting code, adapt for these
constraints:

- **No file-scoped namespace declarations** (`namespace Foo;`) — C# 10+ only.
  Use the traditional braced form (`namespace Foo { ... }`).
- **No `System.Random.Shared`** — .NET 6+ only. Use `new System.Random()` instead.
  Trade-off: `Random.Shared` is thread-safe and avoids correlated sequences when
  two instances are created in the same clock tick; `new Random()` has no such
  guarantee. Fine for a single long-lived instance (the common pattern here),
  worth a one-line comment if a ported type is instantiated more than once.
- **Check for other C# 10+/.NET 6+ syntax and APIs** before porting any file from `Src/`.
  Examples: certain record type features, raw string literals, generic math.
  This list isn't exhaustive; these two are the ones already hit.

### Found by A2 (domain/catalog layer)

Porting the domain/catalog layer (bullets, enemies, stations, physical-entity
interfaces) hit several more C# 10-12/.NET 6+ idioms in the MonoGame source that
don't work in C# 9.0/netstandard2.1. Add these to your mental model before
porting any file in this subsystem:

- **No primary constructors on non-record classes/structs** (C# 12).
  Examples: `class EnemySlotManager()`, `class Counter(Vector2 position) : AbstractStation(...)`,
  `class EnemyMovementController(Body physicsBody, EnemyMovementProfile profile)`.
  Rewrite as explicit constructor bodies instead.

- **No `record struct` / `readonly record struct`** (C# 10).
  Use a plain `readonly struct` with hand-written properties and constructor.
  Value equality isn't needed by anything this subsystem ports; if it becomes
  needed later, hand-write `Equals`/`GetHashCode`.

- **No collection expressions** (C# 12).
  Examples: `[1, 2, 3]`, `[]`, `[..existing]`.
  Use `new[] { ... }`, `new List<T> { ... }`, `new HashSet<T>()` instead.

- **No `ArgumentException.ThrowIfNullOrWhiteSpace` / `ArgumentNullException.ThrowIfNull`** (.NET 6/7 static helpers).
  Use explicit `if` guards:
  `if (string.IsNullOrWhiteSpace(x)) throw new ArgumentException(...)`
  `if (x == null) throw new ArgumentNullException(...)`

- **Verify `System.Linq.Enumerable.MinBy`/`MaxBy` before using them** (.NET 6).
  These aren't guaranteed present in Unity's netstandard2.1 BCL.
  Prefer a manual loop or `OrderBy(...).FirstOrDefault()` when in doubt.

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
See "Core vs. Runtime: where catalogs and interfaces live" below for which
assembly each piece belongs in.

## Core vs. Runtime: where catalogs and interfaces live

`Gamelab.Core` has `noEngineReferences: true`, so nothing that references
`UnityEngine` (including `ScriptableObject`) can live there. Split ported
catalogs and interfaces like this:

- **`Gamelab.Core`**: the pure data, calculation logic, and engine-free
  interfaces. Use `System.Numerics.Vector2` (available on netstandard2.1)
  anywhere the original MonoGame code used `Microsoft.Xna.Framework.Vector2`.
  Reason: `System.Numerics.Vector2` is engine-agnostic and testable without a
  scene, same as the rest of Core, whereas `UnityEngine.Vector2` drags a
  `UnityEngine` reference into an assembly whose whole point is not having
  one. Don't swap this back to `UnityEngine.Vector2` even though it's the more
  common Unity idiom.
- **`Gamelab.Runtime`**: the `ScriptableObject` catalog/wrapper assets
  themselves (`EnemyCatalog`, `StationRegistry`, bullet combinators), plus any
  interface whose signature needs a `UnityEngine` type. These reference the
  plain data types that live in Core.

## Headless Unity verification (no GUI needed)

You do not need Editor GUI access to verify compilation or run tests — use the
Unity CLI in batch mode. The binary on this machine (macOS) is at:

```
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity
```

(This is this machine's path, not a universal one. On Windows it's typically
`C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe`; on Linux,
`~/Unity/Hub/Editor/6000.3.24f1/Editor/Unity`. Adjust for your platform.)

**Compile-only check** (fast, use `-quit` freely here):

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath <project> -logFile <path>
```

Check exit code 0, the log ends with `Exiting batchmode successfully now!`, and
`grep -i "error CS"` on the log is empty.

**Running tests — do NOT combine `-quit` with `-runTests`.** They race in this
Unity version: batchmode exits before the test runner actually executes,
silently producing no results file and no test-runner trace in the log, while
still exiting 0. This looks like a passing run and isn't one. Use:

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath <project> -runTests -testPlatform EditMode \
  -testResults <path>.xml -logFile <path>.log
```

This takes noticeably longer (Unity quits on its own once tests finish). Always
confirm your test actually ran by checking the results XML contains a `<test-case>`
entry for your specific test with `result="Passed"`. Do not rely solely on the
log or exit code — both can be misleading.

Ignore any `[Licensing::Module] Error: Access token is unavailable` line in any
log — harmless, unrelated to compilation or tests.

## Verification

Before merging any subsystem worktree: `Gamelab.Tests.EditMode` (and
`PlayMode` if present) must be green, and the relevant `GameInstructions.md`
checklist items for that subsystem must be manually re-checked against the
integrated build.

When staging new files, use `git add Unity/Assets` (or run `git status`
afterward and check for stray untracked `.meta` files) instead of narrow
per-file/per-folder `git add` paths. Unity generates a `.meta` file for a
folder one level above the folder's own contents, so a `git add <folder>`
that only targets the folder's contents misses the parent folder's `.meta`
file, and it's easy for the omission to go unnoticed across four parallel
Wave A branches.

## FMOD native libraries

The Editor and PlayMode tests run real FMOD. Provenance and caveats:

- The natives are the same binaries the MonoGame build uses (repo-root
  `libfmodL.dylib`, `fmodL.dll`, `fmodstudioL.dll`, 2.02.19), copied under
  `Assets/Plugins/FMOD/platforms/{mac,win}/lib` and tracked by Git LFS. The
  vendored managed wrapper is 2.02.35. FMOD only enforces major.minor, so this
  runs today, but a header mismatch error means one side moved.
- `platforms/mac/lib/fmodstudioL.dylib` is patched: `install_name_tool
  -add_rpath @loader_path/.` plus an ad hoc `codesign`, so it finds the sibling
  `libfmodL.dylib`. Refreshing it from an FMOD download loses that patch.
- Editor only. The plugin metas enable Editor and disable Standalone. FMOD's
  own macOS build step expects `fmodstudio{L}.bundle`, so a macOS player build
  has no FMOD natives yet. Whoever does the build wave must install the
  official FMOD Unity plugin binaries or extend this setup.
- Windows files are untested (no Windows machine was available).
- `SourceBankPath` is `../Src/Content/soundbanks` (authoring-time read
  dependency on the MonoGame tree). Copy the banks into Unity before `Src/` is
  removed.
- No automated test can prove audible output. Play a scene with
  `SoundServiceRunner` in the Editor with speakers on to confirm.

## Bullet model

- A `BulletDefinitionAsset` holds an ordered list of `BulletComponentAsset`s. It needs at least one Casing, one Propellant and one Projectile. The same asset may not appear twice (state and root flags are keyed by asset).
- Component assets are stateless. Per-bullet data lives on the bullet: `bullet.GetState<T>(component)` creates it lazily.
- `bullet.IsRoot(component)` is true unless `MarkNonRoot` was called. `SpawnChild(spawner, pos, aim, delay, configure)` builds a fresh bullet from the same definition and faction with the spawner marked non-root, so a spawner does not recurse. `configure` runs before the child's create phase.
- Phases: create phase (OnCreate of all components in list order, then RefreshCollider), then spawn phase (OnSpawn of all, then RefreshCollider). With delay > 0 the child runs only the create phase, is not simulated, and `Tick` runs the spawn phase after the delay. Age and lifetime start after the spawn phase. Pending bullets get no OnUpdate.
- Units: `BulletStats` stay in MonoGame pixels (Speed, Size). Conversion to Unity meters happens only through `WorldUnits` at the physics boundary (velocity, collider radius).

## A2 breadth round: what is ported, what is deferred

Ported (logic, with tests). Presentation (sound, VFX, animation, draw, highlight, shop tooltips) is dropped everywhere and marked in code comments.

- `BulletItem`, `ComponentTraits`, `GridDirection` (Core). `BulletItem` has no colour, `IsEqual` or `GetEffects` (draw/guid only). `HasUpgrade` replaces `GetEffects().Any(!IsBasic)`.
- Workbench and AutoWorkbench: `WorkbenchCrafting` (Core rules and 2 s timing) plus `WorkbenchRuntime` / `AutoWorkbenchRuntime`.
- Conveyor, BulletConveyor, UpgradedComponentConveyor: `ConveyorRuntime` and subclasses, full `Update` state machine. Neighbours come from `IStationGrid`.
- `ComponentResourceStationRuntime` (station id `ResourceComponent<id>`; use `InitializeComponent`), `BulletRackRuntime` (Src class `BulletRack`, capacity 5, FIFO). Coal needs no class (see `ResourceStationRuntime`). Peek on the component station returns the `BulletItem` it would provide, where Src inherits `Item("Component<id>")`. Every conveyor filter treats the two the same.
- Rifle enemy: `RifleEnemyBrain` (Core state machine and timers) and `RifleEnemyRuntime` / `TutorialEnemyRuntime` (movement, firing, hit rule). Tutorial values come from `EnemyTuning` (Core), which holds the effective Src `GameplayConfig` values (class defaults overlaid with `gameplay.json`). Health and size for the rifle come from the enemy catalog entry, as before.
- Off-screen enemy removal: `EnemyCulling` rules plus `EnemyRuntime.Bounds` (`IWorldBounds`). With `Bounds == null` nothing is culled. The flee cull (Enemy.cs right edge) sits in `RifleEnemyRuntime`.
- Cannon logic: `CannonStationRuntime` (cooldown, reload from adjacent racks, fire, aim step via Core `CannonAim`). `AimAngle` is a field on the station, not the Rigidbody rotation (the station body is static).
- SpeedLever: `SpeedLeverRuntime`, `ITrainState`, `TrainSpeedSetting` (Core, namespace `Gamelab.Map.Train.State`, folder `Core/Map`), `TrainSpeedTuning` (speeds 150/300/600, burn 0.5/1/4 from `gameplay.json`; the class defaults are 1/1/1).
- `StationRuntime.IsConsumerFirstInLine` now returns true for `IPlayerActor` consumers, as Src does for `Player`.
- Classes that re-implement `IInteractable` hooks must re-declare `: IInteractable`, or interface-typed calls from the player hit the default no-op.
- `IInteractable` uses default interface methods. They work in the Editor (Mono) but are not yet exercised in a player build. Any new `StationRuntime` subclass that defines an interaction hook must re-declare `: IInteractable`.

Deferred (needs wave B). Each line is exactly what is missing and why:

- Cannon seat: `OnGrab` / `OnRelease`, `SeatedPlayer`, `SeatPosition`, `FindEjectPosition`, `IsTileFree`. Needs the player type (`SeatAt` / `UnseatFrom`) and map tile and `MapObjects` queries. The aim seam is `ICannonAimSource` (`GetMovement()`); the seat should set it. `CannonSlot` (Structures) is not ported.
- Map adjacency: `IStationGrid.GetAdjacentStation(position, direction)` must be supplied by the map for conveyors and the cannon. The map also has to call `Initialize`, set `Grid`, `Spawner` and `IsBeingHeld`.
- Enemy world: `IEnemyWorld` (`GetSlotAnchor`, `GetTargetPoint`, `MinX`, `MaxX`). `EnemyTrainSlot.GetAnchor` needs map bounds. Target semantics: nearest ShootHoleWall on the side (Top means Y < map centre Y), else the map bounds centre. Positions are pixels in Src's y-down frame. `Vector2Interop` flips Y for input only. The enemy layer keeps Src's frame in the Unity world, so the map/camera seam should flip once, not per call site.
- Enemy manager hook: Src `AbstractEnemy.TryShoot` is virtual and called by `EnemyManager`. `RifleEnemyRuntime.TryShoot` is a plain method; wave B should add a virtual on `EnemyRuntime`. `Configure` must run after `Initialize`.
- Train state: `ITrainState` (`VictoryLapActive`, `IsCoalOvenBurning`, `CurrentSpeed`, `SlowDownIfRunning`) and `SpeedLeverRuntime.Speeds` (the `TrainSpeedSetting.Set` shared with the train) come from wave B's `TrainState`.
- Bullet emission from a held item: `IBulletItemSpawner` (`CatalogBulletSpawner` is the default implementation, it needs a `BulletComponentCatalogAsset`). Src `InitialShooter is CannonStation or CannonSlot` is replaced by `BulletFaction.Player`. The cannon `Fired` event replaces `Events.FireCannonFired`.
- Shop and tooltips: ported in B2.2 (see "B2.2 hub and shop"). `BuyableStationWrapper` is now `BuyableOffer` plus `HubShopModel`; the station objects and their highlight stay with the world wave.
- Player-driven pieces: `OnPickup` / grab / highlight / snap-to-cell on stations; workbench sparks, light and the `isCrafting` flag driven by highlight. Need the player and UI.
- `BulletStats.Color` and the `BulletItem` colour: draw-only. The render wave derives it from the recipe.
- Sound, VFX, animation for all of the above (horse sound, neck bleed, blood, muzzle flash, conveyor belt animation, aim line).

## B1 levels and map: what is ported, what is deferred

Plan: `docs/superpowers/plans/2026-09-20-b1-levels-map-plan.md`.

### The single Y flip

The enemy, cannon and map layers all live in Src's pixel frame, Y down, in meters (`WorldUnits`, 100 px per meter). The train tile is 80 px = 0.8 m. No position, velocity or direction is ever negated in the simulation. Input is the exception: `Vector2Interop` negates stick Y once, converting screen-up into world-up. That is a screen-to-world conversion, not a second world flip.

The only flip is the display mirror in `Runtime/Map/MapSpace.cs`:

- `MapSpace.ApplyTo(Camera)` mirrors the projection in Y, so the Y-down world shows Y down on screen. `MirroredCamera` is a component that re-applies it every LateUpdate. A custom projection matrix stops Unity recomputing it when `orthographicSize` or the aspect changes, so any camera driver (B2's camera director, zoom, shake) must either keep `MirroredCamera` on the camera or call `ApplyTo` after each change.
- A mirrored projection reverses winding: fine for sprites and tilemaps with Cull Off, but anything with backface culling disappears. `Camera.ScreenToWorldPoint` and `ScreenPointToRay` must be checked against the custom projection matrix before mouse aiming is built on them (B2 note).
- The mirror also turns upright art upside down. `MapSpace.SpriteFlip` (tile transform) and `MapSpace.ApplyToSprite` (`flipY`) cancel that. Every new `SpriteRenderer` in the world must go through `ApplyToSprite`. This compensates the mirror, it is not a second coordinate flip.
- UI and world-space text are affected by a mirrored camera. Screen-space canvases are not. B2 should use screen-space canvases or a second unmirrored UI camera.

### Sprites and scale

Art lives in `Assets/Resources/Map/` (loaded by `MapSprites.Get`). `Assets/Editor/MapSpriteImportSettings.cs` applies the import rule (Point, no compression, PPU) by path; `MapSpriteImportTests` checks every importer. PPU is 100 for the 100 px train family (floor tiles, all walls) and the sprite's own width for everything else (rails 233, pine 847, hub sheets by width). Runtime scale is `MapSpace.SpriteScale(ppu, srcScale)` = `srcScale * ppu / 100`. Sort order is the feet Y in pixels.

### Facts found while porting

- `gameplay.json` overrides the C# defaults where the key matches a property name: `trainHeight` is 5 (default 6), so the train is 10 x 5 tiles and the left wall door rows are 1 and 2. `TrainTuning` holds the effective values. The json temperature keys lack the `Train` prefix and never bind, so the class defaults (3, 3, 6) apply.
- Mono and .NET agree bit for bit on `System.Random(seed)` sequences, but 13 of the 48 golden rows have a distance that differs from the .NET value by exactly one float ulp (the ulp size depends on magnitude: 0.00024 near 3000, 0.0078 near 67000). The golden tests compare distances with `Within(1).Ulps`.
- `GameplayScreen` and `HubScreen` use `Random.Shared` for tile variants, walls and scroller trees. B1 takes an injected `System.Random`, so those are deterministic per seed but not comparable with Src output.

### Ported

- Levels (`Core/Levels`): `LevelDefinition`, `SpawnEvent`, `LevelGenerationConfig`, `ProceduralLevelGenerator` (goldens produced by running the real Src generator), `ProceduralLevelProvider(playerCount, runSeed)`, `PlayerCountScaling`, `LevelCompletionWatcher.Update(distance, hasThreats)`, `SpawnSchedule` (the spawn consumption of Src `EnemyManager`), and `LevelRuntime` (Runtime) that raises `SpawnDue` and `LevelCompleted`.
- Train (`Core/Map`, `Core/Map/Train`): `TrainLayout` (geometry, wall specs, left wall tiles, spawn tiles, snap cell), `TrainGrid<T>`, `PatchField`, `TrainState` and `TrainStateTuning`, `WallHealth`, `WallSpriteNames`, `EnemyWorldMath`, `MapBounds`, `EnemyTrainSlot.GetAnchor`.
- Runtime train (`Runtime/Map`): `TrainMapRuntime` (Tilemap floor with A/B variants, snow and ice overlay, left wall Tilemap collider, one static box per boundary wall, station cell registry and snap), `ShootHoleWallRuntime` (health, repair, enemy bullet hits, damage sprites), `DoorWallRuntime` (sensor toggle), `TrainStateRuntime`.
- World scroller and hub: `WorldScrollerModel` and `WorldScrollerView`, `HubMapModel` (goldens from Src) and `HubMapView` (boundary, houses, stakes, prep train with its door).

### Seam implementations

- `IStationGrid`: `TrainMapRuntime.GetAdjacentStation(positionMeters, dir)`. Use `TrainMapRuntime.Attach(station)` to snap a station and set `Grid` on conveyors and cannons.
- `IEnemyWorld` and `IWorldBounds`: `TrainMapRuntime` too. Set `EnemyRuntime.Bounds` and `RifleEnemyRuntime` world to the map. `WorldBounds` defaults to MinX 0, MaxX 1920 (Src virtual screen). Slot anchors use `Layout.GetBounds()` and the tile size as top clearance. Target point is the nearest ShootHoleWall on the side by wall centre, else the train bounds centre.
- `ITrainState` and the shared `TrainSpeedSetting.Set`: `TrainStateRuntime` (`Speeds` is the one set, pass it to `SpeedLeverRuntime.Speeds`). `TrainMapRuntime.Link(TrainStateRuntime)` keeps `numberBreachedWalls` in step with wall breach and repair. Call it once per map.

### Deferred (exact seam and reason)

- Structures `CannonWagon`, `CannonSlot`, `CoalWagon`, `TrainNose`, and `AddDefaultStructures` (cannon wagon bounds). Need the cannon seat and player. Seam: assign `TrainLayout.CannonWagonBounds` when the wagon exists (`IsOnTrain` and `SnapCell` already read it).
- `TrainMap.LoadLayout`, `CaptureLayout`, `AddDefaultStationLoadout`. Need `StationFactory`, `StationSaveData` and `RunSession` (save system). Seam: create stations from `StationCatalogAsset` and call `TrainMapRuntime.Attach`.
- Wall interaction: `OnPickup`, `OnInteractHeld` (repair driven by a player), highlight, wall sound, smoke VFX, screen shake on breach, `DrawLightBatch`. Seam: `ShootHoleWallRuntime.Repair(dt)` and the `Breached` and `Repaired` events. Door: `OnInteract` sound and highlight; `Toggle()` is the seam. Door damage sprites are not used by Src walls either.
- Train visuals not drawn: wheels and their animation, the shadow rectangle, side wall strips and corner, patch and furnace lights, the depth function `RenderUtility.CalculateDepth` (replaced by Y-based sorting order).
- `TrainState` freeze sound and the temperature FMOD parameter. Seam: `Temperature / MaxTemperature`.
- Hub: shop offers. Data, restock and purchase rules are ported in B2.2 (`HubShopModel`, `BuyableOffer`, `ShopManager`). Still deferred: the world wave spawns the real stations from `HubShopModel.Offers` and listens to `Purchased`. `Hub.png` is only read for its size (the village rect); `Village_Enter` is unreferenced in Src. Both are imported for the render wave.
- `TutorialLevelProvider` (`Src/Tutorial`), `EnemyManager`, `CameraDirector`, player spawning glue (`GetFreeSpawnTile` exists in `TrainLayout`).
- `IInteractable` default interface methods on the wall and door runtimes are not needed until the player exists.

### Integration notes for wave B2 (UI and camera)

- Put the camera under `MirroredCamera` (or call `MapSpace.ApplyTo` after each size change). Camera position and zoom are in the Y-down world: to follow the train, use `TrainLayout.GetBounds()` divided by 100.
- HUD data: the HUD (`HudController`, see B2.3) reads `State.DistanceTraveled`, `actualSpeed`, `Temperature`, `MaxTemperature` and `LevelRuntime.Definition`. It does not read `CurrentSpeed` or wall health.
- `LevelRuntime.SpawnDue` is where the enemy manager hooks in. `LevelRuntime` does not tick the train state because `TrainStateRuntime` ticks itself in `Update`. Set `TrainStateRuntime.SelfTick = false` if B2 wants to drive `Tick(dt)` itself (for example on pause).
- Screen-space UI is not mirrored. World-space text is.

## B2.1 menus: what is ported, what is deferred

Plan: `docs/superpowers/plans/2026-09-21-b2-1-menus-plan.md`. Slice: main menu, pause overlay, options menu, controls overlay, in UI Toolkit, screen space only (the mirrored world camera does not affect it).

### Ported

- View models (Core, namespace `Gamelab.UI.ViewModels`, folder `Core/UI/ViewModels`): `MainMenuViewModel` (per-player selection, entries built by the owner), `OptionsViewModel`, `PauseMenuModel` (items Continue, Options, Controls, Exit; `IsPaused`, `ControlsOpen`, `Changed`, `ExitRequested`). `EnsurePlayer` on the main menu model now raises `OnSelectionChanged` when it registers a player (Src does not), so a view can show the first player without waiting for a move.
- `ISoundService` lives in Runtime (FMOD types), so `OptionsViewModel` takes the Core interface `IVolumeSettings`. `Gamelab.UI.Runtime.SoundVolumeSettings` adapts an `ISoundService`. Volume step default is 0.05 (`gameplay.json` `menuVolumeStep`).
- `MenuNavigator` (Core, `Gamelab.UI`) holds the Src input semantics and is tested with fake `IInputActions`: `TickMainMenu`, `TickOptions`, `TickPause`. It takes `Func<IReadOnlyList<IInputActions>>` and an `Action playSelect`. Up and Down move with the menu sound, Pickup confirms with the sound, Left and Right adjust volume without sound, Pause or Pickup closes options, Pause or Pickup closes controls (with sound). Pause flow follows `GameplayScreen.UpdatePauseMenu`: toggle check first (blocked while options are open), then options, then controls, then root.
- Views (Runtime, `Gamelab.UI.Runtime`): `MainMenuView`, `OptionsMenuView`, `PauseMenuView`, `ControlsOverlayView`, each a MonoBehaviour on a `UIDocument`. Controllers: `MainMenuController` and `PauseMenuController` own the models, views, one shared `PanelSettings` (`UiPanel.Create`: scale with screen size, 1920x1080, shrink, the same uniform scale as Gum) and tick the navigator in `Update`. Both have a `Configure(...)` with injected volume, players and sound, and a convenience overload that reads `SoundServiceRunner.Instance` and the `PlayerJoinManager` roster.
- UXML and USS are in `Assets/Resources/UI/` (`MainMenu`, `OptionsMenu`, `PauseMenu`, `ControlsOverlay`, shared `Common.uss`, `RuntimeTheme.tss`) and load through `UiResources`. Layout numbers come from the Gum project files (`Src/Content/GumProject`), not from the `.Generated.cs` files, which hold no layout. Gum unit codes used: dimension 0 absolute, 1 percent of parent, 3 percent of source file, 4 relative to children; position 0 from left, 1 from top, 2 percent width, 3 percent height, 4 from right, 6 from center X, 7 from center Y.
- Pause sets `Time.timeScale = 0` from `PauseMenuModel.Changed`, restores the value it saw when the pause began on unpause, disable and destroy, and re-applies it on re-enable while paused. The `TrainStateRuntime.SelfTick = false` fallback was not needed. `PlayerInputHandler` now ticks its directional repeater with `Time.unscaledDeltaTime`, because `deltaTime` is 0 while paused and hold-to-repeat would stop. Only the repeater uses it. A test proves the repeat under timeScale 0.
- Art in `Assets/Resources/UI/Art/` (Title, FmodLogo, pause paper, controller image, chevron), imported by `Assets/Editor/UiSpriteImportSettings.cs` (Sprite, FullRect, Point, uncompressed, no mipmaps, PPU = pixel width, max 8192). `UiSpriteImportTests` checks every importer.

### Facts found

- Gum text fonts are pre-rendered BMFont caches of system fonts (Ubuntu Mono, Special Elite, Bodoni MT). No TTF was in the repo. Ubuntu Mono (UFL) and Special Elite (Apache 2.0, under `apache/specialelite` in google/fonts, not `ofl/`) were downloaded into `Assets/Resources/UI/Fonts/` with their licences. Bodoni MT is a commercial Monotype font, so Libre Bodoni (OFL, variable font, default weight instance) stands in. Font glyphs were never looked at.
- Gum rotation is counter-clockwise and UI Toolkit `rotate` is clockwise, so the 1 degree paper tilt is `-1deg`. Not verified visually.
- Headless layout snaps to physical pixels in a window smaller than 1920x1080 (the 380 px paper measures 378), so layout tests use tolerances of 1.5 to 3 units. `translate` is not part of layout rects, tests add the resolved translate.
- `UnityEngine.UIElements.ParameterBinding` collides with the audio `ParameterBinding` type in files that import both, use an alias.
- Src `MainMenuScreen` lets only the lowest player index drive the menu, while `MainMenuPanel` lets every player navigate. The port follows the panel (all players, per-player selection, small P1 to P4 tags next to the row).
- In Src a Pause press while the controls overlay is open unpauses the whole menu (the toggle check runs before the controls branch). Kept.

### Deviations from the Gum layout

- Options row container is centred at 50% of the paper. Gum has 21.35%, which would put a 340 px row about 89 px off the paper.
- Gum's extra 1 degree rotation on the options row container is dropped.
- Libre Bodoni for Bodoni MT (above).
- Src places the P1 and P2 tags left of the label and the P3 and P4 tags right at different offsets, the port puts all tags in one label left of the row.

### Deferred (exact seam and reason)

- Xbox glyphs: resolved in B2.2. `UiSpriteCrop.Glyph` crops the sheet and the controls overlay Close button shows the real A glyph.
- Menu input before any player has joined: the join screen (B2.4, `JoinScreenController`) is where players join, but no scene shows it yet, so `MainMenuController` needs at least one `IInputActions` in its player list, or nobody can drive the menu. Src always has a keyboard config. Seam: the `Func<IReadOnlyList<IInputActions>>` argument of `Configure`. A default keyboard and gamepad source is not built.
- Main menu wiring in a scene: no scene contains `MainMenuController`, `PauseMenuController` or a `SoundServiceRunner` yet. The convenience `Configure` overloads (`SoundServiceRunner.Instance`, `FindFirstObjectByType<PlayerJoinManager>`) have no test.
- Not ported from Src `MainMenuScreen`: the Shift+R shortcut to `ShootingRangeScreen`, the snowstorm particles, the battle theme start and stop, `SaveManager.HasSave`. `hasSave` is a plain parameter. The screen switches (`StartNewGame`, `ContinueGame`, `Game.Exit`) are the callbacks the caller passes in.
- `MainMenuPanel` also lists a "Shooting Range" entry that the live Gum screen does not show. Not ported.
- Row pitch on the main menu: Gum `MainMenuButton` height is unit 5 with value 20, meaning unclear. The port uses 5 px margins top and bottom. Compare against the MonoGame build.
- Everything in this slice is unverified visually (no Editor GUI): fonts, paper tilt, row spacing, the vignette, background stretch, panel scaling on non-16:9 windows.

## B2.2 hub and shop: what is ported, what is deferred

Plan: `docs/superpowers/plans/2026-09-21-b2-2-hub-shop-plan.md`. Slice: the hub overlay, crafting help, departure hint and decision bubble, the shop tooltip, and the shop domain behind them, in UI Toolkit, screen space only. Src has no shop screen: the shop is world objects (`BuyableStationWrapper`) that show a `ToolTip` above them.

### Ported

- Shop data (Core, engine-free): `EItemType`, `StationConfig` and `ComponentConfig` (shop and text subset) with tables transcribed from `StationConfig.json` and `ComponentConfig.json` in JSON order, `CatalogItem`, `IShopService` and `ShopManager` (catalog order matters, `HubMapModel.SelectOfferIndices` indexes into it), `RunCredits` (credits, upgrade counter, crafting help flag), `StationTooltipInfo` (category, functionality and icon rect per station id), `ShopItemIconAtlas`, `XboxButtonAtlas`. A test pins ids, prices and order.
- Offers: `BuyableOffer` is the data shape of `BuyableStationWrapper` and implements `ITooltipable`. `HubShopModel` restocks with `HubMapModel.RestockSeed`, `SelectOfferIndices` and `OfferPositions` and runs the purchase rule (spend, count the upgrade, raise `Purchased`, remove the offer).
- Hub state: `HubDepartureModel` (ready set pruned to joined players, all ready, pending off-board items, 0.75 s depart hold, 5 s hint delay and 8 s hide, decision after all ready with pending items, 0.2 s input block), `DialogBubbleModel`, `CraftingHelpModel`, `HubInput` (Back toggles crafting help, Interact allows depart, Grab un-readies, Grab wins when both are pressed). The plan's `HubOverlayModel` was dropped as a pass-through: the overlay reads `RunCredits` and `HubDepartureModel` directly.
- Tooltip: `TooltipModel`, `TooltipInteractions` (the `ConfigureInteractionButtons` switch) and `TooltipLayout` (ideal placement, overlap separation, clamp) in 1920x1080 canvas units.
- Views (Runtime, `Gamelab.UI.Runtime`): `HubOverlayView`, `CraftingHelpView`, `DialogBubbleView` (passive hint, two-button decision, hide), `ToolTipView`, with UXML and USS under `Assets/Resources/UI/` (`ButtonWithIcon` is cloned into the button slots). `HubUiController` owns them, ticks `HubInput` and the tooltips from `Update`, and passes `DepartRequested` through. Every dependency is injected in `Bind`. `TooltipLayer.Set(id, ITooltipable, TooltipKind)` places a tooltip through `IWorldToScreen`.
- `UiSpriteCrop` cuts source rects out of a sheet (Src top-left origin, Y flipped for Unity) and caches them. The Xbox glyphs on the controls overlay and on the tooltip and bubble buttons come from `xbox_buttons_spritesheet.png` this way. This resolves the B2.1 Xbox glyph deferral.
- Art added under `Assets/Resources/UI/Art/`: coin, tooltip base, badge, category icons (`BasicCasing`, `BasicProjectile`, `BasicPropellant`, copied from `Src/Content/Items`), `IdleA0` to `IdleA3`, bubble parts, the Xbox sheet and the select glyph.

### Facts found

- `Resources.Load<StyleSheet>("UI/Name")` can return the StyleSheet named `inlineStyle` that a UXML of the same name exposes, instead of the USS. Which one comes back depends on import order, and it flipped once the art folder held twelve or more files, silently dropping a whole USS (three B2.1 tests failed). `UiResources.LoadStyle` now uses `LoadAll` and matches by name. Keep USS and UXML names identical per view and load styles only through it.
- In the 640x480 headless panel the canvas scale is 0.44, so one screen pixel is 2.25 canvas units and layout values snap accordingly. Layout tests use a tolerance of 3. 0.44 is the max rule: Shrink with ScaleWithScreenSize resolves to the larger of the two axis scales, max(w/1920, h/1080), and the root is 1440 wide there. The B2.1 comment "min, same as Gum" was wrong. Gum letterboxes with the smaller scale, so on non-16:9 windows UI Toolkit shows less of the canvas than Gum. Left as is, unverified against Gum. `UiPanel.CanvasScale` returns the real value and `TooltipLayer` clamps to the real canvas size (screen / scale).
- Category icons: Src picks `Content/Items/Basic*.png` when the file exists and only otherwise crops the atlas, so the three categories always use the files. The atlas rect stays in Core for `IconSourceRect` parity.
- `spr_xbtn_32.png` is a Gum design-time value that Src overrides with a glyph from the Xbox sheet at runtime, so it is not imported.
- The plan's `HubUiController` takes `Func<IReadOnlyList<PlayerSlot>>` (the API `HubInput` already had) and a caller-supplied `Func<int>` for the pending off-board count, which needs the map.

### Deviations from the Gum layout

- Fonts: Bahnschrift Light (tooltip and crafting help descriptions) becomes Ubuntu Mono 18, Bernard MT Condensed (tooltip name) Ubuntu Mono 22, Berlin Sans FB (category badge) Ubuntu Mono 16, Bodoni MT (button text) Libre Bodoni. Mono is wider, so long names and descriptions may wrap or overflow their fixed boxes. Unverified visually.
- Nine-slice borders are equal thirds of each texture (64 tooltip, 16 bubble, 16 and 7 name bar, 21 and 8 badge). Gum's blend mode on the name bar tint is applied as a plain tint colour.
- The decision bubble spaces its two buttons 40 px apart. Gum sizes the interaction box as the widest button plus 105, so the gap differs by a few pixels.
- ButtonWithIcon text stays at the top of the button, as Gum leaves Y unset.
- The CraftingHelp text is hard-coded in the view (static text in Gum).
- `HubDepartureModel.ToggleReady` is also blocked while Departing. Src blocks it only while the decision is open. Harmless, since the screen is leaving.

### Deferred (exact seam and reason)

- Station objects, their highlight, the lever `OnInteractOverride`, spawning the real station on purchase, the buy VFX and `Sounds.Purchase`. Seam: the world wave reads `HubShopModel.Offers` and listens to `Purchased`, and calls `TooltipLayer.Set` for the highlighted offer.
- `CountPendingShopItemsOffBoard`, the departure fade and screen switch, `SaveManager`. Each is the `Func<int>` or the event handler the caller passes to `HubUiController.Bind`.
- DialogBubble typewriter reveal, `Show(line)` with world anchor and the bubble tail. They belong to the tutorial dialogue slice (`DialogueOverlay`, `DialogueManager`).
- Category icons for categories other than Casing, Projectile and Propellant would crop `BulletComponentSpriteSheet.png`, which is not imported, so those icons stay hidden.
- `CameraWorldToScreen` is checked only against `MirroredCamera` at the headless test camera size. Real scene cameras, non-16:9 windows and a camera rect that is not full screen are unverified, and the tooltip clamp now uses the real canvas size (see Facts found).
- Not consumed yet: no scene contains `HubUiController`. Player spawning and the world-space hub view belong to other slices. The in-game HUD is B2.3 and the post-level, fail and join screens are B2.4.
- Everything in this slice is unverified visually (no Editor GUI): fonts, nine-slice edges, text wrapping, tooltip placement over stations, panel scaling on non-16:9 windows.
- The controller is stopped while paused, so the caller must hide tooltips with `Tooltips.SetAllVisible(false)` then and call `Tooltips.ClearAll` on departure.

## B2.3 HUD: what is ported, what is deferred

Plan: `docs/superpowers/plans/2026-09-21-b2-3-hud-plan.md`. Slice: `GameplayHud` and the four `Src/Components/IngameHUD` components (distance track with train marker, goal marker and enemy dots, speedometer needle, temperature frost), in UI Toolkit, screen space only.

### Ported

- Core (`Gamelab.UI`, `Core/UI`): `HudMath` (distance ratio, track travel range, speed ratio, needle degrees, frost opacity per layer, dot ratio) and `HudModel` (level, smoothed speed ratio, temperature ratio, marker fraction, needle degrees, frost opacities, dot ratios, `LevelVersion`). Goldens come from evaluating the Src expressions with a script. The model has no clock and no events.
- Runtime (`Gamelab.UI.Runtime`): `HudView` (UXML and USS `Hud`, one `UIDocument`) and `HudController`. `HudController.Bind(TrainState, LevelDefinition, maxSpeed)` and `Bind(TrainStateRuntime, LevelRuntime)` read `State.DistanceTraveled`, `actualSpeed`, `Temperature`, `MaxTemperature` and the level definition. `Update` calls `HudModel.Update` and `HudView.Refresh` every frame. `SetLevel` rebuilds the dots only when the level reference changes.
- Art in `Assets/Resources/UI/Art/`: `GaugeDistance`, `GaugeHand`, `spr_hud_train_marker`, `spr_hud_goal_x`, `spr_hud_enemy`, `FrostScreen1` to `3`. The UI import rule applies by path.
- Pause: the HUD reads state and uses no clock, so `Time.timeScale = 0` needs no special case. The needle smoothing is one 0.15 lerp per `Update` call, as in Src, so it depends on frame rate and keeps converging while paused. A test runs the controller under timeScale 0.

### Facts found

- UI art was importing as Tight, so the sprite rects in the .meta files were alpha-trimmed (GaugeDistance 3593x1000 instead of 4846x1000, GaugeHand 33x292). It now imports as FullRect like the map art, so rects equal the PNG size. This also affects the B2.1 and B2.2 art: their layouts were tested against trimmed sprites and still pass, but were not looked at visually.
- The "enemy dots" are the level's spawn events, not live enemies. `EnsureEnemyDots` places one dot per `LevelDefinition.SpawnEvents` entry at `spawn.Distance / levelDistance`, and never removes them when the enemy spawns. No seam into the enemy runtime is needed.
- The Src HUD shows no wall health, so `ShootHoleWallRuntime` is not touched. Temperature drives only the frost overlay.
- There is no dial sprite in the speedometer. The dial is part of `GaugeDistance.png` (4846 x 1000, drawn at 20 percent, so 969.2 x 200). The track is 51.95 percent of that width, 503.5 canvas units, and the travel range is 503.5 minus 48.
- Gum position unit 5 is from the bottom with positive Y going down, unit 3 on a sprite is percent of the source image, and Gum rotation is counter-clockwise (USS `rotate` is clockwise, so the needle uses the negated angle).
- `WorldUiManager` is the world-anchored tooltip manager. B2.2 ported all of it, and the HUD does not use `CurrencyDisplay` or `ToolTip`. Nothing of it was added here.

### Deviations from the Gum layout and Src

- `HudModel` starts with a temperature ratio of 1 (no frost). Src starts at 0, which would draw full frost on a frame before the first update.
- Draw order is the same as Src: `GameplayScreen` draws `hud.Draw` (the frost) with the sprite batch and then calls `GumService.Default.Draw()`, so frost is under the gauge, as in the port's tree order. `virtualScreenSize` is 1920x1080, so the fixed 1920x1080 frost rect is the Src rect.
- The three frost layers stay in the tree at opacity 0 (Src skips the draw below the threshold). This keeps about 44 MB of uncompressed textures resident for the HUD. Switch to `display: none` only if a frame capture shows a cost.
- The HUD document has sorting order -10 so menus, hub views and tooltips draw above it.
- Gum's needle is a 260 px image with a vertical flip and a 180 degree rotation about its top edge. The port uses a horizontal mirror and places the image so the tip lies 58 units above the pivot. Measured: the pivot is the container's bottom centre at (100, 73) canvas units. The dial pin in `GaugeDistance.png` is at about (100.1, 70.5) with radius 2.8, so the pivot is on the pin. `GaugeHand.png` has no hub, its wide end is a counterweight reaching 17 units past the pivot, and the tip reaches 58 units against a dial radius of 43 because Gum draws the hand at 26 percent and the dial at 20 percent of their sources (a Gum authoring mismatch, kept as is). The on-screen look was still not seen.
- `TrackWidth` 503.5 is a constant in `HudView` and in `Hud.uss`. Src reads the real container width.
- Font: Ubuntu Mono 35 for the dashes, as in Gum (it is already the B2.1 font).

### Deferred (exact seam and reason)

- Nothing in a scene contains `HudController`. Seam: a gameplay scene calls `Bind(trainStateRuntime, levelRuntime)` and `SetLevel` when the next level starts.
- `HudController` does not follow `LevelRuntime.Definition` changes on its own (review ruling). The caller calls `SetLevel`, and `Bind` before `LevelRuntime.Initialize` sees a null definition.
- The runtime `Bind` overload throws a plain `NullReferenceException` on null arguments (review ruling).
- Player spawning and the cannon seat: other slices. The post-level and fail screens, the join screen and the skip element are B2.4.
- Everything is unverified visually (no Editor GUI): the layout numbers are asserted only at a 640x480 test panel with a tolerance of 3, and the needle pivot, frost draw order, sorting order against other UI and scaling on non-16:9 windows were not seen.

## B2.4 screens: what is ported, what is deferred

Plan: `docs/superpowers/plans/2026-09-21-b2-4-screens-plan.md`. Slice: the post-level waybill (`PostLevelStatsScreen`), the fail incident report (`FailScreen`), the join screen (`JoinScreen`) and the `SkipTutorial` element, with their Gum components, `StampRevealAnimator` and `LevelRewardBreakdown`, in UI Toolkit, screen space only. This closes wave B2.

### Ported

- Core data (`Core/Screens`, `Core/Utils`): `Easing`, `StageNaming.GetStageTitle`, `LevelRewardBreakdown` (the Src formula, banker's rounding kept, with `baseReward` 25 and `referenceBonus` 20 as parameters because the Unity port has no `GameplayConfig`), `FailureReason` with `FailureReasonText` (the exact Src strings, including the missing spaces), `PostDeathStatsSnapshot` and `PostDeathStatsText` (stat strings and the incident line). Goldens come from scripts that evaluate the Src expressions in float32.
- Core timelines (`Gamelab.UI`, `Core/UI`): `FadeTransition` (the `FilterTransition` opacity math without the colour), `StampRevealTimer`, `PostLevelStatsModel` (phases, cue times, count-up, credit grant once, stamp, fade, `ContinueRequested`), `FailScreenModel` (typewriter, stamp, `ReturnRequested`), `SkipTutorialModel` (+2.0 and -5.0 per second, cap 1.5, `SkipRequested` latches until `ConsumeRequest`, as Src `pendingHubOutroRequest`), `JoinScreenModel` (title, per-slot figure, button text and glyph from a joined-count function). Models have no clock. Sounds leave through a `Cue` event carrying a `Sounds` id.
- Views (Runtime, `Gamelab.UI.Runtime`): `PostLevelStatsView`, `FailScreenView`, `JoinScreenView`, `SkipTutorialView`, and `StampView.Apply` (scale and rotation from a `StampRevealTimer`). UXML and USS of the same name are in `Assets/Resources/UI/` and load through `UiResources`. The Continue, Return and Join buttons reuse `ButtonWithIconElement` and `UiSpriteCrop.Glyph`.
- Controllers: `PostLevelStatsController.Configure(actualTime, referenceTime, stageNumber, grantCredits, onContinue, players, playSound)`, `FailScreenController.Configure(reason, stats, levelNumber, now, onReturn, players, playSound)` and `JoinScreenController.Bind(JoinFlowController, onAdvance)`. Each configures or binds once. `players` is the same `Func<IReadOnlyList<IInputActions>>` the menu controllers take, and confirm is Pickup or Start on any player (`AnyPressedMenuConfirm`). `SkipTutorialView` has no controller: the caller feeds `SkipTutorialModel.Update(dt, anyPlayerHoldsBack)` and calls `Refresh()`.
- Join seam: `JoinScreenController` reads `JoinFlowController.IsSlotJoined(0..3)` and forwards `OnReadyToAdvance`. `JoinFlowController` and `PlayerJoinManager` were not changed. Joining itself (A on a pad, Space on the keyboard) stays in `PlayerJoinManager`.
- `PlayersReady` was already ported inside `HubOverlayView` (B2.2). `PostDeathDisplay` is an empty Gum container and has no port.
- Art added under `Assets/Resources/UI/Art/`: `spr_waybill_paper`, `spr_waybill_punch`, `spr_waybill_paid_stamp`, `spr_incident_paper`, `spr_filed_closed_stamp`, `Silhouette`.

### Time and pause, per animation

- Post-level screen (reveal clock, line reveals, count-up, credit tick cooldown, stamp pop, the 1.15 s stamp phase, both fades) and fail screen (typewriter, tick cadence, stamp pop, the 1.10 s return delay): `Time.unscaledDeltaTime`. Both screens replace gameplay, but a pause menu that left `Time.timeScale` at 0 must not freeze them. Tests run both under `timeScale` 0.
- Join screen: no animation and no clock. `SkipTutorialView`: no clock, it shows what `SkipTutorialModel` holds. The gameplay caller feeds that model with the gameplay `dt`, which is 0 while paused, so the bar holds still, as in Src.

### Facts found

- `LevelRewardBreakdown` in Src uses `MathF.Round` and the distance text uses `Math.Round`, both banker's rounding. A 22.5 bonus rounds to 22. Kept and pinned by a test.
- Src calls `SwitchToScreen` every frame once its timer or fade is done. The models raise `ContinueRequested` and `ReturnRequested` once.
- `FailScreenModel` runs the typewriter before the phase switch with the same `dt`, so a pickup tick can fire on the confirm frame before `MenuSelect`. Src does the same. `RevealAll` on the post-level screen fires no cues.
- Src never sets `IncidentTitleDetail` on the waybill, so the static Gum string stays.
- Join figure order is not slot order: joined1 to joined4 use `IdleA0`, `IdleA3`, `IdleA1`, `IdleA2`, an empty slot uses `Silhouette`. `JoinScreenModel` raises no `Changed` for the initial empty state, so the view renders on bind.
- Gum title X=54 on the join screen is absolute and looks like a typo for 5 percent. Kept.
- `PlayerJoinManager.ResetJoins()` exists. Src `JoinScreen` resets the roster on init and `JoinScreenController` does not, so a caller that wants the Src behaviour calls it before showing the screen.

### Deviations from the Gum layout and Src

- `StampView.Apply` sets display, scale and rotation from a `StampRevealTimer`. Gum resizes the sprite from its top-left origin, the view scales with `transform-origin: 0 0`, which gives the same picture. Rotation is negated (Gum counter-clockwise, USS clockwise).
- `PostLevelStatsView` and `FailScreenView` (UXML and USS of the same name). `Refresh()` is meant to be called every frame by the controller. `PostLevelStatsModel.Rewards` was added for the row texts.
- Nine-slice art uses `-unity-slice-type: tiled` (Gum tiles the middle sections). Borders are assumed thirds: waybill 32 on all sides, incident paper 85 and 64. Not looked at visually.
- Waybill: the Gum root offset (-13,-65) is folded into the paper margin. The Continue button at Gum (1081,691) is paper-relative (421,351). The incident detail text is the static Gum string, Src never sets it.
- Fail report: MainBox order follows the gucx instance order (title separator line, stats, dash separator, cause title, cause text, dash separator, button). Title lines are placed by their vertical centre (Gum uses 1 px high boxes anchored from the bottom). The paper height follows the content and grows while the cause text types, as in Gum.
- Fonts: Gum sets Font=Ubuntu Mono on the amount text and no size; the row descriptions and the dots have no font set. The views use size 18 and 17 for these with the theme default font, and Ubuntu Mono for the amounts. All layout and glyph rendering is unverified visually.
- `JoinScreenView` and `SkipTutorialView` (5b). Both use px in the 1920x1080 canvas, so at 16:9 they equal the Gum layout and at other aspect ratios they are anchored top-left (join) or bottom-right (skip) instead of letterboxed. The join screen background is black (Src has none, `GamelabGame` clears black). Gum title X=54 is absolute and looks like a typo for 5 percent, kept. Slots are 432 px squares (25 percent of 1728, a height of 100 percent of width), the figure fills an 80 percent Player square, the Join button is centred at 55 percent of it and 110 percent down. `JoinScreenView` refreshes on Bind and rebuild and on `model.Changed`, the owner calls `model.Refresh()` each frame.
- `SkipTutorialView`: Src `SetProgress` resizes the whole ProgressBar container (background and fill), which is centred, so the bar shrinks about its centre. The view does the same. The element size (240 x 50) is derived from Gum "relative to children" (text ends at 232, plus 8, bar bottom at 50) and the text box height (24) is a guess. The caller sets the document sorting order and feeds the model.
- Silhouette and `IdleA0` are 410x400, `IdleA1` to `IdleA3` are 400x390, and Gum's source rect is 410x400 for all four. The port stretches every texture to the Player square. Unverified.
- Snowstorm particles (`IVfxService`) on both screens are not ported. Background colours are USS (white waybill screen, dark red fail screen under the vignette, black join screen).
- Fonts follow B2.1: Courier New is Ubuntu Mono, Special Elite is kept, Bodoni MT is Libre Bodoni.

### Deferred (exact seam and reason)

- Nothing in a scene contains `PostLevelStatsController`, `FailScreenController`, `JoinScreenController` or a `SkipTutorialView`. Seams: `grantCredits` is `RunSession.AddCredits`, `onContinue` switches to the hub, `onReturn` switches to the main menu, `onAdvance` switches to the main menu, `playSound` is `ISoundService.PlayOnce`, `players` is the roster's input list.
- The caller does what Src does in the screens' load step: `soundService.ResetGlobalParameters()` for the post-level screen, `LoadSound` for the sounds used, the screen switch and the `Game.CurrentRun` reads (stage number, level number, stats snapshot).
- `SkipTutorialModel.SkipRequested` is consumed by the tutorial caller, which also does what Src `ConsumePendingHubOutroRequest` does (clear guidance, mark the tutorial completed, save the run). No `TutorialDirector` exists in the port.
- Everything is unverified visually (no Editor GUI): the waybill and incident paper nine-slice tiling (borders assumed thirds), fonts and text fit, stamp pop, the fail report title line overlap, vignette, the skip element size, join slot layout on non-16:9 windows (px anchored top-left instead of Gum's letterbox), and the panel scale finding from B2.2.

## Wave B2 complete: what remains for wave C

Every UI seam that no scene consumes yet, and what the scene or caller has to pass:

- `MainMenuController`, `PauseMenuController`: a scene, a `SoundServiceRunner`, at least one `IInputActions` source, `StartNewGame`, `ContinueGame` and exit callbacks, `hasSave`.
- `HubUiController`: `RunCredits`, `HubDepartureModel`, `CraftingHelpModel`, the player list, the pending off-board count, an `IWorldToScreen` (`CameraWorldToScreen` is checked only headless), `DepartRequested`. The world wave also consumes `HubShopModel.Offers` and `Purchased`, and calls `TooltipLayer.Set`.
- `HudController`: `Bind(trainStateRuntime, levelRuntime)` and `SetLevel` per level.
- `PostLevelStatsController`, `FailScreenController`, `JoinScreenController`, `SkipTutorialView` (this section).
- Still not built: the dialogue overlay (typewriter, world anchored bubble), the tutorial director, player spawning, the cannon seat, snowstorm and other particles, the save system and run session, screen switching.
- Scene wiring that touches several of these together: how pause and scene switching interact (`Time.timeScale` restored on scene switch, `TrainStateRuntime.SelfTick`), the sorting orders (HUD -10, hub views 0 to 2, menus above), and the shared `PanelSettings` per screen.
- Visual check of every screen against the MonoGame build, on 16:9 and on other aspect ratios. Nobody has looked at any B2 screen.
