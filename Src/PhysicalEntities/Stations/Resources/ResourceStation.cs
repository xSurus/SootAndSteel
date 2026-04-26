using Gamelab.Items;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ResourceStation(
    Vector2 position,
    string resourceId)
    : AbstractStation(StationIds.GetResourceStationId(resourceId), position)
{
    protected string ResourceId { get; } = resourceId;

    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new Item(ResourceId);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId)
        {
            interactingPlayer.HeldItem = null;
        }
    }
}