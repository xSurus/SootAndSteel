using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Enemies.Hazards;
using Gamelab.PhysicalEntities.Hazards;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Projectiles;

public class MolotovProjectile : AbstractPhysicalEntity, IEnemyHazard
{
    private readonly Vector2 velocity;
    private readonly float radius;
    private readonly float fireRadius;
    private readonly float fireDurationSeconds;
    private float remainingLifetime;
    private FireZone pendingFireZone;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => false;

    public MolotovProjectile(
        Vector2 position,
        Vector2 velocity,
        float lifetimeSeconds,
        float radius,
        float fireRadius,
        float fireDurationSeconds)
    {
        this.velocity = velocity;
        remainingLifetime = lifetimeSeconds;
        this.radius = radius;
        this.fireRadius = fireRadius;
        this.fireDurationSeconds = fireDurationSeconds;

        PhysicsBody =
            gameplayContext.PhysicsWorld.CreateCircle((radius / 2f).ToMeters(), 1f, position.ToMeters(),
                BodyType.Static);
        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }
    }

    public void Update(float dt)
    {
        if (ShouldRemove)
        {
            return;
        }

        Position += velocity * dt;
        remainingLifetime -= dt;
        if (remainingLifetime > 0f)
        {
            return;
        }

        pendingFireZone = new FireZone(Position, fireRadius, fireDurationSeconds);
        ShouldRemove = true;
    }

    public IEnemyHazard TryCreateHazard()
    {
        if (pendingFireZone == null)
        {
            return null;
        }

        FireZone zone = pendingFireZone;
        pendingFireZone = null;
        return zone;
    }

    public void RemovePhysicsBody()
    {
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int size = (int)radius;
        Rectangle rect = new((int)(Position.X - size / 2f), (int)(Position.Y - size / 2f), size, size);
        spriteBatch.Draw(AssetManager.BlankTexture, rect, Color.DarkOrange);
    }
}