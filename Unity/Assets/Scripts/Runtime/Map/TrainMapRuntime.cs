using System.Collections.Generic;
using Gamelab.Enemies.Core;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.Map.Train
{
    /// <summary>
    /// Unity build of Src TrainMap floor, patches, walls and station cells. Physics stays in Src's
    /// pixel frame (Y down) in meters. Grid cell (x, y) is train tile (x, y): the Grid is scaled by
    /// TileSize/100 and its origin is the train top-left, so the cell centre is layout.GetTileCenterMeters.
    /// </summary>
    public sealed class TrainMapRuntime : MonoBehaviour, IStationGrid, IEnemyWorld
    {
        private const int FloorOrder = -5000;
        private const int PatchOrder = -4999;

        private readonly List<Tile> ownedTiles = new List<Tile>();
        private Tile snowTile;
        private Tile iceTile;
        private int patchVersion = -1;
        private PatchField patchSource;

        public TrainLayout Layout { get; private set; }
        public TrainGrid<StationRuntime> Stations { get; private set; }
        public PatchField Patches => patchSource;
        public Tilemap Floor { get; private set; }
        public Tilemap PatchOverlay { get; private set; }
        public Tilemap LeftWall { get; private set; }
        public IReadOnlyList<GameObject> Walls => walls;
        private readonly List<GameObject> walls = new List<GameObject>();
        private readonly List<ShootHoleWallRuntime> shootHoles = new List<ShootHoleWallRuntime>();
        private readonly List<NVector2> shootHoleCentersPx = new List<NVector2>();
        private System.Random wallRandom;

        public IReadOnlyList<ShootHoleWallRuntime> ShootHoleWalls => shootHoles;

        /// <summary>Horizontal cull extent for enemies (IWorldBounds). Default 0..1920.</summary>
        public MapBounds WorldBounds { get; set; } = new MapBounds();
        public float MinX => WorldBounds.MinX;
        public float MaxX => WorldBounds.MaxX;

        public static TrainMapRuntime Create(TrainLayout layout, System.Random tileRandom)
        {
            var root = new GameObject("TrainMap");
            var map = root.AddComponent<TrainMapRuntime>();
            map.Build(layout, tileRandom);
            return map;
        }

        public bool IsOnTrain(Vector2 positionPx) => Layout.IsOnTrain(MapSpace.ToNumerics(positionPx));

        private Tile NewTile(Sprite sprite, Tile.ColliderType collider)
        {
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = sprite;
            t.colliderType = collider;
            t.transform = MapSpace.SpriteFlip;
            ownedTiles.Add(t);
            return t;
        }

        private Tilemap NewTilemap(Transform grid, string name, bool withRenderer, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid, false);
            var map = go.AddComponent<Tilemap>();
            if (withRenderer)
            {
                var r = go.AddComponent<TilemapRenderer>();
                r.sortingOrder = order;
            }
            return map;
        }

        /// <summary>
        /// Builds the map. Note it rolls the layout's tile variants from tileRandom (mutating the layout),
        /// then seeds the wall damage-sprite variations from the same Random.
        /// </summary>
        private void Build(TrainLayout layout, System.Random tileRandom)
        {
            Layout = layout;
            Stations = new TrainGrid<StationRuntime>(layout);
            layout.RollTileVariants(tileRandom);
            wallRandom = new System.Random(tileRandom.Next());

            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(transform, false);
            var grid = gridGo.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            float s = layout.TileSize / WorldUnits.PixelsPerMeter;
            gridGo.transform.localScale = new Vector3(s, s, 1f);
            gridGo.transform.position = MapSpace.ToUnity(WorldUnits.ToMeters(layout.Position));

            Floor = NewTilemap(gridGo.transform, "Floor", true, FloorOrder);
            Tile[] variants =
            {
                NewTile(MapSprites.Get("Train_Tile_A"), Tile.ColliderType.None),
                NewTile(MapSprites.Get("Train_Tile_B"), Tile.ColliderType.None),
            };
            for (int x = 0; x < layout.Width; x++)
                for (int y = 0; y < layout.Height; y++)
                    Floor.SetTile(new Vector3Int(x, y, 0), variants[layout.GetTileVariant(x, y)]);

            PatchOverlay = NewTilemap(gridGo.transform, "Patches", true, PatchOrder);
            PatchOverlay.color = new Color(1f, 1f, 1f, 0.7f);
            snowTile = NewTile(MapSprites.Get("Snow_Tile"), Tile.ColliderType.None);
            iceTile = NewTile(MapSprites.Get("Ice_Tile"), Tile.ColliderType.None);

            // Grid collider type needs no sprite: the shape is the cell square.
            LeftWall = NewTilemap(gridGo.transform, "LeftWall", false);
            Tile wallTile = NewTile(null, Tile.ColliderType.Grid);
            foreach (TrainPoint p in layout.LeftWallTiles)
                LeftWall.SetTile(new Vector3Int(p.X, p.Y, 0), wallTile);
            LeftWall.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            LeftWall.gameObject.AddComponent<TilemapCollider2D>();

            foreach (WallSpec spec in layout.WallSpecs) BuildWall(spec);
        }

        private void BuildWall(WallSpec spec)
        {
            Vector2 centerM = MapSpace.PxToMeters(MapSpace.ToUnity(spec.CenterPx));
            Vector2 sizeM = MapSpace.PxToMeters(MapSpace.ToUnity(spec.SizePx));
            var go = new GameObject(spec.Kind == WallKind.Door ? "DoorWall" : "ShootHoleWall");
            go.transform.SetParent(transform, false);
            go.transform.position = centerM;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            go.AddComponent<BoxCollider2D>().size = sizeM;
            walls.Add(go);

            // Src feet: ShootHoleWall top feet at the bottom edge of the collider, bottom at the top edge.
            // Src DoorWall always uses the bottom edge (Position + (0, dim.Y / 2)), top or bottom door.
            bool feetAtBottomEdge = spec.IsTop || spec.Kind == WallKind.Door;
            float feetYPx = spec.CenterPx.Y + (feetAtBottomEdge ? spec.SizePx.Y / 2f : -spec.SizePx.Y / 2f);
            // Sprites live on children so placing them never moves the collider body.
            var sr = NewVisual(go.transform, "Visual");
            if (spec.Kind == WallKind.Door)
            {
                var door = go.AddComponent<DoorWallRuntime>();
                door.IsTop = spec.IsTop;
                sr.sprite = MapSprites.Get("Walls/WallTileTopDoor");
                door.StateRenderer = NewVisual(go.transform, "State");
                door.StateRenderer.sprite = MapSprites.Get("Walls/WallTileTopDoorClosed");
                PlaceSprite(sr, spec, feetYPx, 0);
                PlaceSprite(door.StateRenderer, spec, feetYPx, 1);
            }
            else
            {
                var hole = go.AddComponent<ShootHoleWallRuntime>();
                hole.Configure(spec, feetYPx, sr, wallRandom);
                shootHoles.Add(hole);
                shootHoleCentersPx.Add(spec.CenterPx);
            }
        }

        private static SpriteRenderer NewVisual(Transform parent, string name)
        {
            var v = new GameObject(name);
            v.transform.SetParent(parent, false);
            return v.AddComponent<SpriteRenderer>();
        }

        // Sprite pivot is the centre. In the Y-down frame the art's bottom edge is the larger Y, so the
        // centre sits half the sprite height above (smaller Y than) the feet.
        internal static void PlaceSprite(SpriteRenderer sr, WallSpec spec, float feetYPx, int orderOffset)
        {
            Sprite sprite = sr.sprite;
            float scale = MapSpace.SpriteScale(sprite.pixelsPerUnit, spec.SizePx.X / sprite.rect.width);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            float heightM = sprite.rect.height / sprite.pixelsPerUnit * scale;
            sr.transform.position = new Vector3(spec.CenterPx.X / WorldUnits.PixelsPerMeter,
                feetYPx / WorldUnits.PixelsPerMeter - heightM / 2f, 0f);
            MapSpace.ApplyToSprite(sr);
            sr.sortingOrder = Mathf.RoundToInt(feetYPx) + orderOffset;
        }

        /// <summary>Src SnapToNearestValidCell: moves the station to the cell centre and registers it.</summary>
        public bool SnapToNearestValidCell(StationRuntime station)
        {
            TrainPoint? cell = Layout.SnapCell(MapSpace.ToNumerics(station.Position * WorldUnits.PixelsPerMeter),
                Layout.CannonWagonBounds);
            if (!cell.HasValue) return false;
            station.Position = MapSpace.ToUnity(Layout.GetTileCenterMeters(cell.Value.X, cell.Value.Y));
            Stations.Register(cell.Value, station);
            return true;
        }

        /// <summary>
        /// Station wiring the map owns: sets Grid on conveyors and cannon stations, then snaps to a cell.
        /// Not wired here (player and station wave): CannonStationRuntime.Spawner (bullet spawner) and
        /// ConveyorRuntime.IsBeingHeld (set by the grabbing player).
        /// </summary>
        public bool Attach(StationRuntime station)
        {
            if (station is ConveyorRuntime c) c.Grid = this;
            else if (station is CannonStationRuntime k) k.Grid = this;
            return SnapToNearestValidCell(station);
        }

        public StationRuntime GetAdjacentStation(Vector2 positionMeters, GridDirection direction) =>
            Stations.GetAdjacent(MapSpace.ToNumerics(positionMeters * WorldUnits.PixelsPerMeter), direction);

        public NVector2 GetSlotAnchor(EnemyTrainSlot slot, float distanceFromTrainPx) =>
            slot.GetAnchor(Layout.GetBounds(), distanceFromTrainPx, Layout.TileSize);

        public NVector2 GetTargetPoint(EnemySlotSide side, NVector2 enemyPositionPx) =>
            EnemyWorldMath.GetTargetPoint(shootHoleCentersPx, side, enemyPositionPx, Layout.GetBounds());

        /// <summary>Src GameplayScreen OnWallBreached / OnWallRepaired: keeps numberBreachedWalls in step.</summary>
        public void Link(TrainStateRuntime train)
        {
            foreach (ShootHoleWallRuntime w in shootHoles)
            {
                w.Breached += () => train.State.numberBreachedWalls++;
                w.Repaired += () => train.State.numberBreachedWalls--;
            }
        }

        public void Refresh(PatchField patches)
        {
            if (patches == patchSource && patches.Version == patchVersion) return;
            patchSource = patches;
            patchVersion = patches.Version;
            PatchOverlay.ClearAllTiles();
            foreach (TrainPoint p in patches.SnowTiles) PatchOverlay.SetTile(new Vector3Int(p.X, p.Y, 0), snowTile);
            foreach (TrainPoint p in patches.IceTiles) PatchOverlay.SetTile(new Vector3Int(p.X, p.Y, 0), iceTile);
        }

        private void LateUpdate()
        {
            if (patchSource != null) Refresh(patchSource);
        }

        private void OnDestroy()
        {
            foreach (Tile t in ownedTiles)
                if (t != null) Destroy(t);
            ownedTiles.Clear();
        }
    }
}
