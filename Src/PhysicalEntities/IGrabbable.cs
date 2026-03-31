using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities;

public interface IGrabbable : IPhysicalEntity
{
    bool OnGrab(Player player, GameplayContext gameplayContext, Vector2 grabPointWorldMeters);
    void OnRelease(Player player, GameplayContext gameplayContext);
}