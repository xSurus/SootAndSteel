using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class CoalResource(Vector2 position, GameplayContext gameplayContext)
    : AbstractResource("CoalResource", Color.Black, "Coal", position, gameplayContext)
{
    // overrides since it consumes coal from the train
    public override void OnPickup(Player interactingPlayer, GameplayContext gameplayContext)
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