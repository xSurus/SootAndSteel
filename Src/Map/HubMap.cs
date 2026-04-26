using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Data;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Services.Shop;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Map;

public class HubMap : IDisposable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly int worldWidth;
    private readonly int worldHeight;
    private readonly List<Body> worldBoundaryBodies = [];
    private Rectangle shoppingArea;
    private Texture2D baseBackgroundTexture;

    public HubMap(int worldWidth, int worldHeight)
    {
        int hubPad = 120;
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;

        shoppingArea = new Rectangle(
            hubPad, hubPad,
            worldWidth - hubPad * 2,
            worldHeight / 2 - hubPad - 160);
        CreateWorldBoundaryWalls();
        baseBackgroundTexture = new Texture2D(GamelabGame.Instance.GraphicsDevice, 1, 1);
        Color baseColor = new Color(208, 232, 242);
        baseBackgroundTexture.SetData(new[] { baseColor });
    }

    public void RestockHubDragOffers(Random hubRandom, int offerCount)
    {
        List<CatalogItem> selectedOffers = GamelabGame.Instance.Services.GetService<IShopService>()
            .GenerateCatalog()
            .OrderBy(x => hubRandom.Next())
            .Take(offerCount)
            .ToList();

        Vector2 shopCenter = new Vector2(shoppingArea.Center.X, shoppingArea.Bottom - 350);
        float spacingX = 100f;

        for (int i = 0; i < selectedOffers.Count; i++)
        {
            var offer = selectedOffers[i];
            float totalRowWidth = (offerCount - 1) * spacingX;
            float startX = shopCenter.X - (totalRowWidth / 2f);
            Vector2 spawnPos = new Vector2(startX + (i * spacingX), shopCenter.Y);

            gameplayContext.Map.MapObjects.Add(new BuyableStationWrapper(offer.ItemId, spawnPos));
        }
    }


    private void CreateWorldBoundaryWalls()
    {
        float boundaryThickness = 1;
        float boundaryThicknessMeters = boundaryThickness.ToMeters();

        AddWorldWallSegment(worldWidth.ToMeters(), boundaryThicknessMeters,
            new Vector2(worldWidth / 2f, -boundaryThickness / 2f));
        AddWorldWallSegment(worldWidth.ToMeters(), boundaryThicknessMeters,
            new Vector2(worldWidth / 2f, worldHeight + boundaryThickness / 2f));
        AddWorldWallSegment(boundaryThicknessMeters, worldHeight.ToMeters(),
            new Vector2(-boundaryThickness / 2f, worldHeight / 2f));
        AddWorldWallSegment(boundaryThicknessMeters, worldHeight.ToMeters(),
            new Vector2(worldWidth + boundaryThickness / 2f, worldHeight / 2f));
    }

    private void AddWorldWallSegment(float widthMeters, float heightMeters, Vector2 centerPixels)
    {
        Body b = gameplayContext.PhysicsWorld.CreateRectangle(widthMeters, heightMeters, 1f, centerPixels.ToMeters(),
            0f, BodyType.Static);
        worldBoundaryBodies.Add(b);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawBackground(spriteBatch);
        DrawRails(spriteBatch);
        DrawNpcs(spriteBatch);
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture: baseBackgroundTexture,
            destinationRectangle: new Rectangle(0, 0, worldWidth, worldHeight),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: RenderUtility.BackgroundLayer - RenderUtility.Eps
        );

        float scale = worldWidth * 1.0f / AssetManager.HubTexture.Width;
        spriteBatch.Draw(AssetManager.HubTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale,
            SpriteEffects.None, RenderUtility.BackgroundLayer);
    }

    private void DrawRails(SpriteBatch spriteBatch)
    {
        Texture2D railTex = AssetManager.TrainTrackTexture[0];
        int railTileWidth = railTex.Width;
        float centerY = ((worldHeight / 4f) * 3) - AssetManager.TrainTrackTexture[0].Height + 40;
        int railCols = (worldWidth / railTileWidth) + 3;
        for (int col = 0; col < railCols; col++)
        {
            Vector2 railDrawPos = new Vector2(col * railTileWidth, centerY);
            spriteBatch.Draw(
                texture: railTex,
                position: railDrawPos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 1,
                effects: SpriteEffects.None,
                layerDepth: (RenderUtility.BackgroundLayer + RenderUtility.FloorLayer) / 2f);
        }
    }

    private void DrawNpcs(SpriteBatch spriteBatch)
    {
        Vector2 vendorPosition = new Vector2(1160, 580);
        Texture2D vendorTex = AssetManager.GetNPCTexture("Vendor");
        Vector2 bottomCenterOrigin = new Vector2(vendorTex.Width / 2f, vendorTex.Height);
        float vendorScale = 0.3f;
        float vendorDepth = RenderUtility.CalculateDepth(vendorPosition.Y);
        spriteBatch.Draw(
            texture: vendorTex,
            position: vendorPosition,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: bottomCenterOrigin,
            scale: vendorScale,
            effects: SpriteEffects.None,
            layerDepth: vendorDepth
        );

        Vector2 town1Position = new Vector2(800, 200);
        Texture2D town1Tex = AssetManager.GetNPCTexture("Town1");
        Vector2 town1Origin = new Vector2(town1Tex.Width / 2f, town1Tex.Height);
        float town1Scale = 0.3f;
        float town1Depth = RenderUtility.CalculateDepth(town1Position.Y);
        spriteBatch.Draw(
            texture: town1Tex,
            position: town1Position,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: town1Origin,
            scale: town1Scale,
            effects: SpriteEffects.None,
            layerDepth: town1Depth
        );
    }

    public void Dispose()
    {
        foreach (Body b in worldBoundaryBodies)
        {
            if (b.World != null) b.World.Remove(b);
        }

        worldBoundaryBodies.Clear();
    }
}