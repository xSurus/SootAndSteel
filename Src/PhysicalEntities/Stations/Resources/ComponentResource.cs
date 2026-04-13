using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ComponentResource(Vector2 position, string componentId)
    : AbstractResource(
        "ComponentResource",
        ComponentRegistry.GetColor(componentId),
        "Bullet",
        position)
{

    public string componentId = componentId;
    
    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new BulletItem(componentId);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId
                 && ((BulletItem)interactingPlayer.HeldItem).ComponentIds.SequenceEqual([componentId]))
        {
            interactingPlayer.HeldItem = null;
        }
    }
}