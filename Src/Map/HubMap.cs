using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Data;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Services.IShopService;
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
        float scale = worldWidth * 1.0f / AssetManager.HubTexture.Width;
        spriteBatch.Draw(AssetManager.HubTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale,
            SpriteEffects.None, 0f);
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