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

    public DoorWall(Vector2 dimensionsPixels, Vector2 positionPixels)
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

        spriteBatch.Draw(
            texture: AssetManager.BlankTexture,
            position: snappedPos,
            sourceRectangle: sourceRect,
            color: color,
            rotation: PhysicsBody.Rotation,
            origin: origin,
            scale: 1f,
            effects: SpriteEffects.None,
            layerDepth: 0f);

        // Open/closed indicator dot
        int dot = 8;
        var dotRect = new Rectangle(
            (int)(snappedPos.X + dimensionsPixels.X / 2f) - dot - 3,
            (int)(snappedPos.Y - dot / 2),
            dot, dot);
        spriteBatch.Draw(AssetManager.BlankTexture, dotRect, isOpen ? Color.LimeGreen : Color.Red);
    }
}
