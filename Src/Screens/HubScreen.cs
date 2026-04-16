using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Items.Bullets;
using Gamelab.Map.Hub;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Players;
using Gamelab.Screens.Camera;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using Thickness = Myra.Graphics2D.Thickness;

namespace Gamelab.Screens;

/// <param name="RelX">Offset from hub plaza center X.</param>
record struct HubShopOfferTemplate(string KindId, string DisplayName, int Cost, Color Color, float RelX, float SpawnY);

public class HubScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private GameplayContext gameplayContext;
    private HubMap hubMap;
    private TrainMap prepTrainMap;
    private Desktop desktop;
    private Rectangle departMarker;
    private Rectangle hubPlazaBounds;

    private readonly List<Body> worldBoundaryBodies = [];

    private float accumulator;
    private ParticleEmitter hubSnowEmitter;
    private WhiteFilterTransition departWhiteFilter;
    private bool isTransitioningToNextLevel;
    private float departHoldTimer;

    private int worldWidth;
    private int worldHeight;

    private OrthographicCamera camera;
    private CameraDirector cameraDirector;

    private Label creditsLabel;
    private Label departBlockedLabel;
    private WorldUiManager worldUiManager;

    public override void LoadContent()
    {
        base.LoadContent();

        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);
        gameplayContext.State.IsCoalOvenBurning = false;
        gameplayContext.State.FuelBurningEnabled = false;

        worldWidth = virtualScreenSize.X;
        worldHeight = virtualScreenSize.Y * 2;

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
        float prepViewCameraY = virtualScreenSize.Y - 200;
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

        gameplayContext.Map = prepTrainMap;

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

        camera = new OrthographicCamera(viewportAdapter);
        cameraDirector = new CameraDirector(virtualScreenSize);
        cameraDirector.Update(camera, 100f, players, prepTrainMap.GetBounds());
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
        worldUiManager = new WorldUiManager(desktop, Game);
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
            new(YardStationKindIds.BasicProjectile, "Basic projectile", 18, new Color(255, 0, 0), 0f, pickupY),
            new(YardStationKindIds.Cannon, "Extra cannon", 45, Color.DarkRed, -170f, dragRowY),
            new(YardStationKindIds.Workbench, "Extra workbench", 38, Color.DarkSlateGray, 170f, dragRowY),
            new(YardStationKindIds.HomingCasing, "Homing casing upgrade", 50, new Color(0, 119, 255), -85f, pickupY),
            new(YardStationKindIds.ScatterProjectile, "Scatter projectile upgrade", 25, new Color(255, 119, 0), 85f,
                pickupY),
        ];

        foreach (HubShopOfferTemplate offer in offers)
        {
            if (prepTrainMap.MapObjects.Exists(e => e is BuyableStationWrapper o && o.StationKindId == offer.KindId))
                continue;
            prepTrainMap.MapObjects.Add(new BuyableStationWrapper(offer.KindId, offer.Cost,
                new Vector2(cx + offer.RelX, offer.SpawnY)));
        }
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

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isTransitioningToNextLevel)
        {
            worldUiManager.ClearAll();
            departWhiteFilter?.Update(dt);
            if (departWhiteFilter is { IsDone: true })
                Game.SwitchToScreen(new NextLevelIntroScreen(Game));
            return;
        }

        creditsLabel.Text = $"Credits: {Game.Credits}";

        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (var player in players)
                player.Update(Game.GameplayConfig.FixedTimeStep);

            gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            prepTrainMap.Update(Game.GameplayConfig.FixedTimeStep);

            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }

        cameraDirector.Update(camera, dt, players, prepTrainMap.GetBounds());
        hubSnowEmitter.Position = new Vector2(virtualScreenSize.X / 2f, camera.Position.Y);
        bool allInDepart = players.Count > 0 && players.All(p => departMarker.Contains(p.Position));

        Rectangle trainBounds = prepTrainMap.GetBounds();
        int pendingShopCount =
            prepTrainMap.MapObjects.Count(e => e is BuyableStationWrapper && trainBounds.Contains(e.Position));
        if (players.Count > 0)
        {
            departBlockedLabel.Visible = true;
            if (allInDepart && pendingShopCount > 0)
            {
                departBlockedLabel.Text =
                    "Purchase placed shop upgrades (Interact) or drag them back to the vendor before departing.";
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

        worldUiManager.Update(prepTrainMap.MapObjects, camera.GetViewMatrix());

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

        Matrix worldMatrix = camera.GetViewMatrix();
        spriteBatch.Begin(transformMatrix: worldMatrix);

        float scale = worldWidth * 1.0f / AssetManager.HubTexture.Width;
        spriteBatch.Draw(AssetManager.HubTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale,
            SpriteEffects.None, 0f);

        prepTrainMap.Draw(spriteBatch);
        spriteBatch.Draw(AssetManager.GetNPCTexture("Vendor"), new Vector2(1040, 580), null, Color.White, 0f,
            Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
        spriteBatch.Draw(AssetManager.BlankTexture, departMarker, Color.Lime * 0.18f);
        Services.GetService<IVfxService>().Render(spriteBatch);
        foreach (var player in players)
            player.Draw(spriteBatch);

        spriteBatch.End();

        desktop.Render();
        float gw = departWhiteFilter?.Opacity ?? 0f;
        if (gw > 0.001f)
        {
            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
            spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * gw);
            spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        foreach (Body b in worldBoundaryBodies)
        {
            if (b.World != null) b.World.Remove(b);
        }

        worldBoundaryBodies.Clear();
        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        hubMap?.Dispose();
        hubMap = null;
        base.UnloadContent();
    }
}