namespace Gamelab.PhysicalEntities.Interfaces
{
    // Integration point for A4 (Src/Players/): A4's `Player` MonoBehaviour should
    // implement this marker interface so it satisfies IGrabbable/IHighlightable/
    // IInteractable/IPickable/ICannonSeat without this subsystem depending on A4's
    // concrete Player type.
    public interface IPlayerActor
    {
    }
}
