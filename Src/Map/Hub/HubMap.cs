using System;
using Gamelab.Assets;
using Gamelab.Map.Train.State;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map.Hub;

public class HubMap : IDisposable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();

    public Rectangle BoundsPixels { get; }

    private Body leftWall;
    private Body rightWall;
    private Body topWall;
    private Body bottomWall;

    public HubMap(Rectangle boundsPixels, bool openBottom = false)
    {
        BoundsPixels = boundsPixels;
        CreateBoundaryWalls(openBottom);
    }

    private void CreateBoundaryWalls(bool openBottom)
    {
        float thicknessPixels = 40f;
        float thicknessMeters = thicknessPixels.ToMeters();

        float widthMeters = BoundsPixels.Width.ToMeters();
        float heightMeters = BoundsPixels.Height.ToMeters();

        Vector2 centerPixels = new Vector2(BoundsPixels.Center.X, BoundsPixels.Center.Y);
        Vector2 centerMeters = centerPixels.ToMeters();

        leftWall = gameplayContext.PhysicsWorld.CreateRectangle(
            thicknessMeters,
            heightMeters,
            1f,
            (centerMeters + new Vector2(-widthMeters / 2f - thicknessMeters / 2f, 0f)),
            0f,
            BodyType.Static
        );

        rightWall = gameplayContext.PhysicsWorld.CreateRectangle(
            thicknessMeters,
            heightMeters,
            1f,
            (centerMeters + new Vector2(widthMeters / 2f + thicknessMeters / 2f, 0f)),
            0f,
            BodyType.Static
        );

        topWall = gameplayContext.PhysicsWorld.CreateRectangle(
            widthMeters,
            thicknessMeters,
            1f,
            (centerMeters + new Vector2(0f, -heightMeters / 2f - thicknessMeters / 2f)),
            0f,
            BodyType.Static
        );

        if (!openBottom)
        {
            bottomWall = gameplayContext.PhysicsWorld.CreateRectangle(
                widthMeters,
                thicknessMeters,
                1f,
                (centerMeters + new Vector2(0f, heightMeters / 2f + thicknessMeters / 2f)),
                0f,
                BodyType.Static
            );
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(AssetManager.BlankTexture, BoundsPixels, new Color(30, 30, 40));
    }

    public void Dispose()
    {
        if (leftWall != null) gameplayContext.PhysicsWorld.Remove(leftWall);
        if (rightWall != null) gameplayContext.PhysicsWorld.Remove(rightWall);
        if (topWall != null) gameplayContext.PhysicsWorld.Remove(topWall);
        if (bottomWall != null) gameplayContext.PhysicsWorld.Remove(bottomWall);
        leftWall = null;
        rightWall = null;
        topWall = null;
        bottomWall = null;
    }
}

