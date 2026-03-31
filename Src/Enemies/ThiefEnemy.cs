using System;
using Gamelab.Config;
using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Enemies;

public enum ThiefState
{
    Approaching,
    Stealing,
    Fleeing
}

public class ThiefEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, EnemyTrainSlot slot)
    : AbstractEnemy(gameplayContext, spawnPosition, slot)
{
    public override Color EnemyColor => currentState switch
    {
        ThiefState.Approaching => Color.Purple,
        ThiefState.Stealing => Color.Yellow,
        ThiefState.Fleeing => Color.Green,
        _ => Color.Purple
    };
    private float ApproachSpeed => GamelabGame.Instance.GameplayConfig.ThiefApproachSpeed;
    private float FleeSpeed => GamelabGame.Instance.GameplayConfig.ThiefFleeSpeed;
    private float StealDuration => GamelabGame.Instance.GameplayConfig.ThiefStealDuration;
    private int CoalToSteal => GamelabGame.Instance.GameplayConfig.ThiefCoalAmount;

    private ThiefState currentState = ThiefState.Approaching;
    private float stealTimer;
    private bool hasStolen;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        switch (currentState)
        {
            case ThiefState.Approaching:
                UpdateApproaching(deltaTime);
                break;
            case ThiefState.Stealing:
                UpdateStealing(deltaTime);
                break;
            case ThiefState.Fleeing:
                UpdateFleeing(deltaTime);
                break;
        }
    }

    private void UpdateApproaching(float deltaTime)
    {
        Vector2 targetPosition = GetTargetPosition();
        float distance = Vector2.Distance(Position, targetPosition);

        if (distance > 5f)
        {
            UpdateApproachMovement(deltaTime, targetPosition);
        }
        else
        {
            currentState = ThiefState.Stealing;
            stealTimer = 0f;
        }
    }
    
    private Vector2 GetTargetPosition()
    {
        float margin = Size / 2 + 10;
        return Slot.GetAnchor(gameplayContext, margin);
    }

    private void UpdateApproachMovement(float deltaTime, Vector2 targetPosition)
    {
        MoveTowards(targetPosition, ApproachSpeed * deltaTime);
    }
    
    private void UpdateStealing(float deltaTime)
    {
        Vector2 targetPosition = GetTargetPosition();
        UpdateApproachMovement(deltaTime, targetPosition);
        
        stealTimer += deltaTime;
        
        if (stealTimer >= StealDuration && !hasStolen)
        {
            if (gameplayContext.State.CoalAmount > 0)
            {
                int stolen = (int)Math.Min(CoalToSteal, gameplayContext.State.CoalAmount);
                gameplayContext.State.ConsumeCoal(stolen);
            }
            hasStolen = true;
            currentState = ThiefState.Fleeing;
        }
    }
    
    private void UpdateFleeing(float deltaTime)
    {
        switch (Slot.Side)
        {
            case EnemySlotSide.Top:
                Position = new Vector2(Position.X, Position.Y - FleeSpeed * deltaTime);
                if (Position.Y < -Size)
                {
                    ShouldRemove = true;
                }
                break;
            case EnemySlotSide.Bottom:
                Position = new Vector2(Position.X, Position.Y + FleeSpeed * deltaTime);
                if (Position.Y > gameplayContext.ScreenHeight + Size)
                {
                    ShouldRemove = true;
                }
                break;
        }
    }
}
