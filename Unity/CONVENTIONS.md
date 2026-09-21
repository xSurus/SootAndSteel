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
- HUD data: `TrainStateRuntime.State` (`Temperature`, `MaxTemperature`, `DistanceTraveled`, `CurrentSpeed`), `LevelRuntime.Definition.LevelDistance` and `DistanceTraveled`, `ShootHoleWallRuntime.Health` and events.
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
- Art in `Assets/Resources/UI/Art/` (Title, FmodLogo, pause paper, controller image, chevron), imported by `Assets/Editor/UiSpriteImportSettings.cs` (Sprite, Point, uncompressed, no mipmaps, PPU = pixel width, max 8192). `UiSpriteImportTests` checks every importer.

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
- Menu input before any player has joined: nothing creates a player before the join screen, so `MainMenuController` needs at least one `IInputActions` in its player list, or nobody can drive the menu. Src always has a keyboard config. Seam: the `Func<IReadOnlyList<IInputActions>>` argument of `Configure`. A default keyboard and gamepad source is not built.
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
- In the 640x480 headless panel the canvas scale is 0.44, so one screen pixel is 2.25 canvas units and layout values snap accordingly. Layout tests use a tolerance of 3.
- Category icons: Src picks `Content/Items/Basic*.png` when the file exists and only otherwise crops the atlas, so the three categories always use the files. The atlas rect stays in Core for `IconSourceRect` parity.
- `spr_xbtn_32.png` is a Gum design-time value that Src overrides with a glyph from the Xbox sheet at runtime, so it is not imported.
- The plan's `HubUiController` takes `Func<IReadOnlyList<PlayerSlot>>` (the API `HubInput` already had) and a caller-supplied `Func<int>` for the pending off-board count, which needs the map.

### Deviations from the Gum layout

- Fonts: Bahnschrift Light (tooltip and crafting help descriptions) becomes Ubuntu Mono 18, Bernard MT Condensed (tooltip name) Ubuntu Mono 22, Berlin Sans FB (category badge) Ubuntu Mono 16, Bodoni MT (button text) Libre Bodoni. Mono is wider, so long names and descriptions may wrap or overflow their fixed boxes. Unverified visually.
- Nine-slice borders are equal thirds of each texture (64 tooltip, 16 bubble, 16 and 7 name bar, 21 and 8 badge). Gum's blend mode on the name bar tint is applied as a plain tint colour.
- The decision bubble spaces its two buttons 40 px apart. Gum sizes the interaction box as the widest button plus 105, so the gap differs by a few pixels.
- ButtonWithIcon text stays at the top of the button, as Gum leaves Y unset.
- The CraftingHelp text is hard-coded in the view (static text in Gum).

### Deferred (exact seam and reason)

- Station objects, their highlight, the lever `OnInteractOverride`, spawning the real station on purchase, the buy VFX and `Sounds.Purchase`. Seam: the world wave reads `HubShopModel.Offers` and listens to `Purchased`, and calls `TooltipLayer.Set` for the highlighted offer.
- `CountPendingShopItemsOffBoard`, the departure fade and screen switch, `SaveManager`. Each is the `Func<int>` or the event handler the caller passes to `HubUiController.Bind`.
- DialogBubble typewriter reveal, `Show(line)` with world anchor and the bubble tail. They belong to the tutorial dialogue slice (`DialogueOverlay`, `DialogueManager`).
- Category icons for categories other than Casing, Projectile and Propellant would crop `BulletComponentSpriteSheet.png`, which is not imported, so those icons stay hidden.
- `CameraWorldToScreen` is checked only against `MirroredCamera` at the headless test camera size. Real scene cameras, non-16:9 windows and a camera rect that is not full screen are unverified, and the tooltip clamp assumes a 1920x1080 canvas.
- Not consumed yet: no scene contains `HubUiController`. Player spawning, the world-space hub view, the in-game HUD, post-level screens and the join screen belong to other slices.
- Everything in this slice is unverified visually (no Editor GUI): fonts, nine-slice edges, text wrapping, tooltip placement over stations, panel scaling on non-16:9 windows.
