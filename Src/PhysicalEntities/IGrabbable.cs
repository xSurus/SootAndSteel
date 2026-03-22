using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities;

public interface IGrabbable : IPhysicalEntity
{
    bool OnGrab(Player player, TrainContext trainContext, Vector2 grabPointWorldMeters);
    void OnRelease(Player player, TrainContext trainContext);
}