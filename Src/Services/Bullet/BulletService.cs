using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Services.Bullet;

public class BulletService : IGameSystem, IBulletService
{
    private class PendingBullet(float timeRemaining, BulletEntity bulletEntity)
    {
        public float TimeRemaining = timeRemaining;
        public BulletEntity Bullet => bulletEntity;
    }
    
    protected List<BulletEntity> activeBullets = new();
    private List<PendingBullet> pendingBullets = new();
    protected float gameTimeAccumulator;
    protected GamelabGame game;

    public void Initialize(GamelabGame game)
    {
        this.game = game;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        gameTimeAccumulator += Math.Min(dt, game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        float fixedDt = game.GameplayConfig.FixedTimeStep;
        while (gameTimeAccumulator >= fixedDt)
        {
            RemoveStaleBullets();
            foreach (var bullet in activeBullets)
            {
                bullet.OnUpdate(fixedDt);
            }

            foreach (var bullet in pendingBullets)
            {
                bullet.TimeRemaining -= dt;
            }

            gameTimeAccumulator -= fixedDt;
        }

        if (pendingBullets.Count > 0)
        {
            List<BulletEntity> spawningBullets = new();
            spawningBullets.AddRange(pendingBullets
                .Where(b => b.TimeRemaining < 0f)
                .Select(b => b.Bullet));
            pendingBullets = pendingBullets.Where(b => b.TimeRemaining > 0f).ToList();
            foreach (var bullet in spawningBullets) bullet.OnSpawn();
            activeBullets.AddRange(spawningBullets);
        }
    }

    public void Draw()
    {
    }

    public void Render(SpriteBatch sb)
    {
        foreach (var bullet in activeBullets) bullet.Draw(sb);
    }

    public void Shutdown()
    {
        foreach (var bullet in activeBullets) bullet.Cleanup();
        activeBullets.Clear();
        foreach (var bullet in pendingBullets) bullet.Bullet.Cleanup();
        pendingBullets.Clear();
    }

    public BulletEntity EmitBullet(
        BulletItem bulletItem,
        Vector2 position,
        Vector2 direction,
        IBulletEmitter initialShooter)
    {
        BulletStats stats = new BulletStats(game.GameplayConfig, position, direction);
        GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
        BulletEntity bullet = new BulletEntity(bulletItem, stats, gameplayContext.PhysicsWorld, initialShooter);
        activeBullets.Add(bullet);
        bullet.OnCreate();
        bullet.OnSpawn();
        return bullet;
    }

    public BulletEntity EmitAdditionalBullet(
        BulletEntity bulletEntity,
        IBulletEffect spawningEffect,
        float delay = 0f)
    {
        BulletEntity bullet =
            new BulletEntity(bulletEntity, spawningEffect);
        pendingBullets.Add(new PendingBullet(delay, bullet));
        bullet.OnCreate();
        return bullet;
    }

    private void RemoveStaleBullets()
    {
        List<BulletEntity> staleBullets = activeBullets.Where(x => x == null || !x.IsActive).ToList();
        foreach (var bullet in staleBullets) bullet.Cleanup();
        activeBullets.RemoveAll(x => staleBullets.Contains(x));
    }
}