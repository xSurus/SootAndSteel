using Gamelab.Items;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class CoalResourceStation : ResourceStation
{
    public CoalResourceStation(Vector2 position)
        : base(position, "Coal")
    {
        soundService.LoadSound(Sounds.ShovelUp);
        soundService.LoadSound(Sounds.ShovelDown);
    }

    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new Item(ResourceId);
            soundService.PlayOnce(Sounds.ShovelUp);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId)
        {
            interactingPlayer.HeldItem = null;
            soundService.PlayOnce(Sounds.ShovelDown);
        }
    }
}