using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Interactable.Stations;

public class CoalResource() : AbstractStation("CoalResource", Color.DarkGray)
{
    public override void Interact(Player interactingPlayer, TrainContext trainContext)
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