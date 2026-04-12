using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public enum MounterState
{
    Approaching,
    Mounting,
    MountedStealing,
    Escaping
}

public class MounterEnemy : AbstractEnemy
{
    public override Color EnemyColor => currentState switch
    {
        MounterState.Approaching => Color.Purple,
        MounterState.Mounting => Color.Orange,
        MounterState.MountedStealing => Color.Yellow,
        MounterState.Escaping => Color.Green,
        _ => Color.Purple
    };

    private float ApproachSpeed => GamelabGame.Instance.GameplayConfig.MounterApproachSpeed;
    private float FleeSpeed => GamelabGame.Instance.GameplayConfig.MounterFleeSpeed;
    private float StealDuration => GamelabGame.Instance.GameplayConfig.MounterStealDuration;
    private int CoalToSteal => GamelabGame.Instance.GameplayConfig.MounterCoalAmount;
    private float MountDuration => GamelabGame.Instance.GameplayConfig.MounterMountDuration;

    private MounterState currentState = MounterState.Approaching;
    private float stateTimer;
    private bool hasStolen;

    public MounterEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, EnemyTrainSlot slot)
        : base(
            gameplayContext,
            new EnemyDefinition(EnemyType.Mounter),
            spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(System.Math.Max(
                GamelabGame.Instance.GameplayConfig.MounterApproachSpeed,
                GamelabGame.Instance.GameplayConfig.MounterFleeSpeed)))
    {
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        switch (currentState)
        {
            case MounterState.Approaching:
                UpdateApproaching(deltaTime);
                break;
            case MounterState.Mounting:
                UpdateMounting(deltaTime);
                break;
            case MounterState.MountedStealing:
                UpdateMountedStealing(deltaTime);
                break;
            case MounterState.Escaping:
                UpdateEscaping(deltaTime);
                break;
        }
    }

    private void UpdateApproaching(float deltaTime)
    {
        Vector2 targetPosition = GetTargetPosition();
        EnemyMovement.UpdateTowardPoint(targetPosition, deltaTime);

        if (HasReached(targetPosition, EnemyMovement.Profile.ArrivalRadius + 8f))
        {
            currentState = MounterState.Mounting;
            stateTimer = 0f;
        }
    }

    private void UpdateMounting(float deltaTime)
    {
        stateTimer += deltaTime;
        EnemyMovement.UpdateHoldPosition(GetTargetPosition(), deltaTime, includeTrainDrift: false);

        if (stateTimer >= MountDuration)
        {
            currentState = MounterState.MountedStealing;
            stateTimer = 0f;
        }
    }

    private void UpdateMountedStealing(float deltaTime)
    {
        stateTimer += deltaTime;
        EnemyMovement.UpdateHoldPosition(GetTargetPosition(), deltaTime, includeTrainDrift: false);

        if (stateTimer >= StealDuration && !hasStolen)
        {
            if (gameplayContext.State.CoalAmount > 0)
            {
                int stolen = System.Math.Min(CoalToSteal, gameplayContext.State.CoalAmount);
                gameplayContext.State.ConsumeCoal(stolen);
            }

            hasStolen = true;
            currentState = MounterState.Escaping;
        }
    }

    private void UpdateEscaping(float deltaTime)
    {
        Vector2 direction = Slot.Side == EnemySlotSide.Top
            ? -Vector2.UnitY
            : Vector2.UnitY;
        EnemyMovement.UpdateTowardDirection(direction, FleeSpeed, deltaTime);

        if ((Slot.Side == EnemySlotSide.Top && IsOffScreenTop()) ||
            (Slot.Side == EnemySlotSide.Bottom && IsOffScreenBottom()))
        {
            ShouldRemove = true;
        }
    }

    private Vector2 GetTargetPosition()
    {
        float margin = Size / 2f + 10f;
        return Slot.GetAnchor(gameplayContext, margin);
    }
}
