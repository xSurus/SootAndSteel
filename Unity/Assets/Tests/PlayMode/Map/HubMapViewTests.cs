using System.Collections;
using System.Linq;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.PhysicalEntities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gamelab.Tests.PlayMode
{
    public class HubMapViewTests
    {
        private HubMapModel model;
        private HubMapView view;
        private GameObject entityGo;

        [SetUp]
        public void SetUp()
        {
            model = new HubMapModel(2160, 2160);
            view = HubMapView.Create(model);
        }

        [TearDown]
        public void TearDown()
        {
            if (entityGo != null) Object.Destroy(entityGo);
            Object.Destroy(view.gameObject);
        }

        private PhysicalEntity NewEntity(Vector2 posMeters)
        {
            entityGo = new GameObject("Entity");
            var e = entityGo.AddComponent<PhysicalEntity>();
            e.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            e.Position = posMeters;
            return e;
        }

        private IEnumerator Push(PhysicalEntity e, Vector2 velocity, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                e.Body.linearVelocity = velocity;
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator Colliders_MatchModelCountsAndBounds()
        {
            yield return null;
            Assert.AreEqual(4, view.BoundaryColliders.Count);
            Assert.AreEqual(2, view.HouseColliders.Count);
            Assert.AreEqual(model.Stakes.Count, view.StakeColliders.Count);
            for (int i = 0; i < 4; i++) AssertBox(view.BoundaryColliders[i], model.BoundaryWalls[i]);
            for (int i = 0; i < 2; i++) AssertBox(view.HouseColliders[i], model.HouseColliders[i]);
            for (int i = 0; i < model.Stakes.Count; i++) AssertBox(view.StakeColliders[i], model.Stakes[i].Collider);
        }

        private static void AssertBox(BoxCollider2D c, HubMapModel.BoxPx b)
        {
            Assert.AreEqual(b.Center.X / 100f, c.bounds.center.x, 1e-4f);
            Assert.AreEqual(b.Center.Y / 100f, c.bounds.center.y, 1e-4f);
            Assert.AreEqual(b.Size.X / 100f, c.bounds.size.x, 1e-4f);
            Assert.AreEqual(b.Size.Y / 100f, c.bounds.size.y, 1e-4f);
            Assert.AreEqual(RigidbodyType2D.Static, c.attachedRigidbody.bodyType);
        }

        [UnityTest]
        public IEnumerator WorldBoundary_BlocksEntity()
        {
            yield return null;
            PhysicalEntity e = NewEntity(new Vector2(10f, 10f));
            yield return Push(e, new Vector2(-3f, 0f), 60);
            Assert.GreaterOrEqual(e.Position.x, 0f, "at " + e.Position);
        }

        [UnityTest]
        public IEnumerator House_BlocksEntity()
        {
            yield return null;
            // House 1 collider spans x 2.33..8.47 m, y 1.5 -/+ 2.66 m, so push up from below? Its bottom is 4.16 m.
            HubMapModel.BoxPx h = model.HouseColliders[0];
            float bottom = (h.Center.Y + h.Size.Y / 2f) / 100f;
            PhysicalEntity e = NewEntity(new Vector2(h.Center.X / 100f, bottom + 1f));
            yield return Push(e, new Vector2(0f, -3f), 60);
            Assert.Greater(e.Position.y, bottom, "at " + e.Position);
        }

        [UnityTest]
        public IEnumerator Stake_BlocksEntity()
        {
            yield return null;
            HubMapModel.Stake s = model.Stakes[0];
            HubMapModel.BoxPx c = s.Collider;
            float bottom = (c.Center.Y + c.Size.Y / 2f) / 100f;
            PhysicalEntity e = NewEntity(new Vector2(c.Center.X / 100f, bottom + 1f));
            yield return Push(e, new Vector2(0f, -3f), 60);
            Assert.Greater(e.Position.y, bottom, "at " + e.Position);
        }

        [UnityTest]
        public IEnumerator PrepTrain_SitsAtHubTopLeft_WithTopDoorAtMiddleColumn()
        {
            TrainMapRuntime map = HubMapView.CreatePrepTrain(model, new System.Random(7));
            yield return null;
            try
            {
                Assert.AreEqual(680f, map.Layout.Position.X, 1e-3f);
                Assert.AreEqual(1240f, map.Layout.Position.Y, 1e-3f);
                DoorWallRuntime[] doors = map.Walls.Select(g => g.GetComponent<DoorWallRuntime>())
                    .Where(d => d != null).ToArray();
                Assert.AreEqual(1, doors.Length);
                DoorWallRuntime door = doors[0];
                Assert.IsTrue(door.IsTop);
                Assert.AreEqual((680f + 5 * 80 + 40) / 100f, door.transform.position.x, 1e-3f);
            }
            finally
            {
                Object.Destroy(map.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Sprites_StakeCropIsFiftyPixelsWide()
        {
            yield return null;
            SpriteRenderer r = view.StakeRenderers[0];
            Assert.AreEqual(0.5f, r.bounds.size.x, 1e-3f);
            Assert.IsTrue(r.flipY);
            Assert.AreEqual(model.RailColumnCount, view.RailRenderers.Count);
        }
    }
}
