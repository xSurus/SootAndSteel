using System;
using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Map.Hub;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Items.Bullets;
using Gamelab.Particles;
using Gamelab.PhysicalEntities;
using Gamelab.Players;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
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
    private Rectangle prepEntryMarker;
    private Rectangle departMarker;
    private Rectangle hubPlazaBounds;

    private readonly List<Body> worldBoundaryBodies = [];

    private float accumulator;
    private ParticleEmitter hubSnowEmitter;
    private WhiteFilterTransition departWhiteFilter;
    private bool isTransitioningToNextLevel;
    private float departHoldTimer;

    private Vector2 cameraPosition;
    private int worldWidth;
    private int worldHeight;
    private float prepViewCameraY;
    private HubWorldPhase phase = HubWorldPhase.Hub;

    private readonly List<HubShopOffer> hubDragOffers = [];

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
        gameplayContext.State.FuelBurningEnabled = false;

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
        int gapMid = Game.GameplayConfig.TrainWidth / 2;
        prepTrainMap = new TrainMap(
            new Vector2((worldWidth - trainW) / 2f, prepViewCameraY + 360f),
            new DoorSpec(OnBottom: false, Column: gapMid),
            new DoorSpec(OnBottom: true, Column: gapMid));

        prepTrainMap.AddDefaultStructures();

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

        if (Game.CurrentLevel == 0)
            rootPanel.Widgets.Add(BuildControlsHelpPanel());

        desktop = new Desktop { Root = rootPanel };

        var soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.AmbientSong);
        soundService.GetSoundInstance(Sounds.AmbientSong)?.Start();
    }

    private Widget BuildControlsHelpPanel()
    {
        var font = Game.fontSystem.GetFont(18);
        var headerFont = Game.fontSystem.GetFont(22);
        var dimWhite = new Color(210, 210, 220);

        var stack = new VerticalStackPanel
        {
            Spacing = 3,
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidBrush(new Color(10, 10, 15, 180)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14, 14, 0, 0)
        };

        stack.Widgets.Add(new Label
        {
            Text = "Controls",
            Font = headerFont,
            TextColor = new Color(255, 220, 100)
        });

        (string button, string action)[] entries =
        [
            ("Left Stick / D-Pad", "Move"),
            ("X", "Interact / Repair"),
            ("Y", "Grab items"),
            ("A", "Pick up / Confirm"),
            ("Start", "Pause"),
        ];

        foreach ((string button, string action) in entries)
        {
            stack.Widgets.Add(new Label
            {
                Text = $"  {button}  —  {action}",
                Font = font,
                TextColor = dimWhite
            });
        }

        return stack;
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

    private void RestockHubDragOffers()
    {
        float cx = hubPlazaBounds.Center.X;
        float pickupY = hubPlazaBounds.Bottom - 300;
        float dragRowY = hubPlazaBounds.Bottom - 195;
        HubShopOfferTemplate[] offers =
        [
            new(YardStationKindIds.BasicProjectile, "Basic projectile",    18, new Color(255, 0, 0),    0f, pickupY),
            new(YardStationKindIds.Cannon,    "Extra cannon", 45, Color.DarkRed,          -170f, dragRowY),
            new(YardStationKindIds.Workbench,     "Extra workbench",  38, Color.DarkSlateGray,     170f, dragRowY),
            new(YardStationKindIds.HomingCasing, "Homing casing upgrade",   50, new Color(0, 119, 255), -85f, pickupY),
            new(YardStationKindIds.ScatterProjectile, "Scatter projectile upgrade",  25, new Color(255, 119, 0), 85f, pickupY),
        ];

        foreach (HubShopOfferTemplate offer in offers)
        {
            if (hubDragOffers.Exists(o => o.Type == offer.KindId)) continue;
            if (prepTrainMap.MapObjects.Exists(e => e is HubShopOffer o && o.Type == offer.KindId)) continue;
            hubDragOffers.Add(new HubShopOffer(
                new Vector2(cx + offer.RelX, offer.SpawnY),
                offer.Cost, offer.KindId, offer.DisplayName, offer.Color,
                prepTrainMap,
                hubDragOffers));
        }
    }

    private bool AllPlayersInRectangle(Rectangle r)
    {
        if (players.Count == 0) return false;
        foreach (Player p in players)
            if (!r.Contains((int)p.Position.X, (int)p.Position.Y)) return false;
        return true;
    }

    private int CountUnpurchasedShopOffers()
    {
        int count = 0;
        foreach (IPhysicalEntity e in prepTrainMap.MapObjects)
            if (e is HubShopOffer offer && !offer.IsPurchased)
                count++;
        return count;
    }

    private void UpdateContextualTooltips()
    {
        hubShopTooltip.Clear();

        TryOfferHubShopTooltip();

        if (Game.CurrentLevel == 0)
            TryOfferPrepStationTooltips();
    }

    private void TryOfferHubShopTooltip()
    {
        float reach = Game.GameplayConfig.PlayerInteractDistancePixels * 1.4f;
        float reachSq = reach * reach;
        float liftPx = Game.GameplayConfig.TrainTileSize * 0.55f;

        foreach (Player player in players)
        {
            if (phase == HubWorldPhase.Hub)
            {
                foreach (HubShopOffer offer in hubDragOffers)
                    TryOfferTooltip(player, offer, reachSq, liftPx);
            }
            else
            {
                foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
                    if (entity is HubShopOffer offer && !offer.IsPurchased)
                        TryOfferTooltip(player, offer, reachSq, liftPx);
            }
        }
    }

    private void TryOfferPrepStationTooltips()
    {
        if (phase != HubWorldPhase.Prep) return;

        float reach = Game.GameplayConfig.PlayerInteractDistancePixels * 1.7f;
        float reachSq = reach * reach;
        float liftPx = Game.GameplayConfig.TrainTileSize * 0.60f;

        foreach (Player player in players)
        {
            foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
            {
                string tooltip = BuildPrepStationTooltip(entity);
                if (string.IsNullOrEmpty(tooltip)) continue;

                Vector2 to = entity.Position - player.Position;
                float dsq = to.LengthSquared();
                if (dsq > reachSq || dsq < 4f) continue;
                to.Normalize();
                if (Vector2.Dot(player.LookDirection, to) < 0.42f) continue;

                hubShopTooltip.OfferCloser(
                    dsq,
                    tooltip,
                    (entity.Position - cameraPosition) + new Vector2(0f, -liftPx));
            }
        }
    }

    private static string BuildPrepStationTooltip(IPhysicalEntity entity)
    {
        return entity switch
        {
            CannonStation => "Cannon\nLoad ammo with A, then Interact (X) to fire.",
            AbstractStation station when station.Type is "Anvil" or "Workbench" =>
                "Workbench (Anvil)\nPlace ingredients with A.\nHold Interact (X) to craft or repair.",
            ComponentResource componentResource => BuildComponentResourceTooltip(componentResource),
            SpeedLever => "Speed Lever\nInteract (X) to cycle train speed.\nHold Interact (X) to repair.",
            CopperResource => "Copper Pile\nPickup (A) to take or return copper.",
            GunpowderResource => "Gunpowder Pile\nPickup (A) to take or return gunpowder.",
            CoalWagon => "Coal Wagon\nPickup (A) to take or return coal.",
            TrainNose => "Furnace\nBring coal here and Pickup (A) to refuel the engine.",
            Counter => "Counter\nPickup (A) swaps your item with the counter item.",
            _ => ""
        };
    }

    private static string BuildComponentResourceTooltip(ComponentResource resource)
    {
        string componentName = GetComponentDisplayName(resource.componentId);
        return $"Component Bench ({componentName})\nPickup (A) to take or return this component.";
    }

    private static string GetComponentDisplayName(string componentId)
    {
        try
        {
            return ComponentRegistry.GetName(componentId);
        }
        catch
        {
            return componentId;
        }
    }

    private void TryOfferTooltip(Player player, HubShopOffer offer, float reachSq, float liftPx)
    {
        Vector2 to = offer.Position - player.Position;
        float dsq = to.LengthSquared();
        if (dsq > reachSq || dsq < 4f) return;
        to.Normalize();
        if (Vector2.Dot(player.LookDirection, to) < 0.5f) return;
        hubShopTooltip.OfferCloser(dsq, offer.BuildTooltipText(), (offer.Position - cameraPosition) + new Vector2(0f, -liftPx));
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

        int pendingShopCount = CountUnpurchasedShopOffers();

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

        // Position-based check each frame (same as prep strip). Physics sensor callbacks were unreliable on exit.
        bool allInDepart = phase == HubWorldPhase.Prep && players.Count > 0 && AllPlayersInRectangle(departMarker);

        if (phase == HubWorldPhase.Prep && players.Count > 0)
        {
            departBlockedLabel.Visible = true;
            if (allInDepart && pendingShopCount > 0)
            {
                departBlockedLabel.Text = "Purchase placed shop upgrades (Interact) or drag them back to the vendor before departing.";
                departBlockedLabel.TextColor = new Color(255, 120, 120);
            }
            else if (allInDepart)
            {
                float hold = Game.GameplayConfig.DepartHoldSeconds;
                if (hold <= 0f || departHoldTimer >= hold)
                    departBlockedLabel.Text = "Departing…";
                else
                    departBlockedLabel.Text = $"Stay in zone to depart ({hold - departHoldTimer:0.0}s)…";
                departBlockedLabel.TextColor = new Color(180, 230, 200);
            }
            else if (pendingShopCount > 0)
            {
                departBlockedLabel.Text =
                    "Interact next to a colored tile on the train to buy it, or grab it and return it to the shop row to cancel.";
                departBlockedLabel.TextColor = new Color(200, 200, 120);
            }
            else
            {
                departBlockedLabel.Text =
                    "Depart: move all players into the green-tinted zone at the bottom.";
                departBlockedLabel.TextColor = new Color(160, 200, 170);
            }
        }
        else
        {
            departBlockedLabel.Visible = false;
        }

        UpdateContextualTooltips();

        bool canDepart = allInDepart && pendingShopCount == 0;
        if (canDepart)
            departHoldTimer += dt;
        else
            departHoldTimer = 0f;

        if (departHoldTimer >= Game.GameplayConfig.DepartHoldSeconds)
        {
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
        float scale = worldWidth * 1.0f / AssetManager.HubTexture.Width;
        spriteBatch.Begin(transformMatrix: worldMatrix);
        spriteBatch.Draw(AssetManager.HubTexture, new Vector2(0), null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        
        // spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, worldWidth, worldHeight), new Color(15, 15, 20));
        // hubMap.Draw(spriteBatch);

        if (phase is HubWorldPhase.Hub or HubWorldPhase.ScrollingToPrep or HubWorldPhase.ScrollingToHub)
            spriteBatch.Draw(AssetManager.BlankTexture, prepEntryMarker, new Color(40, 120, 90) * 0.35f);

        prepTrainMap.Draw(spriteBatch);

        foreach (HubShopOffer offer in hubDragOffers)
            offer.Draw(spriteBatch);

        spriteBatch.Draw(AssetManager.GetNPCTexture("Vendor"), new Vector2(1040,580), null, Color.White,
                                0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0f);

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
        foreach (Body b in worldBoundaryBodies)
        {
            if (b.World != null) b.World.Remove(b);
        }

        worldBoundaryBodies.Clear();

        hubDragOffers.Clear();

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        hubMap?.Dispose();
        hubMap = null;
        prepTrainMap = null;
        base.UnloadContent();
    }
}
