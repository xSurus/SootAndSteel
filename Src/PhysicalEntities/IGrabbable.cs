using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities;

public interface IGrabbable : IPhysicalEntity
{
    bool OnGrab(Player player, Vector2 grabPointWorldMeters);
    void OnRelease(Player player);
}