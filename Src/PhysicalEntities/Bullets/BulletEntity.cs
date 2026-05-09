using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Gamelab.PhysicalEntities.Bullets;

public class BulletEntity : AbstractPhysicalEntity
{
    public BulletStats Stats;
    public BulletItem Item { get; }
    public List<IBulletEffect> Effects { get; }
    public bool IsActive { get; set; } = true;
    public IBulletEmitter InitialShooter { get; set; }
    public float Age { get; set; } = 0f;
    public World World { get; set; }
    public BulletEntity ChildTemplate { get; set; }

    private float pierceCount;

    private Dictionary<IDamageable, float> hitCooldown = new();

    public BulletEntity(
        BulletItem ammo,
        BulletStats stats,
        World world,
        IBulletEmitter initialShooter)
    {
        Stats = stats;
        Item = ammo;
        Effects = ammo.GetEffects();
        World = world;
        InitialShooter = initialShooter;
    }

    public BulletEntity(BulletEntity parent, IBulletEffect spawningEffect = null)
    {
        Stats = parent.Stats;
        Item = parent.Item;
        World = parent.World;
        InitialShooter = parent.InitialShooter;
        ChildTemplate = parent.ChildTemplate;
        
        // Create copies of every effect
        Effects = new();
        foreach (var e in parent.Effects)
        {
            IBulletEffect effect = ComponentFactory.CreateDefinition(e.ComponentId);
            effect.Copy(e);
            if (spawningEffect != null && effect.Guid == spawningEffect.Guid)
            {
                effect.IsRootEffect = false;
            }
            Effects.Add(effect);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive) return;
        foreach (var effect in Effects) effect.OnDraw(this, spriteBatch);
    }

    public void OnCreate()
    {
        ChildTemplate = new BulletEntity(this);
        
        foreach (var effect in Effects) effect.OnCreate(this);

        Debug.Assert(PhysicsBody != null, "PhysicsBody needs to be defined on create by one of the effects.");
        if (PhysicsBody != null && Stats != null)
        {
            Vector2 currentVelocity = PhysicsBody.LinearVelocity;
            PhysicsBody.LinearVelocity = Vector2.Normalize(currentVelocity) * Stats.Speed.ToMeters();
        }

        PhysicsBody.OnCollision += OnCollision;
    }

    public void OnSpawn()
    {
        foreach (var effect in Effects) effect.OnSpawn(this);
    }

    public void OnUpdate(float deltaTime)
    {
        if (!IsActive) return;
        foreach (var effect in Effects) effect.OnUpdate(this, deltaTime);
        foreach (var hittable in hitCooldown.Keys)
            hitCooldown[hittable] -= deltaTime;
        hitCooldown = hitCooldown.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public void OnDraw(SpriteBatch spriteBatch)
    {
        if (!IsActive) return;
        foreach (var effect in Effects) effect.OnDraw(this, spriteBatch);
    }

    public bool OnCollision(Fixture sender, Fixture other, Contact contact)
    {
        if (IsActive
            && other.Body.Tag != PhysicsBody.Tag
            && other.Body.Tag is IDamageable hittable
            && !hitCooldown.ContainsKey(hittable)
            && hittable.OnHit(this))
        {
            foreach (var effect in Effects) effect.OnHit(this, hittable);
            pierceCount++;
            hitCooldown[hittable] = 1.0f;

            if (Stats.Pierce <= pierceCount)
            {
                IsActive = false;
            }
        }

        return false;
    }

    public void AddHitCooldown(IDamageable hitEntity, float cooldown)
    {
        hitCooldown[hitEntity] = cooldown;
    }

    public void Cleanup()
    {
        IsActive = false;
        foreach (var effect in Effects) effect.OnCleanup(this);
        Effects.Clear();
        PhysicsBody?.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public IBulletEffect GetEffect(IBulletEffect effect)
    {
        return Effects.Find(e => e.Guid == effect.Guid);
    }
}