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

    public Vector2 GetAnchor(float distanceFromTrain)
    {
        GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
        Rectangle bounds = gameplayContext.Map.GetBounds();

        if (Side == EnemySlotSide.Top)
        {
            float topClearance = GamelabGame.Instance.GameplayConfig.TrainTileSize;
            return new Vector2(
                bounds.Left + bounds.Width * PositionRatio,
                bounds.Top - distanceFromTrain - topClearance
            );
        }

        return new Vector2(
            bounds.Left + bounds.Width * PositionRatio,
            bounds.Bottom + distanceFromTrain
        );
    }
}
