using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IItemReceiver
    {
        bool CanReceiveItem(Item item, IItemProvider source);
        void ReceiveItem(Item item, IItemProvider source);

        void PingPushIntent(IItemProvider source, float dt)
        {
        }
    }

    public class ProviderTicket
    {
        public IItemProvider Provider { get; }
        public float TimeSinceLastPing { get; set; } = 0f;

        public ProviderTicket(IItemProvider provider)
        {
            Provider = provider;
        }
    }
}
