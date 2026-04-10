using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Hub;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.Players;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.PhysicalEntities.Triggers;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.Screens;

/// <param name="RelX">Offset from hub plaza center X.</param>
record struct HubShopOfferTemplate(string KindId, string DisplayName, int Cost, Color Color, float RelX, float SpawnY);

public class HubScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private enum HubWorldPhase
    {
        Hub,
        ScrollingToPrep,
        Prep,
        ScrollingToHub,
    }

    private List<Player> players;
    private GameplayContext gameplayContext;
    private HubMap hubMap;
    private TrainMap prepTrainMap;
    private Desktop desktop;
    private ExitZone departZone;
    private Rectangle prepEntryMarker;
    private Rectangle departMarker;
    private Rectangle hubPlazaBounds;

    private readonly List<Body> worldBoundaryBodies = [];

    private float accumulator;
    private ParticleEmitter hubSnowEmitter;
    private WhiteFilterTransition departWhiteFilter;
    private bool isTransitioningToNextLevel;

    private Vector2 cameraPosition;
    private int worldWidth;
    private int worldHeight;
    private float prepViewCameraY;
    private HubWorldPhase phase = HubWorldPhase.Hub;

    private readonly List<HubShopOffer> hubDragOffers = [];
    private readonly List<(AbstractStation station, int price)> unpaidShopPlacements = [];

    /// <summary>
    /// After arriving on the hub view, block prep-entry for a moment so overlap with the hub plaza does not
    /// instantly scroll back down (see hub return trigger). No physics "re-arm" — avoids getting stuck on top.
    /// </summary>
    private float prepEntryUnlockTimer;
    /// <summary>Last frame (Hub): all players were inside the prep-entry strip — used for rising-edge scroll to prep.</summary>
    private bool prepEntryAllInsidePrev;
    /// <summary>Last frame (Prep): all players were inside the hub plaza — used for rising-edge scroll to hub.</summary>
    private bool hubPlazaAllInsidePrev;
    private Label creditsLabel;
    private Label departBlockedLabel;
    private readonly ViewportTooltipOverlay hubShopTooltip = new();
    private SpriteFontBase hubTooltipFont;

    public override void LoadContent()
    {
        base.LoadContent();

        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);
        gameplayContext.Map = null;
        gameplayContext.State.IsCoalOvenBurning = false;

        worldWidth = virtualScreenSize.X;
        worldHeight = virtualScreenSize.Y * 2;
        prepViewCameraY = virtualScreenSize.Y - 200;
        cameraPosition = Vector2.Zero;

        CreateWorldBoundaryWalls();

        int hubPad = 120;
        hubPlazaBounds = new Rectangle(
            hubPad,
            hubPad,
            worldWidth - hubPad * 2,
            virtualScreenSize.Y - hubPad - 160);
        hubMap = new HubMap(hubPlazaBounds, openBottom: true);

        int trainW = Game.GameplayConfig.TrainWidth * Game.GameplayConfig.TrainTileSize;
        int tw = Game.GameplayConfig.TrainWidth;
        int gapMid = tw / 2;
        prepTrainMap = new TrainMap(
            new Vector2((worldWidth - trainW) / 2f, prepViewCameraY + 360f),
            spawnBreakableBottomEdge: true,
            bottomWallOmitStartTileX: Math.Max(0, gapMid - 1),
            bottomWallOmitEndTileXExclusive: Math.Min(tw, gapMid + 2));

        List<PrepStationEntry> hubSeed = Game.TrainLayoutSeedForHub;
        Game.TrainLayoutSeedForHub = null;
        if (hubSeed != null && hubSeed.Count > 0)
            PrepTrainLayout.ApplyLayout(prepTrainMap, hubSeed);
        else
            prepTrainMap.AddDefaultStationLoadout();

        prepEntryMarker = new Rectangle(
            worldWidth / 2 - 210,
            virtualScreenSize.Y - 150,
            420, 100);

        RestockHubDragOffers();

        departMarker = new Rectangle(
            worldWidth / 2 - 130,
            worldHeight - 300,
            260, 100);
        departZone = new ExitZone(gameplayContext.PhysicsWorld, departMarker);

        players = [];
        Vector2 spawn = new Vector2(hubPlazaBounds.Center.X, hubPlazaBounds.Y + 140f);
        var configs = Game.playerManager.Configs;
        int n = configs.Count;
        for (int i = 0; i < n; i++)
        {
            float ox = n <= 1 ? 0f : (i - (n - 1) * 0.5f) * 88f;
            players.Add(new Player(spawn + new Vector2(ox, 0f), configs[i]));
        }

        prepEntryAllInsidePrev = players.Count > 0 && AllPlayersInRectangle(prepEntryMarker);
        hubPlazaAllInsidePrev = players.Count > 0 && AllPlayersInRectangle(hubPlazaBounds);

        hubSnowEmitter = ParticleFactory.CreateSnowstorm();
        Services.GetService<IVfxService>().AddContinuous(hubSnowEmitter);

        creditsLabel = new Label
        {
            Text = $"Credits: {Game.Credits}",
            Font = Game.fontSystem.GetFont(40),
            TextColor = new Color(230, 200, 120),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 14, 28, 0)
        };

        hubTooltipFont = Game.fontSystem.GetFont(28);

        departBlockedLabel = new Label
        {
            Text = "",
            Font = Game.fontSystem.GetFont(28),
            TextColor = new Color(255, 120, 120),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(80, 0, 80, 48),
            Visible = false
        };

        var rootPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        rootPanel.Widgets.Add(creditsLabel);
        rootPanel.Widgets.Add(departBlockedLabel);

        desktop = new Desktop { Root = rootPanel };

        var soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.AmbientSong);
        soundService.GetSoundInstance(Sounds.AmbientSong)?.Start();
    }

    private void CreateWorldBoundaryWalls()
    {
        float t = 40f;
        float tm = t.ToMeters();

        AddWorldWallSegment(worldWidth.ToMeters(), tm, new Vector2(worldWidth / 2f, t / 2f));
        AddWorldWallSegment(worldWidth.ToMeters(), tm, new Vector2(worldWidth / 2f, worldHeight - t / 2f));
        AddWorldWallSegment(tm, worldHeight.ToMeters(), new Vector2(t / 2f, worldHeight / 2f));
        AddWorldWallSegment(tm, worldHeight.ToMeters(), new Vector2(worldWidth - t / 2f, worldHeight / 2f));
    }

    private void AddWorldWallSegment(float widthMeters, float heightMeters, Vector2 centerPixels)
    {
        Body b = gameplayContext.PhysicsWorld.CreateRectangle(
            widthMeters, heightMeters, 1f, centerPixels.ToMeters(), 0f, BodyType.Static);
        worldBoundaryBodies.Add(b);
    }

    private bool IsTrainTileOccupied(Point tile)
    {
        foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
        {
            if (entity is not AbstractStation station) continue;
            if (prepTrainMap.GetTileIndexFromPixels(station.Position) == tile) return true;
        }

        return false;
    }

    private void OnShopProxyPlacedOnTrain(HubShopOffer proxy, Vector2 tileCenterPixels)
    {
        hubDragOffers.Remove(proxy);
        string kindId = proxy.StationKindId;
        int price = proxy.Cost;
        proxy.Dispose();

        AbstractStation station = StationYardFactory.CreateYardStation(kindId, tileCenterPixels);
        prepTrainMap.MapObjects.Add(station);
        prepTrainMap.SnapToNearestValidCell(station);
        unpaidShopPlacements.Add((station, price));
    }

    private void RestockHubDragOffers()
    {
        float cx = hubPlazaBounds.Center.X;
        float pickupY = hubPlazaBounds.Bottom - 300;
        float dragRowY = hubPlazaBounds.Bottom - 195;
        HubShopOfferTemplate[] offers =
        [
            new(YardStationKindIds.Gunpowder, "Gunpowder",    18, new Color(45, 45, 50),   0f, pickupY),
            new(YardStationKindIds.Cannon,    "Extra cannon", 45, Color.DarkRed,          -170f, dragRowY),
            new(YardStationKindIds.Anvil,     "Extra anvil",  38, Color.DarkSlateGray,     170f, dragRowY),
        ];

        foreach (HubShopOfferTemplate offer in offers)
        {
            if (hubDragOffers.Exists(o => o.StationKindId == offer.KindId)) continue;
            hubDragOffers.Add(new HubShopOffer(
                new Vector2(cx + offer.RelX, offer.SpawnY),
                offer.Cost, offer.KindId, offer.DisplayName, offer.Color,
                prepTrainMap, IsTrainTileOccupied, OnShopProxyPlacedOnTrain));
        }
    }

    private int ComputeUnpaidShopTotalAndPrune()
    {
        int total = 0;
        for (int i = unpaidShopPlacements.Count - 1; i >= 0; i--)
        {
            (AbstractStation station, int price) = unpaidShopPlacements[i];
            if (!prepTrainMap.MapObjects.Contains(station))
                unpaidShopPlacements.RemoveAt(i);
            else
                total += price;
        }

        return total;
    }

    private bool AllPlayersInRectangle(Rectangle r)
    {
        if (players.Count == 0) return false;
        foreach (Player p in players)
            if (!r.Contains((int)p.Position.X, (int)p.Position.Y)) return false;
        return true;
    }

    private void UpdateHubShopTooltip()
    {
        hubShopTooltip.Clear();
        if (phase != HubWorldPhase.Hub) return;

        float reach = Game.GameplayConfig.PlayerInteractDistancePixels * 1.4f;
        float reachSq = reach * reach;
        float liftPx = Game.GameplayConfig.TrainTileSize * 0.55f;

        foreach (Player player in players)
        {
            foreach (HubShopOffer offer in hubDragOffers)
            {
                Vector2 to = offer.Position - player.Position;
                float dsq = to.LengthSquared();
                if (dsq > reachSq || dsq < 4f) continue;

                to.Normalize();
                if (Vector2.Dot(player.LookDirection, to) < 0.5f) continue;

                Vector2 anchor = (offer.Position - cameraPosition) + new Vector2(0f, -liftPx);
                hubShopTooltip.OfferCloser(dsq, offer.BuildTooltipText(), anchor);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isTransitioningToNextLevel)
        {
            hubShopTooltip.Clear();
            departWhiteFilter?.Update(dt);
            if (departWhiteFilter is { IsDone: true })
                Game.SwitchToScreen(new NextLevelIntroScreen(Game));
            return;
        }

        hubSnowEmitter.Position = cameraPosition + new Vector2(virtualScreenSize.X / 2f, virtualScreenSize.Y / 2f);
        creditsLabel.Text = $"Credits: {Game.Credits}";

        int unpaidTotal = ComputeUnpaidShopTotalAndPrune();
        bool allInDepart = phase == HubWorldPhase.Prep && players.Count > 0 && departZone.HaveAllInside(players);

        if (phase == HubWorldPhase.Prep && players.Count > 0)
        {
            departBlockedLabel.Visible = true;
            if (allInDepart && unpaidTotal > Game.Credits)
            {
                departBlockedLabel.Text = $"Need {unpaidTotal}c to depart (you have {Game.Credits}c).";
                departBlockedLabel.TextColor = new Color(255, 120, 120);
            }
            else if (allInDepart)
            {
                departBlockedLabel.Text = "Departing…";
                departBlockedLabel.TextColor = new Color(180, 230, 200);
            }
            else
            {
                departBlockedLabel.Text =
                    "Depart: move all players into the green-tinted zone at the bottom. Shop items are paid when you depart (if you can afford them).";
                departBlockedLabel.TextColor = new Color(160, 200, 170);
            }
        }
        else
        {
            departBlockedLabel.Visible = false;
        }

        if (phase == HubWorldPhase.Hub)
            prepEntryUnlockTimer = Math.Max(0f, prepEntryUnlockTimer - dt);

        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            bool isScrolling = phase is HubWorldPhase.ScrollingToPrep or HubWorldPhase.ScrollingToHub;
            foreach (var player in players)
            {
                if (isScrolling)
                    player.PhysicsBody.LinearVelocity = Vector2.Zero;
                else
                    player.Update(Game.GameplayConfig.FixedTimeStep);
            }

            gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            if (phase is HubWorldPhase.Prep or HubWorldPhase.ScrollingToPrep or HubWorldPhase.ScrollingToHub)
                prepTrainMap.Update(Game.GameplayConfig.FixedTimeStep);

            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }

        bool prepEntryAllInsideNow = players.Count > 0 && AllPlayersInRectangle(prepEntryMarker);
        bool hubPlazaAllInsideNow = players.Count > 0 && AllPlayersInRectangle(hubPlazaBounds);

        if (phase == HubWorldPhase.Hub && prepEntryUnlockTimer <= 0f &&
            prepEntryAllInsideNow && !prepEntryAllInsidePrev)
        {
            phase = HubWorldPhase.ScrollingToPrep;
            gameplayContext.Map = prepTrainMap;
            prepEntryAllInsidePrev = true;
        }

        if (phase == HubWorldPhase.Prep && hubPlazaAllInsideNow && !hubPlazaAllInsidePrev)
        {
            phase = HubWorldPhase.ScrollingToHub;
            hubPlazaAllInsidePrev = true;
        }

        if (phase == HubWorldPhase.ScrollingToPrep)
        {
            cameraPosition.Y = MathHelper.Lerp(cameraPosition.Y, prepViewCameraY, Math.Min(1f, dt * 2.6f));
            if (Math.Abs(cameraPosition.Y - prepViewCameraY) < 1.5f)
            {
                cameraPosition.Y = prepViewCameraY;
                phase = HubWorldPhase.Prep;
            }
        }
        else if (phase == HubWorldPhase.ScrollingToHub)
        {
            cameraPosition.Y = MathHelper.Lerp(cameraPosition.Y, 0f, Math.Min(1f, dt * 2.6f));
            if (cameraPosition.Y < 1.5f)
            {
                cameraPosition = Vector2.Zero;
                phase = HubWorldPhase.Hub;
                gameplayContext.Map = null;
                prepEntryUnlockTimer = 0.5f;
                RestockHubDragOffers();
            }
        }

        if (phase == HubWorldPhase.Hub)
            prepEntryAllInsidePrev = prepEntryAllInsideNow;

        if (phase == HubWorldPhase.Prep)
            hubPlazaAllInsidePrev = hubPlazaAllInsideNow;

        UpdateHubShopTooltip();

        if (allInDepart && unpaidTotal <= Game.Credits && (unpaidTotal == 0 || Game.TrySpendCredits(unpaidTotal)))
        {
            unpaidShopPlacements.Clear();
            Game.PendingPrepTrainLayout = PrepTrainLayout.Capture(prepTrainMap);
            departWhiteFilter ??= new WhiteFilterTransition();
            departWhiteFilter.FadeIn(0.8f);
            isTransitioningToNextLevel = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(15, 15, 20));

        Matrix worldMatrix = Matrix.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0f)
            * viewportAdapter.GetScaleMatrix();

        spriteBatch.Begin(transformMatrix: worldMatrix);
        spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, worldWidth, worldHeight), new Color(15, 15, 20));
        hubMap.Draw(spriteBatch);

        if (phase is HubWorldPhase.Hub or HubWorldPhase.ScrollingToPrep or HubWorldPhase.ScrollingToHub)
            spriteBatch.Draw(AssetManager.BlankTexture, prepEntryMarker, new Color(40, 120, 90) * 0.35f);

        prepTrainMap.Draw(spriteBatch);

        foreach (HubShopOffer offer in hubDragOffers)
            offer.Draw(spriteBatch);

        if (phase is HubWorldPhase.Prep or HubWorldPhase.ScrollingToPrep or HubWorldPhase.ScrollingToHub)
            spriteBatch.Draw(AssetManager.BlankTexture, departMarker, Color.Lime * 0.18f);

        Services.GetService<IVfxService>().Render(spriteBatch);
        foreach (var player in players)
            player.Draw(spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop.Render();
        hubShopTooltip.Draw(spriteBatch, hubTooltipFont, virtualScreenSize);
        float gw = departWhiteFilter?.Opacity ?? 0f;
        if (gw > 0.001f)
            spriteBatch.Draw(AssetManager.BlankTexture,
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y), Color.White * gw);

        spriteBatch.End();
        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        departZone?.Dispose();
        departZone = null;

        foreach (Body b in worldBoundaryBodies)
        {
            if (b.World != null) b.World.Remove(b);
        }

        worldBoundaryBodies.Clear();

        foreach (HubShopOffer offer in hubDragOffers)
            offer.Dispose();

        hubDragOffers.Clear();
        unpaidShopPlacements.Clear();

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        hubMap?.Dispose();
        hubMap = null;
        prepTrainMap = null;
        base.UnloadContent();
    }
}
