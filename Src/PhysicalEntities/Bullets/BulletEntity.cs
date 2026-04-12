using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FontStashSharp.Rasterizers.StbTrueTypeSharp;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Entities;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Gamelab.PhysicalEntities.Bullets;

public class BulletEntity : AbstractPhysicalEntity
{
    public BulletStats Stats { get; }
    public BulletItem Item { get; }
    public List<IBulletEffect> Effects { get; }
    public bool IsActive { get; set; } = true;
    public bool IsRootEntity { get; set; } = true;
    public IBulletEmitter Owner { get; set; }
    public float Age { get; set; } = 0f;
    public World World { get; set; }
    
    private float pierceCount;

    private Dictionary<IDamageable, float> hitCooldown = new();

    public BulletEntity(
        BulletItem ammo,
        BulletStats stats,
        World world,
        IBulletEmitter creator)
    {
        Stats = stats;
        Item = ammo;
        Effects = ammo.GetEffects();
        World = world;
        Owner = creator;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive) return;

        Vector2 position = Position;
        Rectangle destinationRectangle = new Rectangle(
            (int)(position.X - Stats.Size / 2f),
            (int)(position.Y - Stats.Size / 2f),
            (int)Stats.Size,
            (int)Stats.Size
        );
        
        spriteBatch.Draw(AssetManager.BlankTexture, destinationRectangle, Stats.Color);
    }

    public void OnCreate()
    {
        foreach (var effect in Effects) effect.OnCreate(this);
        
        Debug.Assert(PhysicsBody != null, "PhysicsBody needs to be defined on create by one of the effects.");

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

    public void Cleanup()
    {
        IsActive = false;
        foreach (var effect in Effects) effect.OnCleanup(this);
        Effects.Clear();
        PhysicsBody?.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }
}