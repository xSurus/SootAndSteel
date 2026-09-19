using UnityEngine;

namespace Gamelab.PhysicalEntities.Interfaces
{
    public interface IGrabbable : IPhysicalEntity
    {
        bool OnGrab(IPlayerActor player, Vector2 grabPointWorldMeters);
        void OnRelease(IPlayerActor player);
    }
}
