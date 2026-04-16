using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Structures;

/// <summary>
/// A boundary wall segment that players can toggle open or closed via interact.
/// Placed by <see cref="Gamelab.Map.Train.TrainMap.InitializeBoundaryWalls"/> at a specified column.
/// </summary>
public class DoorWall : AbstractPhysicalEntity, IInteractable
{
    private static readonly Color ClosedColor = new Color(60, 100, 130);
    private static readonly Color OpenColor = new Color(50, 150, 60);

    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly Vector2 dimensionsPixels;
    private bool isOpen = false;
    private bool isTop;

    public DoorWall(Vector2 dimensionsPixels, Vector2 positionPixels, bool isTop)
    {
        this.dimensionsPixels = dimensionsPixels;
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(
            dimensionsPixels.X.ToMeters(),
            dimensionsPixels.Y.ToMeters(),
            1f,
            positionPixels.ToMeters(),
            0f,
            BodyType.Static);
        PhysicsBody.Tag = this;
        this.isTop = isTop;
    }

    public void OnInteract(Player interactingPlayer)
    {
        isOpen = !isOpen;
        foreach (var fixture in PhysicsBody.FixtureList)
            fixture.IsSensor = isOpen;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {   
        
        Color color = isOpen ? OpenColor : ClosedColor;
        Vector2 origin = new Vector2(dimensionsPixels.X / 2f, dimensionsPixels.Y / 2f);
        Rectangle sourceRect = new Rectangle(0, 0, (int)dimensionsPixels.X, (int)dimensionsPixels.Y);
        Vector2 snappedPos = new Vector2(MathF.Round(Position.X), MathF.Round(Position.Y));

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = AssetManager.GetWallTexture("WallTileTop").Width;
        float scale = tileSize / originalSize;

        if (isTop){
            Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.75f);
            Texture2D tex = isOpen ? AssetManager.GetWallTexture("WallTileTopDoorOpen") : AssetManager.GetWallTexture("WallTileTopDoorClosed");
            spriteBatch.Draw(tex, drawingPos, null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        } else {
            Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 2f);
            Texture2D tex = isOpen ? AssetManager.GetWallTexture("WallTileBottomDoorOpen") : AssetManager.GetWallTexture("WallTileBottomDoorClosed");
            spriteBatch.Draw(tex, drawingPos, null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        
        // Open/closed indicator dot
        int dot = 8;
        var dotRect = new Rectangle(
            (int)(snappedPos.X + dimensionsPixels.X / 2f) - dot - 3,
            (int)(snappedPos.Y - dot / 2),
            dot, dot);
        spriteBatch.Draw(AssetManager.BlankTexture, dotRect, isOpen ? Color.LimeGreen : Color.Red);
    }
}
