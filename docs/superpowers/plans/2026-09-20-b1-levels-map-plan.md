# B1 levels and map port plan

Branch `port/map` (from main e2543a7). `Src/` is read-only. Follow `Unity/CONVENTIONS.md` (C# 9, netstandard2.1, Core engine-free with `System.Numerics.Vector2`, Runtime for UnityEngine).
Gate: a full level loads with correct tile placement and working collision, matching the reference layout.

## Decisions

- Src frame: pixels, Y down. Unity physics stays in that frame in meters (`WorldUnits`, 100 px per meter). Train tile = 80 px = 0.8 m.
- Y flip: exactly one place, `Gamelab.Map.MapSpace` (Runtime). The world stays Y-down. The camera is mirrored by `MapSpace.ApplyTo(Camera)` (projection Y scale -1). Tile and sprite art must then be drawn upside down in the world so it shows upright, which `MapSpace.SpriteFlip` provides. Nothing else flips Y. Documented in CONVENTIONS.
- Sprite import: an editor postprocessor (`MapSpriteImportSettings`) applies Point / no compression / PPU by path rule, plus an EditMode test over the importers. PPU = tile width of the sheet (100 for train tiles, walls, decorations sized by their own sheet). Runtime scale = `srcScale * PPU / 100` (`MapSpace.SpriteScale`).
- Sprites load through `Resources` (`Assets/Resources/Map/...`), because tests need them without a scene.
- RNG: `System.Random(seed)`, `NextSingle()` is `(float)NextDouble()` (checked equal on .NET 10 for seeded instances). Golden values come from running the real Src generator (scratch project outside the repo referencing `Src/Gamelab.csproj`), pinned in EditMode tests. Unity Mono must produce the same values.
- Src items whose deps are not ported (RunSession, GameplayConfig instance, GamelabGame services) take plain parameters (seed, threat scale, spacing scale, tuning objects).

## Tasks (each: TDD, tests, commit, then task review)

1. Levels core. `Assets/Scripts/Core/Levels/`: LevelDefinition, SpawnEvent, LevelGenerationConfig, ILevelProvider, ProceduralLevelGenerator, ProceduralLevelProvider(seed, threatScale, spacingScale, config), LevelCompletionWatcher(Update(distance, hasThreats)). Golden tests for levels 1, 2, 5, 9 at several seeds, plus scale variants, determinism, sort order, safe zone, spacing.
2. Train geometry core. `Core/Map/`: `RectPx`, `TrainLayout` (width/height/tile size/top-left, tile centres, index from pixels with floor, bounds, `IsOnTrain`, boundary wall specs incl. doors and the left wall gap rows, free spawn tile, tile variants from a seeded Random, station cell dictionary with adjacency), `EnemyTrainSlot.GetAnchor(RectPx bounds, distance, topClearance)`, `MapBounds` (MinX 0, MaxX screen width). Tests against hand-computed Src numbers (default top-left (560,300), wall centres).
3. Train state and patches core. `Core/Map/Train/State/TrainState` (implements `ITrainState`, shared `TrainSpeedSetting.Set`, acceleration, distance, temperature, breached walls, OnSpeedChanged/OnTrainFrozen), `Core/Config/TrainStateTuning` (Src defaults), `PatchField` (snow/ice logic from PatchManager, injected Random and occupancy callback). Tests.
4. Tile assets. Copy PNGs from `Src/Content/` to `Unity/Assets/Resources/Map/` (Ice_Tile, Snow_Tile, Train_Tile_A/B, Rail_Tile_01/02, Walls/*, Decorations pine + Wheels1-3, Hub.png, Hub/*, NPCs Vendor + Town1). `Assets/Editor/MapSpriteImportSettings.cs`. EditMode test over importers (filter, compression, PPU per sheet). One slow Unity run for import.
5. Runtime map build. `Runtime/Map/`: MapSpace, MapSprites (loader), TrainMapBuilder/TrainMapRuntime: Grid + floor Tilemap (variants), patch overlay Tilemap, boundary walls as static BoxCollider2D (left wall tile colliders with the door gap), ShootHoleWallRuntime and DoorWallRuntime markers (health + door toggle logic where pure), SnapToNearestValidCell for stations, station registry. PlayMode tests: tiles match TrainLayout, entity blocked by wall, door gap passes.
6. Seams. `TrainMapRuntime` implements `IStationGrid` and `IEnemyWorld` (GetSlotAnchor, GetTargetPoint nearest ShootHoleWall on side else bounds centre, MinX/MaxX); `TrainStateRuntime` component wrapping TrainState. PlayMode tests with real conveyor and enemy where cheap.
7. World scroller and hub. Core `WorldScrollerModel` (tiles + trees with injected Random, Src constants), Runtime `WorldScrollerView`; `HubMapModel` (Core geometry: fence stakes, tree layout, house boundary rects, walls) and Runtime `HubMapView`. Tests.
8. Level load gate. `LevelRuntime` (definition, train state, watcher) and the PlayMode gate test: generate level, build map, assert placement vs generator/layout, collision, level completes when distance reached and no threats.
9. CONVENTIONS.md update (ported/deferred with seams, Y flip, integration notes for B2).

Final: whole-branch review (most capable model), one fix wave, one scoped re-review. Verify full EditMode and PlayMode, `git diff main -- Src/` empty, `dotnet build Src/Gamelab.csproj`, no stray `.meta`.
