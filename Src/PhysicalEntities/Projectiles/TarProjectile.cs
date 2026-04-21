using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Enemies.Hazards;
using Gamelab.Enemies.Slots;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Hazards;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Projectiles;

public class TarProjectile : AbstractPhysicalEntity, IEnemyHazard
{
    private readonly Vector2 velocity;
    private readonly float radius;
    private readonly EnemySlotSide side;
    private readonly float cleanDurationSeconds;
    private float remainingLifetime;
    private TarPatch pendingTarPatch;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => false;

    public TarProjectile(
        Vector2 position,
        Vector2 velocity,
        float lifetimeSeconds,
        float radius,
        EnemySlotSide side,
        float cleanDurationSeconds)
    {
        this.velocity = velocity;
        remainingLifetime = lifetimeSeconds;
        this.radius = radius;
        this.side = side;
        this.cleanDurationSeconds = cleanDurationSeconds;

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

        pendingTarPatch = new TarPatch(side, cleanDurationSeconds);
        ShouldRemove = true;
    }

    public IEnemyHazard TryCreateHazard()
    {
        if (pendingTarPatch == null)
        {
            return null;
        }

        TarPatch patch = pendingTarPatch;
        pendingTarPatch = null;
        return patch;
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
        spriteBatch.Draw(AssetManager.BlankTexture, rect, Color.Black);
    }
}