using System;
using System.Collections.Generic;
using System.Linq;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Players;
using Gamelab.Screens.Camera;
using Gamelab.Serialization;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameGum;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class HubScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private GameplayContext gameplayContext;

    private HubMap hubMap;
    private TrainMap prepTrainMap;
    private readonly HashSet<Player> readyPlayers = [];

    private OrthographicCamera camera;
    private CameraDirector cameraDirector;

    private HubHud hud;
    private WorldUiManager worldUiManager;
    private ParticleEmitter hubSnowEmitter;
    private WhiteFilterTransition departWhiteFilter;
    private PauseMenuController pauseMenu;

    private int worldWidth, worldHeight;
    private float accumulator;
    private float departHoldTimer;
    private bool isTransitioningToNextLevel;

    private EventInstance ambientMusic;

    public override void LoadContent()
    {
        base.LoadContent();

        GumService.Default.Root.Children.Clear();

        InitializeDimensions();
        InitializeContextAndSave();
        InitializeMaps();
        InitializeSpeedLever();
        InitializePlayers();

        camera = new OrthographicCamera(viewportAdapter);
        cameraDirector = new CameraDirector(virtualScreenSize);
        cameraDirector.SnapToCenter(camera, prepTrainMap.GetBounds(), worldWidth, worldHeight,
            allowOffWorldOverflow: true);
        hubSnowEmitter = ParticleFactory.CreateSnowstorm();
        Services.GetService<IVfxService>().AddContinuous(hubSnowEmitter);

        hud = new HubHud(Game);
        worldUiManager = new WorldUiManager(Game);
        pauseMenu = new PauseMenuController();
        pauseMenu.OnExitRequested += () => Game.SwitchToScreen(new Gamelab.JoinScreen(Game));
        if (hud.Desktop.Root is Panel rootPanel) rootPanel.Widgets.Add(pauseMenu.Overlay);

        var soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.AmbientSong);
        ambientMusic = soundService.GetSoundInstance(Sounds.AmbientSong);
        ambientMusic?.Start();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (pauseMenu.IsToggleRequested(Game.playerManager.Configs))
        {
            pauseMenu.Toggle();
            if (pauseMenu.IsPaused) worldUiManager.SetAllVisible(false);
        }

        if (pauseMenu.IsPaused)
        {
            pauseMenu.Update(Game.playerManager.Configs);
            return;
        }

        if (isTransitioningToNextLevel)
        {
            HandleLevelTransition(dt);
            return;
        }

        UpdatePhysics(dt);

        cameraDirector.Update(camera, dt, players, prepTrainMap.GetBounds(), worldWidth, worldHeight,
            allowOffWorldOverflow: true);
        hubSnowEmitter.Position = camera.Position + camera.Origin;
        worldUiManager.Update(prepTrainMap.MapObjects, camera.GetViewMatrix());

        UpdateDepartureLogic(dt);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(15, 15, 20));

        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: camera.GetViewMatrix()
        );

        hubMap.Draw(spriteBatch);
        prepTrainMap.Draw(spriteBatch);
        Services.GetService<IVfxService>().Render(spriteBatch);
        foreach (var player in players) player.Draw(spriteBatch);

        spriteBatch.End();
        hud.Draw();
        GumService.Default.Draw();
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        if (departWhiteFilter != null && departWhiteFilter.Opacity > 0.001f)
        {
            spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * departWhiteFilter.Opacity);
        }

        pauseMenu.OptionsPanel.Draw(
            spriteBatch,
            virtualScreenSize,
            Game.fontSystem.GetFont(72),
            Game.fontSystem.GetFont(40));
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void InitializeContextAndSave()
    {
        SaveManager.SaveRun(Game.CurrentRun);
        gameplayContext = new GameplayContext(virtualScreenSize, Game.CurrentRun);
        Services.AddService(gameplayContext);
        gameplayContext.WorldHeight = worldHeight;
        gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
        gameplayContext.State.actualSpeed = 0;
        gameplayContext.State.IsCoalOvenBurning = false;
        gameplayContext.State.FuelBurningEnabled = false;
    }

    private void InitializeDimensions()
    {
        worldWidth = virtualScreenSize.Y * 2;
        worldHeight = virtualScreenSize.Y * 2;
    }

    private void InitializeMaps()
    {
        hubMap = new HubMap(worldWidth, worldHeight);
        int trainW = Game.GameplayConfig.TrainWidth * Game.GameplayConfig.TrainTileSize;
        int gapMid = Game.GameplayConfig.TrainWidth / 2;
        float prepViewCameraY = virtualScreenSize.Y - 200;

        prepTrainMap = new TrainMap(
            new Vector2((worldWidth - trainW) / 2f, prepViewCameraY + 360f),
            new DoorSpec(OnBottom: false, Column: gapMid));

        prepTrainMap.LoadLayout(Game.CurrentRun.TrainLayout);
        gameplayContext.Map = prepTrainMap;
        int hubSeed = unchecked(Game.CurrentRun.RunSeed + Game.CurrentRun.CurrentLevel * 4242);
        hubMap.RestockHubDragOffers(new Random(hubSeed), 4);
    }

    private void InitializePlayers()
    {
        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(prepTrainMap.GetTileCenterPixels(playerConfig.PlayerIndex, 1), playerConfig));
        }
    }

    private void InitializeSpeedLever()
    {
        foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
        {
            if (entity is SpeedLever lever)
            {
                lever.OnInteractOverride = player =>
                {
                    if (!readyPlayers.Remove(player))
                        readyPlayers.Add(player);
                };
                break;
            }
        }
    }

    private void UpdatePhysics(float dt)
    {
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (var player in players)
                player.Update(Game.GameplayConfig.FixedTimeStep);

            gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            prepTrainMap.Update(Game.GameplayConfig.FixedTimeStep);

            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }
    }

    private void UpdateDepartureLogic(float dt)
    {
        readyPlayers.RemoveWhere(p => !players.Contains(p));
        bool allReady = players.Count > 0 && readyPlayers.Count == players.Count;
        int pendingShopCount = prepTrainMap.MapObjects.Count(e =>
            e is BuyableStationWrapper && prepTrainMap.GetBounds().Contains(e.Position));
        hud.Update(allReady, pendingShopCount, departHoldTimer, Game.GameplayConfig.DepartHoldSeconds,
            readyPlayers.Count, players.Count);

        bool canDepart = allReady && pendingShopCount == 0;
        departHoldTimer = canDepart ? departHoldTimer + dt : 0f;

        if (departHoldTimer >= Game.GameplayConfig.DepartHoldSeconds)
        {
            Game.CurrentRun.TrainLayout = prepTrainMap.CaptureLayout();
            departWhiteFilter ??= new WhiteFilterTransition();
            departWhiteFilter.FadeIn(0.8f);
            isTransitioningToNextLevel = true;
        }
    }

    private void HandleLevelTransition(float dt)
    {
        worldUiManager.ClearAll();
        departWhiteFilter?.Update(dt);
        if (departWhiteFilter is { IsDone: true })
        {
            Game.SwitchToScreen(new NextLevelIntroScreen(Game));
        }
    }

    public override void UnloadContent()
    {
        worldUiManager?.ClearAll();
        GumService.Default.Root.Children.Clear();
        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        hubMap?.Dispose();
        hubMap = null;
        prepTrainMap = null;
        ambientMusic?.Stop();
        ambientMusic?.Dispose();
        if (players != null)
        {
            foreach (Player player in players)
            {
                player.Dispose();
            }

            players.Clear();
        }

        base.UnloadContent();
    }
}