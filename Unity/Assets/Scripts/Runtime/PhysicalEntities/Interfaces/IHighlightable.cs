namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IHighlightable : IPhysicalEntity
    {
        bool IsHighlighted { get; }
        void OnHighlight(IPlayerActor player);
        void OnHighlightRemoved(IPlayerActor player);
    }
}
