using System;
using System.Collections.Generic;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Enemies;

public class EnemyManager
{
    private readonly List<AbstractEnemy> enemies = [];
    private readonly Random random;
    private readonly EnemySlotManager slotManager;
    private readonly LevelDefinition levelDefinition;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private float timeSinceLastSpawn;
    private float currentSpawnInterval;
    private int nextSpawnIndex;
    private float ShooterSpawnChance => GamelabGame.Instance.GameplayConfig.ShooterSpawnChance;
    private float EnemySpawnIntervalBase => GamelabGame.Instance.GameplayConfig.EnemySpawnIntervalBase;
    private float EnemySpawnIntervalVariance => GamelabGame.Instance.GameplayConfig.EnemySpawnIntervalVariance;
    private float EnemySpawnOffsetX => GamelabGame.Instance.GameplayConfig.EnemySpawnOffsetX;
    private float EnemySize => GamelabGame.Instance.GameplayConfig.EnemySize;
    private float ShooterPreferredDistance => GamelabGame.Instance.GameplayConfig.ShooterPreferredDistance;
    private readonly ProjectileManager projectileManager;

    public IReadOnlyList<AbstractEnemy> Enemies => enemies;

    public EnemyManager(Random random, LevelDefinition levelDef, ProjectileManager projectileManager)
    {
        this.currentSpawnInterval = GamelabGame.Instance.GameplayConfig.EnemySpawnIntervalBase;
        this.levelDefinition = levelDef;
        this.random = random;
        this.slotManager = new EnemySlotManager(random);
        this.projectileManager = projectileManager;
        gameplayContext.Events.OnCannonProjectileFired += AddCannonProjectile;
    }

    public void Update(float deltaTime)
    {
        if (levelDefinition != null)
        {
            while (nextSpawnIndex < levelDefinition.SpawnEvents.Count &&
                   gameplayContext.State.DistanceTraveled >= levelDefinition.SpawnEvents[nextSpawnIndex].Distance)
            {
                // Spawn *all* events in order. Filtering by index can accidentally remove specific enemy types
                // depending on how levels are authored.
                SpawnFromEvent(levelDefinition.SpawnEvents[nextSpawnIndex]);

                nextSpawnIndex++;
            }
        }
        else if (gameplayContext.State.actualSpeed > 0)
        {
            timeSinceLastSpawn += deltaTime;
            if (timeSinceLastSpawn >= currentSpawnInterval)
            {
                SpawnEnemy();
                timeSinceLastSpawn = 0f;
                float nextInterval = EnemySpawnIntervalBase +
                                     (random.NextSingle() - 0.5f) * EnemySpawnIntervalVariance;
                currentSpawnInterval = nextInterval;
            }
        }

        UpdateEnemies(deltaTime);
    }

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            AbstractEnemy enemy = enemies[i];
            enemy.Update(deltaTime);

            if (enemy is ShooterEnemy shooter)
            {
                EnemyProjectile projectile = shooter.TryShoot();
                if (projectile != null)
                {
                    projectileManager.Add(projectile);
                }
            }

            if (!enemy.IsAlive || enemy.ShouldRemove)
            {
                ReleaseSlot(enemy);
                enemy.RemovePhysicsBody();
                enemies.RemoveAt(i);
            }
        }
    }

    private void AddCannonProjectile(CannonProjectile projectile)
    {
        projectileManager.Add(projectile);
    }

    private void SpawnEnemy()
    {
        bool shouldSpawnShooter = random.NextSingle() < ShooterSpawnChance;

        if (shouldSpawnShooter)
        {
            if (slotManager.TryReserveShooterSlot(out EnemyTrainSlot shooterSlot))
            {
                Vector2 spawnPosition = GetShooterSpawnPosition(shooterSlot);
                enemies.Add(new ShooterEnemy(gameplayContext, spawnPosition, random, shooterSlot));
                return;
            }
        }

        if (slotManager.TryReserveThiefSlot(out EnemyTrainSlot thiefSlot))
        {
            Vector2 spawnPosition = GetThiefSpawnPosition(thiefSlot);
            enemies.Add(new ThiefEnemy(gameplayContext, spawnPosition, thiefSlot));
        }
    }

    private void SpawnFromEvent(SpawnEvent spawnEvent)
    {
        if (spawnEvent.Type == "Shooter")
        {
            EnemySlotSide preferredSide = spawnEvent.Side?.ToLowerInvariant() switch
            {
                "top" => EnemySlotSide.Top,
                "bottom" => EnemySlotSide.Bottom,
                _ => random.NextSingle() < 0.5f ? EnemySlotSide.Top : EnemySlotSide.Bottom
            };

            if (slotManager.TryReserveShooterSlotOnSide(preferredSide, out EnemyTrainSlot shooterSlot))
            {
                Vector2 spawnPosition = GetShooterSpawnPosition(shooterSlot);
                enemies.Add(new ShooterEnemy(gameplayContext, spawnPosition, random, shooterSlot));
            }
        }
        else
        {
            EnemySlotSide side = spawnEvent.Side?.ToLowerInvariant() switch
            {
                "top" => EnemySlotSide.Top,
                "bottom" => EnemySlotSide.Bottom,
                _ => EnemySlotSide.Top
            };

            if (slotManager.TryReserveThiefSlotOnSide(side, out EnemyTrainSlot thiefSlot))
            {
                Vector2 spawnPosition = GetThiefSpawnPosition(thiefSlot);
                enemies.Add(new ThiefEnemy(gameplayContext, spawnPosition, thiefSlot));
            }
        }
    }

    private Vector2 GetShooterSpawnPosition(EnemyTrainSlot slot)
    {
        float spawnX = gameplayContext.ScreenWidth + EnemySpawnOffsetX;
        Vector2 anchor = slot.GetAnchor(gameplayContext, EnemySize + ShooterPreferredDistance);

        return new Vector2(spawnX, anchor.Y);
    }

    private Vector2 GetThiefSpawnPosition(EnemyTrainSlot slot)
    {
        return slot.Side switch
        {
            EnemySlotSide.Top => new Vector2(
                gameplayContext.Map.GetBounds().Left + gameplayContext.Map.GetBounds().Width * slot.PositionRatio,
                -EnemySpawnOffsetX
            ),
            EnemySlotSide.Bottom => new Vector2(
                gameplayContext.Map.GetBounds().Left + gameplayContext.Map.GetBounds().Width * slot.PositionRatio,
                gameplayContext.ScreenHeight + EnemySpawnOffsetX
            ),
            _ => new Vector2(gameplayContext.Map.GetBounds().Center.X, -EnemySpawnOffsetX)
        };
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (AbstractEnemy enemy in enemies)
        {
            enemy.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        foreach (AbstractEnemy enemy in enemies)
        {
            ReleaseSlot(enemy);
            enemy.RemovePhysicsBody();
        }

        enemies.Clear();
        slotManager.Clear();
    }

    private void ReleaseSlot(AbstractEnemy enemy)
    {
        slotManager.ReleaseSlot(enemy.Slot);
    }
}