using System;
using Gamelab.Assets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class CannonWagon : AbstractPhysicalEntity, IPickable, IUpdatable
{
    public const int WidthTiles = 5;
    public const int HeightTiles = 8;

    private const float BarrelDistance = 80f;
    private const float BarrelScaleFactor = 0.65f;

    public override bool CanHighlight => false;

    public CannonSlot TopSlot => topSlot;
    public CannonSlot BottomSlot => bottomSlot;
    public BulletRack TopRack => topRack;
    public BulletRack BottomRack => bottomRack;

    private readonly CannonSlot topSlot;
    private readonly CannonSlot bottomSlot;
    private readonly BulletRack topRack;
    private readonly BulletRack bottomRack;
    private readonly float widthPixels;
    private readonly float heightPixels;
    private readonly float scale;
    private readonly Texture2D wagonTexture;

    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            var oldPos = base.Position;
            base.Position = value;
            var delta = value - oldPos;
            if (delta == Vector2.Zero) return;

            topSlot.Position += delta;
            bottomSlot.Position += delta;
            topRack.Position += delta;
            bottomRack.Position += delta;
        }
    }

    public CannonWagon(Vector2 centerPixels)
    {
        var config = GamelabGame.Instance.GameplayConfig;
        int tileSize = config.TrainTileSize;
        int trainHeight = config.TrainHeight;

        widthPixels = WidthTiles * tileSize;
        heightPixels = HeightTiles * tileSize;

        wagonTexture = AssetManager.GetStructureTexture("CannonWagon");
        scale = widthPixels / wagonTexture.Width;

        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(
            widthPixels.ToMeters(), heightPixels.ToMeters(), 1f, centerPixels.ToMeters());
        foreach (var fixture in PhysicsBody.FixtureList)
            fixture.IsSensor = true;

        // Left wall
        gameplayContext.PhysicsWorld.CreateRectangle(
            ((float)tileSize).ToMeters(),
            heightPixels.ToMeters(),
            1f,
            new Vector2(centerPixels.X - widthPixels / 2f + tileSize, centerPixels.Y).ToMeters());

        // top and bottom wall
        gameplayContext.PhysicsWorld.CreateRectangle(
            widthPixels.ToMeters(),
            (tileSize / 2f).ToMeters(),
            1f,
            new Vector2(centerPixels.X, centerPixels.Y - (trainHeight * tileSize) / 2f).ToMeters());
        gameplayContext.PhysicsWorld.CreateRectangle(
            widthPixels.ToMeters(),
            (tileSize / 2f).ToMeters(),
            1f,
            new Vector2(centerPixels.X, centerPixels.Y + (trainHeight * tileSize) / 2f).ToMeters());

        float visualOffset = tileSize / 2f;
        float topSeatOffsetY = -heightPixels * 0.3f; // less negative = lower
        float bottomSeatOffsetY = heightPixels * 0.3f; // less positive = higher

        var topSeatPos = centerPixels + new Vector2(visualOffset, topSeatOffsetY);
        topSlot = new CannonSlot(topSeatPos, arcMin: -MathF.PI, arcMax: 0f, defaultAngle: -MathF.PI / 2f,
            textureName: "CannonSeatingTop", exitOffsetPixels: new Vector2(0, tileSize),
            seatPosition: SeatPosition.Top);
        topRack = new BulletRack(centerPixels +
                                 new Vector2(visualOffset + 1.1f * tileSize, topSeatOffsetY + 1.5f * tileSize));
        topSlot.SetPairedRack(topRack);

        var bottomSeatPos = centerPixels + new Vector2(visualOffset, bottomSeatOffsetY);
        bottomSlot = new CannonSlot(bottomSeatPos, arcMin: 0f, arcMax: MathF.PI, defaultAngle: MathF.PI / 2f,
            exitOffsetPixels: new Vector2(0, -tileSize), seatPosition: SeatPosition.Bottom);
        bottomRack = new BulletRack(centerPixels +
                                    new Vector2(visualOffset + 1.1f * tileSize, bottomSeatOffsetY - 0.6f * tileSize));
        bottomSlot.SetPairedRack(bottomRack);

        float cannonTexHeight = AssetManager.GetStructureTexture("CannonBottom").Height;
        float barrelLength = cannonTexHeight * scale * BarrelScaleFactor;
        topSlot.SetBarrelParams(new Vector2(0f, -BarrelDistance), barrelLength);
        bottomSlot.SetBarrelParams(new Vector2(0f, BarrelDistance), barrelLength);
    }

    public void Update(float dt)
    {
        topSlot.Update(dt);
        bottomSlot.Update(dt);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float drawRight = Position.X + widthPixels / 2f + tileSize / 2f;
        Vector2 rightBottom = new Vector2(drawRight, Position.Y + heightPixels / 2f);
        float scaleX = widthPixels / wagonTexture.Width;
        float scaleY = heightPixels / wagonTexture.Height;
        spriteBatch.Draw(wagonTexture, rightBottom, null, Color.White, 0f,
            new Vector2(wagonTexture.Width, wagonTexture.Height), new Vector2(scaleX, scaleY),
            SpriteEffects.None, RenderUtility.FloorLayer + 3 * RenderUtility.Eps);

        Vector2 topSeatPos = topSlot.PhysicsBody.Position.ToPixels();
        Vector2 bottomSeatPos = bottomSlot.PhysicsBody.Position.ToPixels();
        float topDepth = RenderUtility.CalculateDepth(topSeatPos.Y);
        float bottomDepth = RenderUtility.CalculateDepth(bottomSeatPos.Y);

        Vector2 topBarrelPos = topSeatPos + new Vector2(0, -BarrelDistance);
        Vector2 bottomBarrelPos = bottomSeatPos + new Vector2(0, BarrelDistance);

        spriteBatch.Draw(AssetManager.GetStructureTexture("CannonWagonTop"), rightBottom, null, Color.White, 0f,
            new Vector2(wagonTexture.Width, wagonTexture.Height), new Vector2(scaleX, scaleY),
            SpriteEffects.None, (topDepth - 0.005f));

        topSlot.Draw(spriteBatch);
        bottomSlot.Draw(spriteBatch);

        DrawRotatingCannon(spriteBatch, "CannonTop", bottomSlot, bottomBarrelPos, bottomDepth + 0.01f, MathF.PI / 2f,
            bottom: true);
        DrawRotatingCannon(spriteBatch, "CannonBottom", topSlot, topBarrelPos, topDepth - 0.01f, -MathF.PI / 2f,
            bottom: false);

        DrawFrontWall(spriteBatch, bottomBarrelPos, bottomDepth + 0.02f, bottomSlot.IsHighlighted);
    }

    private void DrawRotatingCannon(SpriteBatch spriteBatch, string texName, CannonSlot slot, Vector2 barrelPos,
        float depth, float angleOffset, bool bottom)
    {
        Texture2D tex = AssetManager.GetStructureTexture(texName);
        Vector2 origin = new Vector2(tex.Width / 2f, bottom ? tex.Height : 0f);
        spriteBatch.DrawWithHighlight(tex, barrelPos, null, Color.White, slot.AimAngle + angleOffset, origin,
            scale * BarrelScaleFactor, SpriteEffects.None, depth, slot.IsHighlighted);
    }

    private void DrawFrontWall(SpriteBatch spriteBatch, Vector2 barrelPos, float depth, bool isHighlighted)
    {
        Texture2D tex = AssetManager.GetStructureTexture("CannonWagonFrontWall");
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
        spriteBatch.DrawWithHighlight(tex, barrelPos + new Vector2(0, -48f), null, Color.White, 0f, origin, scale,
            SpriteEffects.None, depth, isHighlighted);
    }

    public Rectangle GetBounds()
    {
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float drawLeft = Position.X - widthPixels / 2f + tileSize / 2f;
        Vector2 topLeft = new Vector2(drawLeft, Position.Y - heightPixels / 2f);
        return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)widthPixels, (int)heightPixels);
    }
}