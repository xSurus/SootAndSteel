using Gamelab.Items;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class CoalResource(Vector2 position)
    : AbstractResource("CoalResource", Color.Black, "Coal", position)
{
    // overrides since it consumes coal from the train
    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null && gameplayContext.State.CoalAmount > 0)
        {
            interactingPlayer.HeldItem = new Item("Coal");
            gameplayContext.State.ConsumeCoal(1);
        }
        else if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            interactingPlayer.HeldItem = null;
            gameplayContext.State.AddCoal(1);
        }
    }
}