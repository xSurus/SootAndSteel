using Gamelab.Map.Train.State;
using Microsoft.Xna.Framework;

namespace Gamelab.Enemies.Slots;

public enum EnemySlotSide
{
    Top,
    Bottom
}

public readonly struct EnemyTrainSlot(EnemySlotSide side, float positionRatio)
{
    public EnemySlotSide Side { get; } = side;
    public float PositionRatio { get; } = positionRatio;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public Vector2 GetAnchor(float distanceFromTrain)
    {
        return Side switch
        {
            EnemySlotSide.Top => new Vector2(
                gameplayContext.Map.GetBounds().Left + gameplayContext.Map.GetBounds().Width * PositionRatio,
                gameplayContext.Map.GetBounds().Top - distanceFromTrain
            ),
            EnemySlotSide.Bottom => new Vector2(
                gameplayContext.Map.GetBounds().Left + gameplayContext.Map.GetBounds().Width * PositionRatio,
                gameplayContext.Map.GetBounds().Bottom + distanceFromTrain
            ),
            _ => new Vector2(gameplayContext.Map.GetBounds().Right + distanceFromTrain,
                gameplayContext.Map.GetBounds().Center.Y)
        };
    }
}