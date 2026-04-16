using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Hazards;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies;

public enum AnchorState
{
    ApproachingAnchorDeploySlot,
    Deploying,
    Retreating
}

public class AnchorEnemy : AbstractEnemy
{
    public override Color EnemyColor => currentState switch
    {
        AnchorState.ApproachingAnchorDeploySlot => Color.DarkKhaki,
        AnchorState.Deploying => Color.Goldenrod,
        AnchorState.Retreating => Color.SandyBrown,
        _ => Color.DarkKhaki
    };

    private float PreferredDistance => GamelabGame.Instance.GameplayConfig.AnchorPreferredDistance;
    private float DeployDurationSeconds => GamelabGame.Instance.GameplayConfig.AnchorDeployDurationSeconds;
    private float RetreatSpeed => GamelabGame.Instance.GameplayConfig.AnchorRetreatSpeed;
    private float CutDurationSeconds => GamelabGame.Instance.GameplayConfig.AnchorCutDurationSeconds;

    private AnchorState currentState = AnchorState.ApproachingAnchorDeploySlot;
    private float stateTimer;
    private AnchorCable pendingCable;

    public AnchorEnemy(GameplayContext gameplayContext, Vector2 spawnPosition, EnemyTrainSlot slot)
        : base(
            gameplayContext,
            new EnemyDefinition(EnemyType.Anchor),
            spawnPosition,
            slot,
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.AnchorMaxSpeed))
    {
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vector2 deployAnchor = Slot.GetAnchor(gameplayContext, Size + PreferredDistance);
        Vector2 approachAnchor = GetApproachAnchor(deployAnchor);

        switch (currentState)
        {
            case AnchorState.ApproachingAnchorDeploySlot:
                EnemyMovement.UpdateTowardPoint(approachAnchor, deltaTime);
                if (HasReached(approachAnchor, EnemyMovement.Profile.ArrivalRadius + 8f))
                {
                    currentState = AnchorState.Deploying;
                    stateTimer = 0f;
                }
                break;

            case AnchorState.Deploying:
                EnemyMovement.UpdateHoldPosition(deployAnchor, deltaTime);
                stateTimer += deltaTime;
                if (stateTimer >= DeployDurationSeconds && pendingCable == null)
                {
                    pendingCable = new AnchorCable(gameplayContext, Slot, Size + 10f, CutDurationSeconds);
                    currentState = AnchorState.Retreating;
                }
                break;

            case AnchorState.Retreating:
                Vector2 retreatDirection = Slot.Side == EnemySlotSide.Top ? -Vector2.UnitY : Vector2.UnitY;
                EnemyMovement.UpdateTowardDirection(retreatDirection, RetreatSpeed, deltaTime);
                if ((Slot.Side == EnemySlotSide.Top && IsOffScreenTop()) ||
                    (Slot.Side == EnemySlotSide.Bottom && IsOffScreenBottom()))
                {
                    ShouldRemove = true;
                }
                break;
        }
    }

    public override IEnemyHazard TryCreateHazard()
    {
        if (pendingCable == null)
        {
            return null;
        }

        AnchorCable cable = pendingCable;
        pendingCable = null;
        return cable;
    }

    private Vector2 GetApproachAnchor(Vector2 slotAnchor)
    {
        float horizontalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 1.2f;
        float verticalOffset = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.7f;

        return Slot.Side switch
        {
            EnemySlotSide.Top => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y - verticalOffset),
            EnemySlotSide.Bottom => new Vector2(slotAnchor.X + horizontalOffset, slotAnchor.Y + verticalOffset),
            _ => slotAnchor
        };
    }
}
