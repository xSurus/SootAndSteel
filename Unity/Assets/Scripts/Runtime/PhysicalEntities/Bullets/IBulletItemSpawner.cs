using UnityEngine;

namespace Gamelab.PhysicalEntities.Bullets
{
    // Seam for Src IBulletService.EmitBullet(BulletItem, position, direction, shooter).
    public interface IBulletItemSpawner
    {
        BulletRuntime Emit(BulletRecipe recipe, Vector2 positionMeters, Vector2 direction, BulletFaction faction);
    }
}
