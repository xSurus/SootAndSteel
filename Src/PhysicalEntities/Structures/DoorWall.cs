using Gamelab.Assets;
using Gamelab.PhysicalEntities.Interfaces;
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
        Texture2D tex;
        tex = isOpen
            ? AssetManager.GetWallTexture("WallTileTopDoorOpen")
            : AssetManager.GetWallTexture("WallTileTopDoorClosed");

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float scale = tileSize / (float)AssetManager.GetWallTexture("WallTileTop").Width;
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
        Vector2 bottomCenter = Position + new Vector2(0, dimensionsPixels.Y / 2f);
        float depth = RenderUtility.CalculateDepth(bottomCenter.Y);

        spriteBatch.Draw(tex, bottomCenter, null, Color.White, 0f, origin, scale, SpriteEffects.None, depth);
    }
}