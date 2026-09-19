using NUnit.Framework;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.Tests.Interfaces
{
    public class ItemProviderReceiverTests
    {
        private class FakeReceiver : IItemReceiver
        {
            public bool CanReceiveItem(Item item, IItemProvider source) => true;
            public void ReceiveItem(Item item, IItemProvider source) { }
        }

        [Test]
        public void ConsumerTicket_StartsAtZeroPingTime()
        {
            var ticket = new ConsumerTicket(new FakeReceiver());

            Assert.AreEqual(0f, ticket.TimeSinceLastPing);
        }

        [Test]
        public void SpriteRect_StoresAllFields()
        {
            var rect = new SpriteRect(1, 2, 3, 4);

            Assert.AreEqual(1, rect.X);
            Assert.AreEqual(2, rect.Y);
            Assert.AreEqual(3, rect.Width);
            Assert.AreEqual(4, rect.Height);
        }
    }
}
