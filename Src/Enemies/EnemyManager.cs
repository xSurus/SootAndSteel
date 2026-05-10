using System;
using System.Collections.Generic;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Slots;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Enemies;

public class EnemyManager(LevelDefinition levelDef)
{
    private readonly List<AbstractEnemy> enemies = [];
    private readonly Random random = new(levelDef.LevelSeed);
    private readonly EnemySlotManager slotManager = new();
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    private int nextSpawnIndex;
    private float EnemySize => GamelabGame.Instance.GameplayConfig.EnemySize;
    private float RiflePreferredDistance => GamelabGame.Instance.GameplayConfig.RiflePreferredDistance;
    public bool HasActiveThreats => enemies.Count > 0;
    public bool HasAnyEnemyTakenDamage => enemies.Exists(e => e.Health < GamelabGame.Instance.GameplayConfig.EnemyHealth);

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
    }

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            AbstractEnemy enemy = enemies[i];
            enemy.Update(deltaTime);
            enemy.TryShoot();

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
        EnemyDefinition def = EnemyDefinition.Parse(spawnEvent.Type);
        EnemySlotSide side = spawnEvent.Side?.ToLowerInvariant() switch
        {
            "top" => EnemySlotSide.Top,
            "bottom" => EnemySlotSide.Bottom,
            _ => random.NextSingle() < 0.5f ? EnemySlotSide.Top : EnemySlotSide.Bottom
        };

        SpawnEnemy(side, def.Type);
    }

    private void SpawnEnemy(EnemySlotSide preferredSide, EnemyType type = EnemyType.Rifle)
    {
        if (!slotManager.TryReserveSideAttackSlotOnSide(preferredSide, out EnemyTrainSlot slot))
        {
            return;
        }

        Vector2 spawnPosition = GetSideAttackSpawnPosition(slot);
        AbstractEnemy enemy = EnemyFactory.Create(spawnPosition, slot, type);
        enemies.Add(enemy);
    }

    private Vector2 GetSideAttackSpawnPosition(EnemyTrainSlot slot)
    {
        var config = GamelabGame.Instance.GameplayConfig;
        float rightOverflow = (gameplayContext.ScreenWidth / config.CameraMaxZoom - gameplayContext.ScreenWidth) / 2f;
        float spawnX = gameplayContext.ScreenWidth + rightOverflow + config.EnemySpawnOffsetX;
        Vector2 anchor = slot.GetAnchor(EnemySize + RiflePreferredDistance);
        return new Vector2(spawnX, anchor.Y);
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
