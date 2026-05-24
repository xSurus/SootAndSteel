using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Data;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Structures;
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
    private readonly List<Stake> fenceStakes = [];
    private readonly List<HubTree> hubTrees = [];
    private Rectangle shoppingArea;
    private Rectangle villageRect;

    private const float FenceStakeSpacing = 95f;
    private const float SignGapWidth = 280f;
    private const float SignVisualWidth = 360f;
    private const float HubTreeBaseScale = 0.55f;
    private const string PineDecorationKey = "Snow_Covered_Pine";

    private readonly record struct HubTree(Vector2 Feet, float Scale);

    private const float TrackScale = 1.4f;
    private int TileWidth => (int)(AssetManager.TrainTrackTexture[0].Width * TrackScale);

    public HubMap(int worldWidth, int worldHeight)
    {
        int hubPad = 120;
        this.worldWidth = worldWidth;
        this.worldHeight = worldHeight;

        shoppingArea = new Rectangle(
            hubPad, hubPad,
            worldWidth - hubPad * 2,
            worldHeight / 2 - hubPad - 160);


        float hubBgScale = worldWidth * 1.0f / AssetManager.HubTexture.Width;
        int villageBottom = (int)MathF.Min(AssetManager.HubTexture.Height * hubBgScale, worldHeight / 2f);
        villageRect = new Rectangle(
            hubPad,
            hubPad,
            worldWidth - hubPad * 2,
            Math.Max(shoppingArea.Height, villageBottom - hubPad));

        CreateWorldBoundaryWalls();
        BuildVillageFence();
        BuildHubTreeLayout();
        CreateHouseBoundaries();
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

    private void CreateHouseBoundaries()
    {
        Texture2D house1Tex = AssetManager.GetHubDecorationTexture("House1");
        Texture2D house2Tex = AssetManager.GetHubDecorationTexture("House2");
        float houseScale = 0.3f;

        Vector2 house1Pos = new Vector2(worldWidth / 4f, 150);
        float house1WidthPixels = house1Tex.Width * houseScale;
        float house1HeightPixels = house1Tex.Height * houseScale;
        Vector2 house1CenterMeters = house1Pos.ToMeters();
        float house1WidthMeters = house1WidthPixels.ToMeters();
        float house1HeightMeters = (house1HeightPixels - 120).ToMeters();

        Body house1Body = gameplayContext.PhysicsWorld.CreateRectangle(
            house1WidthMeters,
            house1HeightMeters,
            1f,
            house1CenterMeters,
            0f,
            BodyType.Static
        );
        worldBoundaryBodies.Add(house1Body);


        Vector2 house2Pos = new Vector2(worldWidth / 4f * 3f, 150);
        float house2WidthPixels = house2Tex.Width * houseScale;
        float house2HeightPixels = house2Tex.Height * houseScale;
        Vector2 house2CenterMeters = house2Pos.ToMeters();
        float house2WidthMeters = house2WidthPixels.ToMeters();
        float house2HeightMeters = (house2HeightPixels - 120).ToMeters();

        Body house2Body = gameplayContext.PhysicsWorld.CreateRectangle(
            house2WidthMeters,
            house2HeightMeters,
            1f,
            house2CenterMeters,
            0f,
            BodyType.Static
        );
        worldBoundaryBodies.Add(house2Body);
    }

    private void AddWorldWallSegment(float widthMeters, float heightMeters, Vector2 centerPixels)
    {
        Body b = gameplayContext.PhysicsWorld.CreateRectangle(widthMeters, heightMeters, 1f, centerPixels.ToMeters(),
            0f, BodyType.Static);
        worldBoundaryBodies.Add(b);
    }

    private void BuildVillageFence()
    {
        // Bottom edge with a sign-shaped gap centered horizontally
        AddFenceLineHorizontal(villageRect.Left, villageRect.Right, villageRect.Bottom, leaveCenterGap: true);

        // Left and right edges
        AddFenceLineVertical(villageRect.Left, villageRect.Top, villageRect.Bottom);
        AddFenceLineVertical(villageRect.Right, villageRect.Top, villageRect.Bottom);
    }

    private void AddFenceLineHorizontal(float xStart, float xEnd, float y, bool leaveCenterGap)
    {
        float gapHalf = leaveCenterGap ? SignGapWidth / 2f : 0f;
        float centerX = worldWidth / 2f;

        for (float x = xStart; x <= xEnd; x += FenceStakeSpacing)
        {
            if (leaveCenterGap && MathF.Abs(x - centerX) < gapHalf) continue;
            AddStake(new Vector2(x, y));
        }
    }

    private void AddFenceLineVertical(float x, float yStart, float yEnd)
    {
        for (float y = yStart; y <= yEnd - FenceStakeSpacing; y += FenceStakeSpacing)
        {
            AddStake(new Vector2(x, y));
        }
    }

    private void AddStake(Vector2 feetPos)
    {
        Stake stake = new Stake(feetPos);
        fenceStakes.Add(stake);
    }

    private void BuildHubTreeLayout()
    {
        float leftMarginX = villageRect.Left * 0.5f;
        float rightMarginX = (villageRect.Right + worldWidth) / 2f;
        float topY = villageRect.Top + villageRect.Height * 0.15f;
        float midY = villageRect.Top + villageRect.Height * 0.5f;
        float lowerY = villageRect.Top + villageRect.Height * 0.85f;

        Vector2[] handPlaced =
        {
            new Vector2(leftMarginX, topY),
            new Vector2(leftMarginX - 40f, midY),
            new Vector2(leftMarginX + 30f, lowerY),
            new Vector2(rightMarginX, topY),
            new Vector2(rightMarginX + 40f, midY),
            new Vector2(rightMarginX - 20f, lowerY),
        };

        float[] scales = { 1.0f, 0.9f, 1.1f, 1.0f, 1.05f, 0.95f };

        for (int i = 0; i < handPlaced.Length; i++)
        {
            hubTrees.Add(new HubTree(handPlaced[i], HubTreeBaseScale * scales[i]));
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawSnowBackdrop(spriteBatch);
        DrawHubTrees(spriteBatch);
        DrawFence(spriteBatch);
        DrawRails(spriteBatch);
        DrawHouses(spriteBatch);
        DrawNpcs(spriteBatch);
    }

    /// <summary>
    /// TODO: replace with a snow texture
    /// </summary>
    private void DrawSnowBackdrop(SpriteBatch spriteBatch)
    {
        float minZoom = GamelabGame.Instance.GameplayConfig.CameraMinZoom;
        int pad = (int)MathF.Ceiling(MathF.Max(worldWidth, worldHeight) / minZoom);
        Rectangle area = new Rectangle(-pad, -pad, worldWidth + pad * 2, worldHeight + pad * 2);
        spriteBatch.Draw(AssetManager.BlankTexture, area, RenderUtility.SnowBackgroundColor);
    }

    private void DrawFence(SpriteBatch spriteBatch)
    {
        foreach (Stake stake in fenceStakes)
        {
            stake.Draw(spriteBatch);
        }
    }

    private void DrawHubTrees(SpriteBatch spriteBatch)
    {
        Texture2D treeTex = AssetManager.GetDecorationTexture(PineDecorationKey);
        Vector2 origin = new Vector2(treeTex.Width / 2f, treeTex.Height);

        foreach (HubTree t in hubTrees)
        {
            float depth = RenderUtility.CalculateDepth(t.Feet.Y);
            spriteBatch.Draw(treeTex, t.Feet, null, Color.White, 0f, origin, t.Scale,
                SpriteEffects.None, depth);
        }
    }

    private void DrawRails(SpriteBatch spriteBatch)
    {
        Texture2D railTex = AssetManager.TrainTrackTexture[0];
        float centerY = ((worldHeight / 4f) * 3) - AssetManager.TrainTrackTexture[0].Height + 40;
        float minZoom = GamelabGame.Instance.GameplayConfig.CameraMinZoom;
        int pad = (int)MathF.Ceiling(MathF.Max(worldWidth, worldHeight) / minZoom);
        int startCol = (int)MathF.Floor(-pad / (float)TileWidth);
        int railCols = (worldWidth + pad * 2) / TileWidth + 3;
        for (int col = startCol; col < startCol + railCols; col++)
        {
            Vector2 railDrawPos = new Vector2(col * TileWidth, centerY - 30);
            spriteBatch.Draw(
                texture: railTex,
                position: railDrawPos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: TrackScale,
                effects: SpriteEffects.None,
                layerDepth: RenderUtility.BackgroundLayer);
        }
    }

    private void DrawHouses(SpriteBatch spriteBatch)
    {
        Texture2D house1Tex = AssetManager.GetHubDecorationTexture("House1");
        Texture2D house2Tex = AssetManager.GetHubDecorationTexture("House2");
        Vector2 house1Pos = new Vector2(worldWidth / 4f, 150);
        Vector2 house2Pos = new Vector2(worldWidth / 4f * 3f, 150);

        float houseScale = 0.3f;
        Vector2 origin = new Vector2(house1Tex.Width / 2f, house1Tex.Height / 2f);

        spriteBatch.Draw(
            texture: house1Tex,
            position: house1Pos,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: houseScale,
            effects: SpriteEffects.None,
            layerDepth: RenderUtility.FloorLayer
        );

        spriteBatch.Draw(
            texture: house2Tex,
            position: house2Pos,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: origin,
            scale: houseScale,
            effects: SpriteEffects.None,
            layerDepth: RenderUtility.FloorLayer
        );
    }

    private void DrawNpcs(SpriteBatch spriteBatch)
    {
        Vector2 vendorPosition = new Vector2(1260, 700);
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

        Vector2 town1Position = new Vector2(500, 580);
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

        foreach (Stake stake in fenceStakes)
        {
            stake.RemovePhysicsBody();
        }

        fenceStakes.Clear();
    }
}