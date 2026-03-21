using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class CoalResource(Vector2 position, TrainContext trainContext)
    : AbstractResource("CoalResource", Color.Black, "Coal", Color.Black, position, trainContext)
{
    // overrides since it consumes coal from the train
    public override void OnPickup(Player interactingPlayer, TrainContext trainContext)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new Item("Coal", Color.Black);
            trainContext.State.ConsumeCoal(1);
        }
        else if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            interactingPlayer.HeldItem = null;
            trainContext.State.AddCoal(1);
        }
    }
}