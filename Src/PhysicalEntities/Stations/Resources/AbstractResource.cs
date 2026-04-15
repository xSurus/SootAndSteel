using Gamelab.Items;
using Gamelab.Players;
using Microsoft.Xna.Framework;


namespace Gamelab.PhysicalEntities.Stations.Resources;

public abstract class AbstractResource(
    string stationType,
    Color stationColor,
    string resourceId,
    Vector2 position)
    : AbstractStation(stationType, stationColor, position)
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