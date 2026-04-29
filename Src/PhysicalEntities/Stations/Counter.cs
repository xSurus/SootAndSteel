using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations;

public class Counter(Vector2 position)
    : AbstractStation(StationIds.Counter, position)
{
    public override void OnPickup(Player interactingPlayer)
    {
        if ((interactingPlayer.HeldItem == null || HeldItem == null) &&
            (interactingPlayer.HeldItem != null || HeldItem != null))
        {
            soundService.PlayOnce(HeldItem == null ? Sounds.DropItem : Sounds.PickupItem);
            (interactingPlayer.HeldItem, HeldItem) = (HeldItem, interactingPlayer.HeldItem);
        }
    }
}