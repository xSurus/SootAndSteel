using System.Collections;
using Gamelab.Config;
using Gamelab.Enemies;
using Gamelab.Enemies.Core;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.Tests.Bullets;
using Gamelab.Items;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.PhysicalEntities;
using Gamelab.PhysicalEntities.Stations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Gamelab.Tests.PlayMode
{
    public class TrainMapRuntimeTests
    {
        private TrainLayout layout;
        private TrainMapRuntime map;
        private GameObject entityGo;
        private GameObject extra;

        private static TrainLayout NewLayout(params DoorSpec[] doors)
        {
            var topLeft = TrainLayout.DefaultTopLeftPx(TrainTuning.ScreenWidth, TrainTuning.ScreenHeight,
                TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize);
            return new TrainLayout(TrainTuning.Width, TrainTuning.Height, TrainTuning.TileSize, topLeft, doors);
        }

        private void Build(params DoorSpec[] doors)
        {
            layout = NewLayout(doors);
            map = TrainMapRuntime.Create(layout, new System.Random(7));
        }

        [TearDown]
        public void TearDown()
        {
            if (entityGo != null) Object.Destroy(entityGo);
            if (extra != null) Object.Destroy(extra);
            if (map != null) Object.Destroy(map.gameObject);
        }

        private PhysicalEntity NewEntity(Vector2 pos)
        {
            entityGo = new GameObject("Entity");
            var e = entityGo.AddComponent<PhysicalEntity>();
            e.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            e.Position = pos;
            return e;
        }

        private static int CountTiles(Tilemap t)
        {
            int n = 0;
            foreach (Vector3Int p in t.cellBounds.allPositionsWithin)
                if (t.HasTile(p)) n++;
            return n;
        }

        private static IEnumerator Push(PhysicalEntity e, Vector2 velocity, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                e.Body.linearVelocity = velocity;
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator Floor_HasOneTilePerCell_WithExpectedSpritesAndCentres()
        {
            Build();
            yield return null;
            Assert.AreEqual(layout.Width * layout.Height, CountTiles(map.Floor));
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    string name = layout.GetTileVariant(x, y) == 0 ? "Train_Tile_A" : "Train_Tile_B";
                    Assert.AreSame(MapSprites.Get(name), map.Floor.GetSprite(cell));
                    Vector3 c = map.Floor.GetCellCenterWorld(cell);
                    var expected = layout.GetTileCenterMeters(x, y);
                    Assert.AreEqual(expected.X, c.x, 1e-4f);
                    Assert.AreEqual(expected.Y, c.y, 1e-4f);
                }
            }
        }

        [UnityTest]
        public IEnumerator FloorSprite_IsOneUnitWide_AndPointEightMetersInGrid()
        {
            Build();
            yield return null;
            Sprite sprite = map.Floor.GetSprite(new Vector3Int(0, 0, 0));
            Assert.AreEqual(1f, sprite.bounds.size.x, 1e-4f);
            Assert.AreEqual(0.8f, sprite.bounds.size.x * map.Floor.transform.lossyScale.x, 1e-4f);
        }

        [UnityTest]
        public IEnumerator LeftWallTile_BlocksEntity_DoorGapRowsPass()
        {
            Build();
            float wallEdge = layout.Position.X / 100f;
            yield return null;

            PhysicalEntity blocked = NewEntity(layout.GetTileCenterMeters(0, 0).ToUnity());
            yield return Push(blocked, new Vector2(-3f, 0f), 60);
            Assert.Greater(blocked.Position.x, wallEdge, "left wall tile should block, at " + blocked.Position);
            Object.Destroy(entityGo);

            foreach (int row in new[] { layout.DoorRowMin, layout.DoorRowMax })
            {
                PhysicalEntity through = NewEntity(layout.GetTileCenterMeters(0, row).ToUnity());
                yield return Push(through, new Vector2(-3f, 0f), 60);
                Assert.Less(through.Position.x, wallEdge - 0.2f, "gap row " + row + " should pass, at " + through.Position);
                Object.Destroy(entityGo);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator TopShootHoleWall_Blocks()
        {
            Build();
            yield return null;
            PhysicalEntity e = NewEntity(layout.GetTileCenterMeters(0, 0).ToUnity());
            yield return Push(e, new Vector2(0f, -3f), 60);
            // Wall spans 260.5..300.5 px, so its lower edge is y = 3.005 m.
            Assert.Greater(e.Position.y, 3.005f, "at " + e.Position);
        }

        [UnityTest]
        public IEnumerator TopDoorWall_BlocksWhenClosed_PassesAfterToggle()
        {
            Build(new DoorSpec(false, 0));
            yield return null;
            DoorWallRuntime door = map.Walls[0].GetComponent<DoorWallRuntime>();
            Assert.IsNotNull(door);
            Assert.IsTrue(door.IsTop);

            PhysicalEntity e = NewEntity(layout.GetTileCenterMeters(0, 0).ToUnity());
            yield return Push(e, new Vector2(0f, -3f), 60);
            Assert.Greater(e.Position.y, 3.005f, "closed door should block, at " + e.Position);

            door.Toggle();
            Assert.IsTrue(door.GetComponent<BoxCollider2D>().isTrigger);
            yield return Push(e, new Vector2(0f, -3f), 60);
            Assert.Less(e.Position.y, 2.6f, "open door should pass, at " + e.Position);
        }

        [UnityTest]
        public IEnumerator Walls_MatchSpecs_ColliderBoundsAndSpriteBounds()
        {
            Build();
            yield return null;
            Assert.AreEqual(layout.WallSpecs.Count, map.Walls.Count);
            for (int i = 0; i < map.Walls.Count; i++)
            {
                WallSpec spec = layout.WallSpecs[i];
                Bounds b = map.Walls[i].GetComponent<BoxCollider2D>().bounds;
                Assert.AreEqual(spec.CenterPx.X / 100f, b.center.x, 1e-4f);
                Assert.AreEqual(spec.CenterPx.Y / 100f, b.center.y, 1e-4f);
                Assert.AreEqual(spec.SizePx.X / 100f, b.size.x, 1e-4f);
                Assert.AreEqual(spec.SizePx.Y / 100f, b.size.y, 1e-4f);
                Assert.AreEqual(spec.IsTop, map.Walls[i].GetComponent<ShootHoleWallRuntime>().IsTop);

                // Sprite: tile wide (80 px) with its bottom edge at the Src feet.
                Bounds sb = map.Walls[i].GetComponentInChildren<SpriteRenderer>().bounds;
                float feet = spec.CenterPx.Y + (spec.IsTop ? spec.SizePx.Y / 2f : -spec.SizePx.Y / 2f);
                Assert.IsTrue(map.Walls[i].GetComponentInChildren<SpriteRenderer>().flipY);
                Assert.AreEqual(0.8f, sb.size.x, 1e-4f);
                Assert.AreEqual(feet / 100f, sb.max.y, 1e-4f);
                Assert.AreEqual(spec.CenterPx.X / 100f, sb.center.x, 1e-4f);
            }
        }

        [UnityTest]
        public IEnumerator Floor_TileMatrixIsSpriteFlip()
        {
            Build();
            yield return null;
            Assert.AreEqual(MapSpace.SpriteFlip, map.Floor.GetTransformMatrix(new Vector3Int(1, 1, 0)));
        }

        [UnityTest]
        public IEnumerator BottomDoor_SpriteFeetAtColliderBottomEdge_LikeSrcDoorWall()
        {
            Build(new DoorSpec(true, 2), new DoorSpec(false, 3));
            yield return null;
            for (int i = 0; i < map.Walls.Count; i++)
            {
                WallSpec spec = layout.WallSpecs[i];
                if (spec.Kind != WallKind.Door) continue;
                foreach (SpriteRenderer sr in map.Walls[i].GetComponentsInChildren<SpriteRenderer>())
                {
                    Assert.IsTrue(sr.flipY);
                    Assert.AreEqual((spec.CenterPx.Y + spec.SizePx.Y / 2f) / 100f, sr.bounds.max.y, 1e-4f,
                        (spec.IsTop ? "top" : "bottom") + " door");
                    Assert.AreEqual(spec.CenterPx.X / 100f, sr.bounds.center.x, 1e-4f);
                }
            }
        }

        [UnityTest]
        public IEnumerator MirroredCamera_KeepsMirror_WhenSizeChanges()
        {
            var go = new GameObject("Cam");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<MirroredCamera>();
            yield return null;
            Assert.Less(cam.WorldToViewportPoint(new Vector3(0f, 2f, 0f)).y, cam.WorldToViewportPoint(Vector3.zero).y);
            cam.orthographicSize = 3f;
            yield return null;
            float y = cam.WorldToViewportPoint(new Vector3(0f, 2f, 0f)).y;
            Assert.Less(y, cam.WorldToViewportPoint(Vector3.zero).y);
            Assert.AreEqual(0.5f - 2f / 6f, y, 1e-4f); // mirrored and rescaled to the new size
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator PatchOverlay_FollowsPatchField_AndClears()
        {
            Build();
            var field = new PatchField(layout, TrainStateTuning.Default, new System.Random(3), p => false);
            for (int i = 0; i < 40; i++) field.Update(3f, 0f);
            Assert.Greater(field.SnowTiles.Count + field.IceTiles.Count, 0);
            map.Refresh(field);
            yield return null;
            Assert.AreEqual(field.SnowTiles.Count + field.IceTiles.Count, CountTiles(map.PatchOverlay));
            foreach (TrainPoint p in field.SnowTiles)
                Assert.AreSame(MapSprites.Get("Snow_Tile"), map.PatchOverlay.GetSprite(new Vector3Int(p.X, p.Y, 0)));
            foreach (TrainPoint p in field.IceTiles)
                Assert.AreSame(MapSprites.Get("Ice_Tile"), map.PatchOverlay.GetSprite(new Vector3Int(p.X, p.Y, 0)));

            // Warm up: everything melts.
            for (int i = 0; i < 200; i++) field.Update(10f, 100f);
            Assert.AreEqual(0, field.SnowTiles.Count + field.IceTiles.Count);
            map.Refresh(field);
            Assert.AreEqual(0, CountTiles(map.PatchOverlay));
        }

        [UnityTest]
        public IEnumerator SnapToNearestValidCell_MovesStationToCellCentre_AndRegisters()
        {
            Build();
            extra = new GameObject("Counter");
            extra.AddComponent<Rigidbody2D>();
            var counter = extra.AddComponent<CounterRuntime>();
            counter.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            yield return null;
            // Inside tile (2, 1), off centre.
            counter.Position = layout.GetTileCenterMeters(2, 1).ToUnity() + new Vector2(0.2f, -0.1f);

            Assert.IsTrue(map.SnapToNearestValidCell(counter));
            Vector2 expected = layout.GetTileCenterMeters(2, 1).ToUnity();
            Assert.AreEqual(expected.x, counter.Position.x, 1e-4f);
            Assert.AreEqual(expected.y, counter.Position.y, 1e-4f);
            Assert.IsTrue(map.Stations.TryGet(new TrainPoint(2, 1), out StationRuntime found));
            Assert.AreSame(counter, found);
            Assert.IsTrue(map.IsOnTrain(expected * 100f));
        }

        [Test]
        public void MapSpace_SpriteScale_MatchesSrcWidth()
        {
            Assert.AreEqual(1.4f * 233f / 100f, MapSpace.SpriteScale(233f, 1.4f), 1e-5f);
            Assert.AreEqual(0.8f, MapSpace.SpriteScale(100f, 0.8f), 1e-6f);
        }

        [Test]
        public void MapSpace_ApplyTo_MirrorsY()
        {
            var go = new GameObject("Cam");
            try
            {
                var cam = go.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                go.transform.position = new Vector3(0f, 0f, -10f);
                float upright = cam.WorldToViewportPoint(new Vector3(0f, 2f, 0f)).y;
                float baseline = cam.WorldToViewportPoint(Vector3.zero).y;
                Assert.Greater(upright, baseline, "unflipped: larger Y is higher");
                MapSpace.ApplyTo(cam);
                Assert.Less(cam.WorldToViewportPoint(new Vector3(0f, 2f, 0f)).y,
                    cam.WorldToViewportPoint(Vector3.zero).y, "flipped: larger Y is lower");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    internal static class NumericsExt
    {
        public static Vector2 ToUnity(this System.Numerics.Vector2 v) => MapSpace.ToUnity(v);
    }
}
