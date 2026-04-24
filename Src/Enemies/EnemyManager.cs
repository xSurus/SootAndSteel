using System;
using System.Collections.Generic;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Hazards;
using Gamelab.Enemies.Slots;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Enemies;

public class EnemyManager(LevelDefinition levelDef)
{
    private readonly List<AbstractEnemy> enemies = [];
    private readonly Random random = new(levelDef.LevelSeed);
    private readonly EnemySlotManager slotManager = new();
    private readonly EnemyHazardManager hazardManager = new();
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private int nextSpawnIndex;
    private float EnemySpawnOffsetX => GamelabGame.Instance.GameplayConfig.EnemySpawnOffsetX;
    private float EnemySize => GamelabGame.Instance.GameplayConfig.EnemySize;
    private float RiflePreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    public bool HasActiveThreats => enemies.Count > 0 || hazardManager.HasActiveThreats;

    public void Update(float deltaTime)
    {
        
        if (levelDef != null)
        {
            while (nextSpawnIndex < levelDef.SpawnEvents.Count &&
                   gameplayContext.State.DistanceTraveled >= levelDef.SpawnEvents[nextSpawnIndex].Distance)
            {
                SpawnFromEvent(levelDef.SpawnEvents[nextSpawnIndex]);
                nextSpawnIndex++;
            }
        }

        UpdateEnemies(deltaTime);
        hazardManager.Update(deltaTime);
    }

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            AbstractEnemy enemy = enemies[i];
            enemy.Update(deltaTime);
            enemy.TryShoot();

            IEnemyHazard hazard = enemy.TryCreateHazard();
            if (hazard != null)
            {
                hazardManager.Add(hazard);
            }

            if (!enemy.IsAlive || enemy.ShouldRemove)
            {
                ReleaseSlot(enemy);
                enemy.RemovePhysicsBody();
                enemies.RemoveAt(i);
            }
        }
    }
    
    private void SpawnFromEvent(SpawnEvent spawnEvent)
    {
        EnemyDefinition definition = EnemyDefinition.Parse(spawnEvent.Type);
        EnemySlotSide side = spawnEvent.Side?.ToLowerInvariant() switch
        {
            "top" => EnemySlotSide.Top,
            "bottom" => EnemySlotSide.Bottom,
            _ => random.NextSingle() < 0.5f ? EnemySlotSide.Top : EnemySlotSide.Bottom
        };

        SpawnEnemy(definition, side);
    }

    private void SpawnEnemy(EnemyDefinition definition, EnemySlotSide preferredSide)
    {
        if (!TryReserveSlot(definition.Type, preferredSide, out EnemyTrainSlot slot))
        {
            return;
        }

        Vector2 spawnPosition = GetSpawnPosition(definition.Type, slot);
        try
        {
            AbstractEnemy enemy = EnemyFactory.Create(definition, spawnPosition, slot);
            enemies.Add(enemy);
        }
        catch (NotSupportedException)
        {
            slotManager.ReleaseSlot(slot);
        }
    }

    private bool TryReserveSlot(EnemyType type, EnemySlotSide preferredSide, out EnemyTrainSlot slot)
    {
        return type switch
        {
            EnemyType.Mounter => slotManager.TryReserveMountSlotOnSide(preferredSide, out slot),
            EnemyType.Rifle or EnemyType.Shield or EnemyType.Molotov or EnemyType.TarThrower =>
                slotManager.TryReserveSideAttackSlotOnSide(preferredSide, out slot),
            EnemyType.Anchor => slotManager.TryReserveAnchorDeploySlotOnSide(preferredSide, out slot),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.")
        };
    }

    private Vector2 GetSpawnPosition(EnemyType type, EnemyTrainSlot slot)
    {
        return type switch
        {
            EnemyType.Mounter => GetMountSpawnPosition(slot),
            EnemyType.Rifle or EnemyType.Shield or EnemyType.Molotov or EnemyType.TarThrower or EnemyType.Anchor =>
                GetSideAttackSpawnPosition(slot),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type.")
        };
    }

    private Vector2 GetSideAttackSpawnPosition(EnemyTrainSlot slot)
    {
        float spawnX = gameplayContext.ScreenWidth + EnemySpawnOffsetX;
        Vector2 anchor = slot.GetAnchor(EnemySize + RiflePreferredDistance);
        return new Vector2(spawnX, anchor.Y);
    }

    private Vector2 GetMountSpawnPosition(EnemyTrainSlot slot)
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

        hazardManager.Draw(spriteBatch);
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
        hazardManager.Clear();
    }

    private void ReleaseSlot(AbstractEnemy enemy)
    {
        slotManager.ReleaseSlot(enemy.Slot);
    }
}