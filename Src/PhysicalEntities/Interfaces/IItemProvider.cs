using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface IItemProvider
{
    Item PeekNextItem();
    bool TryProvideItem(out Item item, IItemReceiver consumer = null);
    bool CanProvideItem(IItemReceiver consumer);

    void PingPullIntent(IItemReceiver consumer, float dt)
    {
    }
}

public class ConsumerTicket(IItemReceiver consumer)
{
    public IItemReceiver Consumer { get; } = consumer;
    public float TimeSinceLastPing { get; set; } = 0f;
}