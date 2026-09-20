# A2 breadth round: stations, enemy firing, off-screen rule

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development. One fresh implementer per task, a review after every task, one commit per task.

**Goal:** Port the game logic of the remaining domain items (conveyors, workbenches, component station, rack, rifle/tutorial enemy, cannon logic, speed lever, off-screen rule) and record what wave B must supply.

**Architecture:** Pure rules go in `Gamelab.Core` (System.Numerics, no engine). MonoBehaviours in `Gamelab.Runtime` follow `StationRuntime` / `EnemyRuntime`. Anything that needs the map, camera, player seat, shop, sound, VFX or animation is a small interface (seam) in Core or Runtime with a fake in the tests. `Src/` is read-only.

**Tech stack:** Unity 6000.3.24f1, C# 9 / netstandard2.1, NUnit EditMode + PlayMode.

**Spec:** `Unity/CONVENTIONS.md` ("Not ported yet"), Src sources named per task.

## Global constraints

- No C# 10+ syntax (no file-scoped namespaces, primary constructors, record structs, collection expressions, `ThrowIfNull`, `MinBy`). See CONVENTIONS.
- Constants and behaviour must match Src. Any dropped presentation behaviour gets a code comment naming the Src file.
- Pixel units stay pixels in Core; convert with `WorldUnits` only at the Unity physics boundary. Src frame is y-down (Top side = smaller Y).
- Tests: `-quit` is never combined with `-runTests`. Confirm `result="Passed"` in the results XML. Never start Unity while another Unity process runs (`pgrep -fl "Unity.app/Contents/MacOS/Unity"`).
- `git add Unity/Assets` (metas) and commit after each task. Never touch `Src/`.
- Not dead: the two `Compile Remove` entries in `Src/Gamelab.csproj` for `BuyableStationWrapper` and `StationConfig` are stale no-op paths and prove nothing. Both types are live in Src. `BuyableStationWrapper` is deferred with the shop. `SpeedLever` is compiled and is ported (Task 8).

## Scope decisions

- Deferred with seam: player seat/eject of the cannon (needs player + map tiles), map adjacency (`IStationGrid`), slot anchors and target walls (`IEnemyWorld`), bounds (`IWorldBounds`), train state (`ITrainState`), bullet emission from a `BulletItem` (`IBulletItemSpawner`).
- Dropped presentation: sound, VFX, animation, draw code, highlight, snapping, shop tooltips (`CategoryName`, `IconSourceRect`).
- `BulletStats.Color` and `BulletItem` colour: draw-only, stay deferred.

## Tasks

### Task 1: GridDirection and BulletItem (Core)

Files: create `Unity/Assets/Scripts/Core/GridDirection.cs`, `Core/Items/Bullets/ComponentTraits.cs`, `Core/Items/Bullets/BulletItem.cs`; tests `Unity/Assets/Tests/EditMode/Items/BulletItemTests.cs`, `GridDirectionTests.cs`.

Source: `Src/Utils/WorldUtility.cs` (GridDirection, Opposite, Clockwise, ToVector2), `Src/Items/Bullets/BulletItem.cs`, `Src/PhysicalEntities/Bullets/Components/*` (IsBasic: BasicCasing, BasicProjectile, BasicPropellant only).

Produces:
- `enum GridDirection { Up, Down, Left, Right }` in namespace `Gamelab.Utils`; `static GridDirectionExtensions { Opposite(), Clockwise(), ToVector2() /*System.Numerics, Up = (0,-1)*/ }`.
- `static ComponentTraits { EComponentType TypeOf(string id); bool IsBasic(string id); }` throwing `ArgumentOutOfRangeException` for unknown ids (all 13 ids from `ComponentIds`).
- `class BulletItem : Item` (namespace `Gamelab.Items.Bullets`), `Id == "Bullet"`: `EComponentType Type`, `IReadOnlyList<string> ComponentIds`, `bool HasBasic`, `bool HasUpgrade` (any non-basic component id), ctors `BulletItem(string componentId)` and `BulletItem(params BulletItem[] components)` with Src rules (single = copy; one type = upgrade merge; three distinct non-Bullet types = Type Bullet; else throw `Exception`), basic ids ordered first after merge, `ToRecipe()` returning `BulletRecipe`.

TDD: tests for each ctor branch, the throw, basic-first ordering, HasBasic/HasUpgrade, direction helpers (each direction).

### Task 2: Workbench and AutoWorkbench

Files: create `Core/PhysicalEntities/Stations/WorkbenchCrafting.cs`, `Runtime/PhysicalEntities/Stations/WorkbenchRuntime.cs`, `AutoWorkbenchRuntime.cs`; tests `Tests/EditMode/Stations/WorkbenchCraftingTests.cs`, `Tests/PlayMode/Stations/WorkbenchRuntimeTests.cs`.

Source: `Src/PhysicalEntities/Stations/Workbench.cs`, `AutoWorkbench.cs`.

Produces: `WorkbenchCrafting` (Core): `const float CraftSeconds = 2f`, `const int MaxItems = 3`, `IReadOnlyList<BulletItem> PlacedItems`, `float CraftProgress`, `bool IsCrafting`, `bool CanPlace(Item)` (ValidatePlace, incl. `craftProgress > 0` refusal), `Place(BulletItem)`, `bool CanTake`, `BulletItem Peek()`, `BulletItem Take()`, `bool CanCraft()` (ValidateCraft), `bool InteractHeld(float dt)` (returns true when a craft completed: replaces items by one combined `BulletItem`, resets progress/IsCrafting), `InteractReleased()` (IsCrafting = false; progress is kept, as in Src). Runtime `WorkbenchRuntime : StationRuntime` overrides Can/Receive/Peek/TryProvide/CanProvide via the logic, `OnInteractHeld(IPlayerActor, float)`, `OnInteractReleased(IPlayerActor)`; `AutoWorkbenchRuntime` calls `InteractHeld(dt)` every `Update(dt)` after `base.Update`. Sparks, craft sound, light and highlight-driven `isCrafting` are dropped (comment). `Initialize` sets StationId to Workbench/AutoWorkbench when the entry id is empty.

TDD cases: place limits, duplicate component id refusal, upgrade rules (same type, 2 max), final-bullet rule (three distinct types, all HasBasic), craft only when valid, 2 s timing (0.5 s chunks), cancel keeps progress and blocks take/place, FIFO consumer ticket removal on TryProvide, AutoWorkbench crafts through `Update`.

### Task 3: Conveyors

Files: create `Runtime/PhysicalEntities/Stations/IStationGrid.cs`, `ConveyorRuntime.cs`, `BulletConveyorRuntime.cs`, `UpgradedComponentConveyorRuntime.cs`; modify `StationRuntime.cs` (IsConsumerFirstInLine: `consumer is IPlayerActor` returns true, as Src; update the comment); test `Tests/PlayMode/Stations/ConveyorRuntimeTests.cs`.

Source: `Src/PhysicalEntities/Stations/Conveyors/*.cs`, `AbstractStation.cs` (IsConsumerFirstInLine).

Produces: `interface IStationGrid { StationRuntime GetAdjacentStation(Vector2 position, GridDirection direction); }` (Runtime, meters or any consistent unit the map uses); `ConveyorRuntime : StationRuntime` with `GridDirection FacingDirection`, `IStationGrid Grid { get; set; }`, `const float TransportDuration = 3f`, `float TransportTimer`, `bool IsBeingHeld` (settable seam for Src `IsBeingHeld`), virtual `AcceptsItem(Item)`, `OnInteract(IPlayerActor)` rotates clockwise; whole `Update(float)` state machine incl. push/pull pings, half-duration hand-off and the sink-ready rules. `BulletConveyorRuntime` accepts only `BulletItem` of Type Bullet. `UpgradedComponentConveyorRuntime` accepts BulletItem, not Type Bullet, HasBasic, HasUpgrade. Animation `SetActive` dropped. If `Grid == null` the conveyor has no neighbours.

Tests use a fake grid. Cases: player receive skips to half, item travels source to sink in 6 s of ticks (3 s per half? No: 1.5 s to half, wait for sink, 1.5 s after), stalls at half when sink not ready, filters per subclass, rotation, provider FIFO on `CanReceiveItem`, conveyor does not pull from a same-facing conveyor.

### Task 4: ComponentResourceStation and BulletRack

Files: create `Runtime/PhysicalEntities/Stations/ComponentResourceStationRuntime.cs`, `BulletRackRuntime.cs`; tests `Tests/PlayMode/Stations/ComponentResourceStationTests.cs`, `BulletRackTests.cs`.

Source: `Resources/ComponentResourceStation.cs`, `Cannon/AmmoRack.cs` (class `BulletRack`), Core `StationIds`.

Produces: `ComponentResourceStationRuntime : ResourceStationRuntime` with `Initialize(StationCatalogEntry, string componentId)` (ResourceId = StationIds.GetComponentResourceId(componentId)... check Src: base id is `Component<id>` via `ResourceStation`; keep `StationId == StationIds.GetResourceStationId(StationIds.GetComponentResourceId(id))` if that is what Src yields, and verify `IsComponentStationId`), `ComponentId`, accepts a `BulletItem` whose ComponentIds equal exactly `[ComponentId]`, provides `new BulletItem(ComponentId)`, peek same. `BulletRackRuntime`: FIFO list, `MaxCapacity = 5`, accepts only `BulletItem` Type Bullet while below capacity, `Peek` is the oldest, `TryProvideItem` removes the oldest and the consumer ticket. Shop/tooltip members dropped (comment). `OnGrab` returning false (racks cannot be grabbed) is a player/grab seam: comment only.

### Task 5: Enemy tuning, bounds seam, off-screen rule

Files: create `Core/Config/EnemyTuning.cs`, `Core/Enemies/IWorldBounds.cs`, `Core/Enemies/EnemyCulling.cs`, `Core/Enemies/IEnemyWorld.cs`; modify `Runtime/Enemies/EnemyRuntime.cs`; tests `Tests/EditMode/Enemies/EnemyCullingTests.cs`, `Tests/PlayMode/Enemies/EnemyOffscreenTests.cs`.

Source: `Src/Config/GameplayConfig.cs`, `Src/Content/Data/gameplay.json`, `Src/Enemies/Core/AbstractEnemy.cs`, `EnemyTargetingHelper.cs`.

Produces:
- `EnemyTuning` (namespace `Gamelab.Config`): immutable, `EnemyTuning.Default` with the Src effective values: EnemyHealth 100, EnemySize 72, RifleMaxSpeed 700, RiflePreferredDistance 30, EnemyShootCooldown 4, TutorialEnemyShootCooldown 6, TutorialEnemyHealth 1, EnemyShootSpread 0.2, EnemyFleeDelay 0.75, TrainTileSize 80. Comment: Src loads `gameplay.json` (which does not override these beyond defaults); wave B may build another instance.
- `interface IWorldBounds { float MinX { get; } float MaxX { get; } }` (pixels).
- `static EnemyCulling { bool IsOffScreenLeft(float xPx, float sizePx, IWorldBounds b) /* x < b.MinX - size, Src uses 0 */; bool IsFleeCulled(float xPx, float sizePx, IWorldBounds b) /* x > MaxX + 600 || x < MinX - size */ }`.
- `interface IEnemyWorld : IWorldBounds { Vector2 GetSlotAnchor(EnemyTrainSlot slot, float distanceFromTrainPx); Vector2 GetTargetPoint(EnemySlotSide side, Vector2 enemyPositionPx); }` (System.Numerics, pixels).
- `EnemyRuntime`: `public IWorldBounds Bounds { get; set; }`, `protected virtual float SizePixels`, and a private `Update()` calling `protected virtual void Tick(float dt)`, which sets `ShouldRemove = true` when `Bounds != null` and the left rule holds (position converted with `WorldUnits.ToPixels`). Update the stale "Off-screen" comment.

Tests use a fake `IWorldBounds`; PlayMode: a Dummy enemy past the left edge is flagged after one frame, and not flagged when `Bounds` is null.

### Task 6: Rifle enemy state machine (Core)

Files: create `Core/Enemies/RifleEnemyBrain.cs`; test `Tests/EditMode/Enemies/RifleEnemyBrainTests.cs`.

Source: `Src/Enemies/Types/Enemy.cs` (lines 40-215 logic only), `AbstractEnemy.cs`.

Produces: enums `HorseState { ApproachingSideAttackSlot, HoldingSideAttackSlot, Fleeing }`, `RiderState { Idle, Aiming, Recoil, Dead }` (Core namespace `Gamelab.Enemies.Core`); `class RifleEnemyBrain` with ctor `(float shootCooldown, float fleeDelay, float initialTimeSinceLastShot)`, constants `AimDurationSeconds = 1`, `RecoilDurationSeconds = 0.4`, properties `Horse`, `Rider`, `FleeDirection`, `bool WasNeutralized => Rider == Dead`, `void Tick(float dt) /* timeSinceLastShot += dt */`, `bool TryShoot()` (Src guards), `bool UpdateRider(float dt)` returns true on the frame the shot must be fired (Aiming to Recoil), Dead branch counts the flee timer then sets `FleeDirection = 1` and `Horse = Fleeing`, `void ArriveAtApproach()` (Approaching to Holding), `bool TryStartFleeingDeath()` (false if already Dead; sets Dead and the flee timer to `fleeDelay`).

Tests: shot needs Holding + cooldown + Idle; 1 s aim then fire then 0.4 s recoil then Idle; TryShoot resets the cooldown; Dead ignores shooting; flee delay 0.75 then Fleeing; double death returns false.

### Task 7: RifleEnemyRuntime, TutorialEnemyRuntime, enemy bullet spawner

Files: create `Runtime/Enemies/RifleEnemyRuntime.cs`, `TutorialEnemyRuntime.cs`, `Runtime/PhysicalEntities/Bullets/IBulletItemSpawner.cs`, `CatalogBulletSpawner.cs`; tests `Tests/PlayMode/Enemies/RifleEnemyRuntimeTests.cs`.

Source: `Enemy.cs`, `TutorialEnemy.cs`, `EnemyTargetingHelper.cs`, `Src/Services/Bullet/` (EmitBullet: read how base stats and position are used).

Produces:
- `interface IBulletItemSpawner { BulletRuntime Emit(BulletRecipe recipe, Vector2 positionMeters, Vector2 direction, BulletFaction faction); }`; `CatalogBulletSpawner : IBulletItemSpawner` (BulletComponentCatalogAsset + `BulletDefinitionAsset.BuildFromRecipe`, cached per component-id key, `BulletStats.CannonDefault()` base, as the existing ammo resolution test does; owns and destroys its created definitions via `Dispose`).
- `RifleEnemyRuntime : EnemyRuntime` with `Configure(EnemyTuning, EnemyAmmoDefinition, EnemyTrainSlot, IEnemyWorld world, IBulletItemSpawner spawner, System.Random rng)`, movement profile `EnemyMovementProfile.CreateDefault(tuning.RifleMaxSpeed)` through `EnemyMovementController`, `TryShoot()`, Tick order as Src (cull, timers, rider machine, horse machine). Approach anchor: `slotAnchor + (1.5*tile, top ? -tile : +tile)`; slot anchor `world.GetSlotAnchor(slot, size + preferredDistance)`; approach arrival `ArrivalRadius + 8`; Holding uses `UpdateTowardPoint(slotAnchor)` (Src `UpdateHoldPosition` is an alias without drift); Fleeing sets velocity `(FleeDirection * RifleMaxSpeed * 1.1, 0)` in meters and sets `ShouldRemove` via `EnemyCulling.IsFleeCulled`. Fire: direction to `world.GetTargetPoint`, spread `(rng.NextDouble()-0.5)*EnemyShootSpread`, emit enemy-faction bullet at the enemy position. `OnHit` override: Dead returns false; player-faction lethal hit sets Health 1 and starts fleeing death (returns true). `WasNeutralized`. Initial `timeSinceLastShot = rng * cooldown`.
- `TutorialEnemyRuntime : RifleEnemyRuntime` uses `TutorialEnemyShootCooldown` and `TutorialEnemyHealth`.
- Dropped and commented: animation states, horse sound, neck bleed / blood VFX, `EnemyMovementController` train-frame drift, the "InitialShooter is CannonStation or CannonSlot" check (Faction.Player instead, as `EnemyRuntime.OnHit`).

Tests use a fake `IEnemyWorld` and a recording `IBulletItemSpawner`: reaches slot then holds; fires after 1 s aim; lethal cannon hit leaves it alive at Health 1 and flees after 0.75 s; flee cull past MaxX+600; tutorial values (health 1, cooldown 6).

### Task 8: CannonStation logic and SpeedLever

Files: create `Core/Config/CannonTuning.cs` (or fields on `EnemyTuning`: keep a separate small type), `Core/PhysicalEntities/Stations/CannonAim.cs`, `Core/Map/TrainSpeedSetting.cs`, `Core/Map/ITrainState.cs`, `Runtime/PhysicalEntities/Stations/CannonStationRuntime.cs`, `SpeedLeverRuntime.cs`, `Runtime/PhysicalEntities/Interfaces/ICannonAimSource.cs`; tests EditMode `CannonAimTests.cs`, `TrainSpeedSettingTests.cs`; PlayMode `CannonStationRuntimeTests.cs`, `SpeedLeverRuntimeTests.cs`.

Source: `Cannon/CannonStation.cs`, `Src/Structures/CannonSlot.cs` (not ported: record only), `SpeedLever.cs`, `Src/Map/Train/State/TrainSpeedSetting.cs`, `TrainState.cs`.

Produces:
- `CannonTuning` defaults: CannonCooldown 0.5, CannonRotationSpeed 6, TrainTileSize 80, InputMovementDeadzoneSquared 0.25.
- `CannonAim.Step(float currentAngle, Vector2 input, float dt, float rotationSpeed, float deadzoneSquared)` returns the new angle (Src UpdateAim, `WrapAngle` diff clamped to `speed*dt`).
- `CannonStationRuntime : StationRuntime` with `float CooldownTimer`, `Update` decrement then aim, `OnInteract(IPlayerActor)` (cooldown gate, `ReloadCannon` from every adjacent `BulletRackRuntime` via `IStationGrid` over all four directions, then `FireCannon`), `FireCannon()` (barrel offset `tile * 0.5` along the aim from the cannon centre, emits the held `BulletItem` through `IBulletItemSpawner` with `BulletFaction.Player`, sets cooldown, clears HeldItem, raises `event Action Fired`), `float AimAngle` (radians, kept on the Rigidbody rotation), `ICannonAimSource AimSource` (`Vector2 GetMovement()`; null means unseated). Seat, eject search and `OnGrab/OnRelease` are DEFERRED: they need the player type and map tiles (`FindEjectPosition`, `IsTileFree`); document in CONVENTIONS. Muzzle flash, sounds dropped.
- `TrainSpeedSetting` (Core): `Stopped/Slow/Default/Fast`, `All = [Slow, Default, Fast]`, built from `TrainSpeedTuning` (150/300/600, burn 0.5/1/4). `ITrainState { bool VictoryLapActive; bool IsCoalOvenBurning; TrainSpeedSetting CurrentSpeed { get; set; } void SlowDownIfRunning(); }`.
- `SpeedLeverRuntime : StationRuntime`: `ITrainState State`, `Action<IPlayerActor> OnInteractOverride`, `OnInteract` per Src (override first; victory lap noop; oven out slows down; otherwise next of `All` cyclic, `NextIndex` exposed for the sound parameter).

### Task 9: CONVENTIONS and final verification

Rewrite "Not ported yet" in `Unity/CONVENTIONS.md`: for each item say ported / deferred (exact seam and reason) / dead code. Then full EditMode + PlayMode headless, `git diff main -- Src/` empty, `dotnet build Src/Gamelab.csproj`.

## Closing steps

Whole-branch review (most capable model), one consolidated fix wave, one scoped re-review, results XML totals.
