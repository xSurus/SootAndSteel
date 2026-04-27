using Gamelab.Items.Bullets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Bullets.Components.Propellants;

public class BoomerangPropellant : AbstractComponent
{
    public BoomerangPropellant()
    {
        Type = EComponentType.Propellant;
        ComponentId = ComponentIds.BoomerangPropellant;
    }

    public override void OnUpdate(BulletEntity bulletEntity, float deltaTime)
    {
        float returnAcceleration = bulletEntity.Stats.Speed / 2.0f;
        Vector2 backwardForce = bulletEntity.Stats.Direction * returnAcceleration * deltaTime;
        bulletEntity.PhysicsBody.LinearVelocity -= backwardForce.ToMeters();
    }
}