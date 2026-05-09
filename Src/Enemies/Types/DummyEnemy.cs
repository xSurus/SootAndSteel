using Gamelab.Enemies.Core;
using Gamelab.Enemies.Movement;
using Gamelab.Enemies.Slots;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Gamelab.Enemies.Types;

public class DummyEnemy : AbstractEnemy
{
    private float enemySize = GamelabGame.Instance.GameplayConfig.EnemySize;
    
    public DummyEnemy(Vector2 spawnPosition) : 
        base(spawnPosition, 
            new EnemyTrainSlot(0, 0), 
            EnemyMovementProfile.CreateDefault(GamelabGame.Instance.GameplayConfig.RifleMaxSpeed))
    {
        Position = spawnPosition;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || ShouldRemove)
        {
            return;
        }
        
        float renderDepth = RenderUtility.CalculateDepth(Position.Y + enemySize/2);

        spriteBatch.FillRectangle(
            new Vector2(Position.X - enemySize/2, Position.Y - enemySize/2),
            new SizeF(enemySize, enemySize),
            Color.Red,
            renderDepth
        );
        
        spriteBatch.DrawRectangle(
            new Vector2(Position.X - enemySize/2, Position.Y - enemySize/2),
            new SizeF(enemySize, enemySize),
            Color.DarkRed,
            enemySize / 12,
            renderDepth
        );
    }
}