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
    protected List<BulletEntity> activeBullets = new();
    private List<BulletEntity> pendingBullets = new();
    protected float gameTimeAccumulator;
    protected GamelabGame game;
    protected SpriteBatch spriteBatch;

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

            gameTimeAccumulator -= fixedDt;
        }

        if (pendingBullets.Count > 0)
        {
            activeBullets.AddRange(pendingBullets);
            pendingBullets.Clear();
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
        foreach (var bullet in pendingBullets) bullet.Cleanup();
        pendingBullets.Clear();
    }

    public void EmitBullet(
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
    }

    public void EmitAdditionalBullet(
        BulletItem bulletItem,
        Vector2 position,
        Vector2 direction,
        IBulletEmitter initialShooter,
        IBulletEmitter directEmitter = null)
    {
        BulletStats stats = new BulletStats(game.GameplayConfig, position, direction);
        GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
        BulletEntity bullet =
            new BulletEntity(bulletItem, stats, gameplayContext.PhysicsWorld, initialShooter, directEmitter);
        bullet.IsRootEntity = false;
        pendingBullets.Add(bullet);
        bullet.OnCreate();
        bullet.OnSpawn();
    }

    private void RemoveStaleBullets()
    {
        List<BulletEntity> staleBullets = activeBullets.Where(x => x == null || !x.IsActive).ToList();
        foreach (var bullet in staleBullets) bullet.Cleanup();
        activeBullets.RemoveAll(x => staleBullets.Contains(x));
    }
}