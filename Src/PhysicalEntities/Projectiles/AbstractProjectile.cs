using Gamelab.Assets;
using Gamelab.Entities;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Gamelab.PhysicalEntities.Projectiles;

public abstract class AbstractProjectile : AbstractPhysicalEntity
{
    public float Damage { get; }
    public bool IsActive { get; private set; } = true;
    public float Size { get; }
    protected abstract Color ProjectileColor { get; }

    private float lifetime;
    private bool pendingDisable;
    private readonly float maxLifetime;

    protected AbstractProjectile(
        World world,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float maxLifetime,
        float size)
    {
        Damage = damage;
        this.maxLifetime = maxLifetime;
        Size = size;

        float radiusMeters = (size / 2f).ToMeters();
        PhysicsBody = world.CreateCircle(radiusMeters, 1f, position.ToMeters(), BodyType.Dynamic);
        PhysicsBody.IgnoreGravity = true;
        PhysicsBody.IsBullet = true;
        PhysicsBody.FixedRotation = true;
        PhysicsBody.LinearVelocity = velocity.ToMeters();

        foreach (Fixture fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
            fixture.OnCollision += OnCollision;
        }
    }

    public virtual void Update(float deltaTime)
    {
        if (pendingDisable && PhysicsBody != null && PhysicsBody.Enabled)
        {
            PhysicsBody.Enabled = false;
            pendingDisable = false;
        }

        if (!IsActive)
        {
            return;
        }

        lifetime += deltaTime;
        if (lifetime >= maxLifetime)
        {
            Deactivate();
        }
    }

    public virtual void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        if (PhysicsBody != null)
        {
            pendingDisable = true;
        }
    }

    public void RemovePhysicsBody()
    {
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
        pendingDisable = false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive)
        {
            return;
        }

        Vector2 position = Position;
        Rectangle destinationRectangle = new Rectangle(
            (int)(position.X - Size / 2f),
            (int)(position.Y - Size / 2f),
            (int)Size,
            (int)Size
        );

        spriteBatch.Draw(AssetManager.BlankTexture, destinationRectangle, ProjectileColor);
    }

    private bool OnCollision(Fixture sender, Fixture other, Contact contact)
    {
        if (!IsActive || other.Body.Tag == PhysicsBody.Tag)
        {
            return false;
        }

        if (other.Body.Tag is IDamageable hittable)
        {
            // hittable.OnHit(this);
        }

        return false;
    }
}