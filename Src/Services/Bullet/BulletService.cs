using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Services.Bullet;

public class BulletService : IGameSystem, IBulletService
{
    protected List<BulletEntity> bullets = new ();

    protected float gameTimeAccumulator;
    protected GamelabGame game;
    protected World world;
    protected SpriteBatch spriteBatch;
    
    public void Initialize(GamelabGame game)
    {
        this.game = game;
    }

    public void InitializePhysics(World world)
    {
        this.world = world;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;
        gameTimeAccumulator += Math.Min(dt, game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        float fixedDt = game.GameplayConfig.FixedTimeStep;
        while (gameTimeAccumulator >= fixedDt)
        {
            RemoveStaleBullets();
            foreach (var bullet in bullets)
            {
                bullet.OnUpdate(fixedDt);
            }
            gameTimeAccumulator -= fixedDt;
        }
    }

    public void Draw()
    {
    }

    public void Render(SpriteBatch sb)
    {
        foreach (var bullet in bullets) bullet.Draw(sb);
    }

    public void Shutdown()
    {
        foreach (var bullet in bullets) bullet.Cleanup();
        bullets.Clear();
    }

    public void EmitBullet(
        BulletItem bulletItem, 
        Vector2 position,
        Vector2 direction,
        IBulletEmitter owner)
    {
        BulletStats stats = new BulletStats(game.GameplayConfig, position, direction);
        BulletEntity bullet = new BulletEntity(bulletItem, stats, world, owner);
        bullets.Add(bullet);
        bullet.OnCreate();
        bullet.OnSpawn();
    }
    
    public void EmitAdditionalBullet(
        BulletItem bulletItem, 
        Vector2 position,
        Vector2 direction,
        IBulletEmitter owner)
    {
        BulletStats stats = new BulletStats(game.GameplayConfig, position, direction);
        BulletEntity bullet = new BulletEntity(bulletItem, stats, world, owner);
        bullet.IsRootEntity = false;
        bullets.Add(bullet);
        bullet.OnCreate();
        bullet.OnSpawn();
    }

    private void RemoveStaleBullets()
    {
        List<BulletEntity> staleBullets = bullets.Where(x => x == null || !x.IsActive).ToList();
        foreach (var bullet in staleBullets) bullet.Cleanup();
        bullets.RemoveAll(x => staleBullets.Contains(x));
    }
}