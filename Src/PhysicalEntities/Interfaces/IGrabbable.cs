using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Interfaces;

public interface IGrabbable : IPhysicalEntity
{
    bool OnGrab(Player player, Vector2 grabPointWorldMeters);
    void OnRelease(Player player);
}