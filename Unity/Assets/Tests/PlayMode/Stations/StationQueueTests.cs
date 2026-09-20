// Unity/Assets/Tests/PlayMode/Stations/StationQueueTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    // Queues are ticked by StationRuntime's Update message, so these tests only wait real frames.
    // Src/AbstractStation special-cases "consumer is Player" as always first in line. Player is not
    // ported, so that carve-out is not covered here.
    public class StationQueueTests
    {
        private class QueueCounter : CounterRuntime
        {
            public int ProviderTicketCount => providerQueue.Count;
        }

        private static CounterRuntime MakeCounter(string name) => MakeCounter<CounterRuntime>(name);

        private static T MakeCounter<T>(string name) where T : CounterRuntime
        {
            var go = new GameObject(name);
            go.AddComponent<Rigidbody2D>();
            var c = go.AddComponent<T>();
            c.Initialize(new StationCatalogEntry { stationId = StationIds.Counter });
            return c;
        }

        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        // Pings every frame for the duration.
        private static IEnumerator PingFor(CounterRuntime provider, CounterRuntime consumer, float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                provider.PingPullIntent(consumer, 0f);
                yield return null;
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var c in Object.FindObjectsByType<CounterRuntime>(FindObjectsSortMode.None))
                Object.Destroy(c.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondConsumer_WaitsUntilFirstIsServed()
        {
            var provider = MakeCounter("P");
            var first = MakeCounter("A");
            var second = MakeCounter("B");
            provider.ReceiveItem(new Item("Coal"), null);

            provider.PingPullIntent(first, 0f);
            provider.PingPullIntent(second, 0f);
            yield return null;

            Assert.IsFalse(provider.CanProvideItem(second));
            Assert.IsFalse(provider.TryProvideItem(out _, second));
            Assert.IsTrue(provider.CanProvideItem(first));
            Assert.IsTrue(provider.TryProvideItem(out Item item, first));
            Assert.AreEqual("Coal", item.Id);

            // Served ticket is removed, so the second is now first in line.
            provider.ReceiveItem(new Item("Coal"), null);
            Assert.IsTrue(provider.CanProvideItem(second));
        }

        [UnityTest]
        public IEnumerator SilentConsumer_IsKickedAfterTimeUntilKickAndNextGetsItem()
        {
            var provider = MakeCounter("P");
            var first = MakeCounter("A");
            var second = MakeCounter("B");
            provider.ReceiveItem(new Item("Coal"), null);
            provider.PingPullIntent(first, 0f);
            provider.PingPullIntent(second, 0f);

            // Only the second keeps pinging.
            yield return PingFor(provider, second, 0.2f);
            Assert.IsFalse(provider.CanProvideItem(second), "First must not be kicked at 0.2 s.");

            yield return PingFor(provider, second, 0.2f);
            Assert.IsFalse(provider.CanProvideItem(second), "First must still be first at 0.4 s (double-tick check).");
            Assert.IsTrue(provider.CanProvideItem(first));

            yield return PingFor(provider, second, 0.4f);
            Assert.IsTrue(provider.CanProvideItem(second), "First should be kicked by 0.8 s.");
            Assert.IsTrue(provider.TryProvideItem(out _, second));
        }

        [UnityTest]
        public IEnumerator ConsumerThatKeepsPinging_IsNotKicked()
        {
            var provider = MakeCounter("P");
            var first = MakeCounter("A");
            var second = MakeCounter("B");
            provider.ReceiveItem(new Item("Coal"), null);

            provider.PingPullIntent(first, 0f);
            provider.PingPullIntent(second, 0f);
            float end = Time.time + 0.8f;
            while (Time.time < end)
            {
                provider.PingPullIntent(first, 0f);
                provider.PingPullIntent(second, 0f);
                yield return null;
            }

            Assert.IsTrue(provider.CanProvideItem(first));
            Assert.IsFalse(provider.CanProvideItem(second));
        }

        [UnityTest]
        public IEnumerator ProviderTicket_IsKickedLikeConsumerTicket()
        {
            var receiver = MakeCounter<QueueCounter>("R");
            var pinger = MakeCounter("P");
            receiver.PingPushIntent(pinger, 0f);

            yield return Wait(0.2f);
            Assert.AreEqual(1, receiver.ProviderTicketCount, "Not kicked at 0.2 s.");

            yield return Wait(0.6f);
            Assert.AreEqual(0, receiver.ProviderTicketCount, "Kicked by 0.8 s.");
        }

        [UnityTest]
        public IEnumerator NullConsumer_BypassesQueue()
        {
            var provider = MakeCounter("P");
            provider.ReceiveItem(new Item("Coal"), null);
            provider.PingPullIntent(MakeCounter("A"), 0f);
            yield return null;

            Assert.IsTrue(provider.TryProvideItem(out Item item));
            Assert.AreEqual("Coal", item.Id);
        }
    }
}
