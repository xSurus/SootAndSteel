namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IInteractable : IPhysicalEntity
    {
        void OnInteract(IPlayerActor interactingPlayer)
        {
        }

        void OnInteractHeld(IPlayerActor interactingPlayer, float dt)
        {
        }

        void OnInteractReleased(IPlayerActor interactingPlayer)
        {
        }
    }
}
