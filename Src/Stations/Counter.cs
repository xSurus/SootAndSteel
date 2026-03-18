using Gamelab.Map;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.Stations;

public class Counter() : AbstractStation("Counter", Color.SaddleBrown)
{
    public override void Interact(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null || HeldItem == null)
        {
            (interactingPlayer.HeldItem, HeldItem) = (HeldItem, interactingPlayer.HeldItem);
        }
    }
}