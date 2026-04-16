using Gamelab.Players;

namespace Gamelab.PhysicalEntities;

public interface IHighlightable : IPhysicalEntity
{
    bool IsHighlighted { get; }
    void OnHighlight(Player player);
    void OnHighlightRemoved(Player player);
}