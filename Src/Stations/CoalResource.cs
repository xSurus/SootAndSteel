using Gamelab.Items;
using Gamelab.Map;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Stations;

public class CoalResource() : TileObject("CoalResource", Color.DarkGray)
{
    public override void Interact(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new Item("Coal", Color.Black);
        }
        else if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            interactingPlayer.HeldItem = null;
        }
    }
}