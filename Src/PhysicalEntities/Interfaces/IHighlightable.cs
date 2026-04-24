using Gamelab.Players;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface IHighlightable : IPhysicalEntity
{
    bool IsHighlighted { get; }
    void OnHighlight(Player player);
    void OnHighlightRemoved(Player player);
}