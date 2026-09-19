// Unity/Assets/Tests/PlayMode/Stations/StationRuntimeSpawnTests.cs
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class StationRuntimeSpawnTests
    {
        [UnityTest]
        public IEnumerator Spawn_FromCatalogEntry_CounterAcceptsOneItemAtATime()
        {
            var catalog = ScriptableObject.CreateInstance<StationCatalogAsset>();
            var entry = new StationCatalogEntry { stationId = StationIds.Counter, displayName = "Counter" };
            typeof(StationCatalogAsset).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(catalog, new List<StationCatalogEntry> { entry });

            var go = new GameObject("Counter");
            go.AddComponent<Rigidbody2D>();
            var counter = go.AddComponent<CounterRuntime>();
            counter.Initialize(catalog.Get(StationIds.Counter));

            yield return null;

            Assert.AreEqual(StationIds.Counter, counter.StationId);
            Assert.IsTrue(counter.CanReceiveItem(new Item("Coal"), null));

            counter.ReceiveItem(new Item("Coal"), null);
            Assert.IsFalse(counter.CanReceiveItem(new Item("Coal"), null), "Counter should hold at most one item.");

            Assert.IsTrue(counter.TryProvideItem(out Item provided));
            Assert.AreEqual("Coal", provided.Id);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator TryProvideItem_NullConsumer_IgnoresQueuedConsumers()
        {
            var go = new GameObject("Counter");
            go.AddComponent<Rigidbody2D>();
            var counter = go.AddComponent<CounterRuntime>();
            counter.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            counter.ReceiveItem(new Item("Coal"), null);
            var other = new GameObject("Other");
            other.AddComponent<Rigidbody2D>();
            counter.PingPullIntent(other.AddComponent<CounterRuntime>(), 0f);

            yield return null;

            Assert.IsTrue(counter.TryProvideItem(out Item item), "Player-style (null consumer) pickup bypasses the FIFO queue.");
            Assert.AreEqual("Coal", item.Id);
            Object.Destroy(go);
        }
    }
}
