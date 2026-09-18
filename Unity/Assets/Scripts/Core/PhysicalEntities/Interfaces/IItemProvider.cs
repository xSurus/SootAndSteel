using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IItemProvider
    {
        Item PeekNextItem();
        bool TryProvideItem(out Item item, IItemReceiver consumer = null);
        bool CanProvideItem(IItemReceiver consumer);

        void PingPullIntent(IItemReceiver consumer, float dt)
        {
        }
    }

    public class ConsumerTicket
    {
        public IItemReceiver Consumer { get; }
        public float TimeSinceLastPing { get; set; } = 0f;

        public ConsumerTicket(IItemReceiver consumer)
        {
            Consumer = consumer;
        }
    }
}
