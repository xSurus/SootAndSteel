namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IPickable : IPhysicalEntity
    {
        void OnPickup(IPlayerActor interactingPlayer)
        {
        }

        void OnPickupHeld(IPlayerActor interactingPlayer, float dt)
        {
        }
    }
}
