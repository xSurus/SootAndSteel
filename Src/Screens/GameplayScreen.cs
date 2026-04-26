using System;
using System.Collections.Generic;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Levels;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Players;
using Gamelab.Screens.Camera;
using Gamelab.Serialization;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class GameplayScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private enum GameplayPhase
    {
        Running,
        EndOfLevelOutro
    }

    private GameplayContext gameplayContext;
    private LevelCompletionWatcher levelWatcher;
    private LevelDefinition currentLevelDef;

    private TrainMap trainMap;
    private WorldScroller worldScroller;
    private EnemyManager enemyManager;
    private List<Player> players;

    private OrthographicCamera camera;
    private CameraDirector cameraDirector;

    private GameplayHud hud;
    private PauseMenuController pauseMenu;
    private Desktop desktop;
    private ISoundService soundService;
    private EventInstance trainSound;
    private EventInstance ambientMusic;

    private GameplayPhase phase = GameplayPhase.Running;
    private readonly WhiteFilterTransition endLevelWhiteFilter = new();
    private bool isFailureTriggered;
    private float allPlayersStunnedTimer;
    private float accumulator;

    public override void LoadContent()
    {
        base.LoadContent();

        InitializeContextAndRun();
        InitializeMapAndEntities();
        InitializeCameraAndVfx();
        InitializeAudio();
        InitializeUi();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (UpdatePauseMenu()) return;

        if (phase == GameplayPhase.EndOfLevelOutro)
        {
            HandleLevelTransition(dt);
            return;
        }

        UpdatePhysics(dt);
        if (isFailureTriggered) return;

        UpdateSystems(dt);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        DrawWorld();
        DrawUi();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        if (gameplayContext?.Events != null)
        {
            gameplayContext.Events.OnWallBreached -= OnWallBreached;
            gameplayContext.Events.OnWallRepaired -= OnWallRepaired;
        }

        if (gameplayContext?.State != null)
            gameplayContext.State.OnTrainFrozen -= OnTrainFrozen;

        if (levelWatcher != null)
            levelWatcher.OnLevelCompleted -= OnLevelCompleted;

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();

        trainMap?.Dispose();
        worldScroller?.Dispose();
        trainSound?.Stop();
        trainSound?.Dispose();
        ambientMusic?.Stop();
        ambientMusic?.Dispose();

        base.UnloadContent();
    }

    private void InitializeContextAndRun()
    {
        int playerCount = Math.Max(1, Game.playerManager.Configs.Count);
        gameplayContext = new GameplayContext(virtualScreenSize, Game.CurrentRun);
        Services.AddService(gameplayContext);
        gameplayContext.WorldHeight = virtualScreenSize.Y;
        gameplayContext.State.ConfigurePlayerScaling(playerCount);
        gameplayContext.Events.OnWallBreached += OnWallBreached;
        gameplayContext.Events.OnWallRepaired += OnWallRepaired;
        gameplayContext.State.OnTrainFrozen += OnTrainFrozen;

        var proceduralLevels = new ProceduralLevelProvider(playerCount, Game.CurrentRun);
        currentLevelDef = proceduralLevels.GetLevel(Game.CurrentRun.CurrentLevel);
        levelWatcher = new LevelCompletionWatcher(currentLevelDef);
        levelWatcher.OnLevelCompleted += OnLevelCompleted;
    }

    private void InitializeMapAndEntities()
    {
        trainMap = new TrainMap();
        trainMap.LoadLayout(Game.CurrentRun.TrainLayout);
        gameplayContext.Map = trainMap;

        worldScroller = new WorldScroller(GraphicsDevice);
        enemyManager = new EnemyManager(currentLevelDef);

        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(trainMap.GetTileCenterPixels(playerConfig.PlayerIndex, 1), playerConfig));
        }
    }

    private void InitializeCameraAndVfx()
    {
        camera = new OrthographicCamera(viewportAdapter);
        cameraDirector = new CameraDirector(virtualScreenSize);
        cameraDirector.SnapToCenter(camera, trainMap.GetBounds(), virtualScreenSize.Y);
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm());
    }

    private void InitializeAudio()
    {
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);

        trainSound = soundService.GetSoundInstance(Sounds.Train);
        ambientMusic = soundService.GetSoundInstance(Sounds.AmbientSong);

        soundService.RegisterParameter(trainSound, "Train Velocity", () => gameplayContext.State.actualSpeed);
        trainSound?.Start();
        ambientMusic?.Start();
    }

    private void InitializeUi()
    {
        hud = new GameplayHud();
        pauseMenu = new PauseMenuController();
        pauseMenu.OnExitRequested += () => Game.SwitchToScreen(new global::Gamelab.JoinScreen(Game));

        desktop = new Desktop();
        var mainPanel = new Panel
            { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        mainPanel.Widgets.Add(pauseMenu.Overlay);
        desktop.Root = mainPanel;
    }

    private bool UpdatePauseMenu()
    {
        if (pauseMenu.IsToggleRequested(Game.playerManager.Configs))
            pauseMenu.Toggle();

        if (pauseMenu.IsPaused)
        {
            pauseMenu.Update(Game.playerManager.Configs);
            return true;
        }

        return false;
    }

    private void UpdatePhysics(float dt)
    {
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        float fixedDt = Game.GameplayConfig.FixedTimeStep;

        while (accumulator >= fixedDt)
        {
            foreach (Player player in players) player.Update(fixedDt);

            enemyManager.Update(fixedDt);
            gameplayContext.State.Update(fixedDt);

            if (isFailureTriggered) return;

            trainMap.Update(fixedDt);
            gameplayContext.PhysicsWorld.Step(fixedDt);
            UpdateAllPlayersStunnedFailure(fixedDt);

            if (isFailureTriggered) return;
            accumulator -= fixedDt;
        }
    }

    private void UpdateSystems(float dt)
    {
        worldScroller.Update(dt);
        hud.Update(currentLevelDef);
        levelWatcher.Update(enemyManager);
        cameraDirector.Update(camera, dt, players, trainMap.GetBounds(), virtualScreenSize.Y);
    }

    private void HandleLevelTransition(float dt)
    {
        worldScroller.Update(dt);
        float fixedDt = Game.GameplayConfig.FixedTimeStep;
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= fixedDt)
        {
            foreach (Player player in players) player.Update(fixedDt);
            trainMap.Update(fixedDt);
            gameplayContext.PhysicsWorld.Step(fixedDt);
            accumulator -= fixedDt;
        }

        cameraDirector.Update(camera, dt, players, trainMap.GetBounds(), virtualScreenSize.Y);
        endLevelWhiteFilter.Update(dt);

        if (endLevelWhiteFilter.IsDone)
        {
            Game.CurrentRun.TrainLayout = trainMap.CaptureLayout();
            Game.SwitchToScreen(new PostLevelStatsScreen(Game));
        }
    }

    private void UpdateAllPlayersStunnedFailure(float dt)
    {
        if (players.Count == 0) return;

        bool allPlayersStunned = true;
        foreach (Player player in players)
        {
            if (!player.IsStunned)
            {
                allPlayersStunned = false;
                break;
            }
        }

        if (allPlayersStunned)
        {
            allPlayersStunnedTimer += dt;
            if (allPlayersStunnedTimer >= Game.GameplayConfig.AllPlayersStunnedFailDelaySeconds)
                TriggerFailure();
        }
        else
        {
            allPlayersStunnedTimer = 0f;
        }
    }

    private void DrawWorld()
    {
        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            transformMatrix: camera.GetViewMatrix()
        );
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);
        enemyManager.Draw(spriteBatch);

        foreach (Player player in players) player.Draw(spriteBatch);

        Services.GetService<IVfxService>().Render(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        spriteBatch.End();
    }

    private void DrawUi()
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        hud.Draw(spriteBatch, virtualScreenSize);
        desktop.Render();

        float w = endLevelWhiteFilter.Opacity;
        if (w > 0.001f)
        {
            spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * w);
        }

        pauseMenu.OptionsPanel.Draw(
            spriteBatch,
            virtualScreenSize,
            Game.fontSystem.GetFont(72),
            Game.fontSystem.GetFont(40));

        spriteBatch.End();
    }

    private void OnWallBreached()
    {
        gameplayContext.State.numberBreachedWalls++;
        cameraDirector.TriggerShake(Game.GameplayConfig.ScreenShakeIntensity, Game.GameplayConfig.ScreenShakeDuration);
    }

    private void OnWallRepaired() => gameplayContext.State.numberBreachedWalls--;

    private void OnTrainFrozen()
    {
        if (!gameplayContext.State.VictoryLapActive) TriggerFailure();
    }

    private void TriggerFailure()
    {
        if (isFailureTriggered) return;
        isFailureTriggered = true;
        SaveManager.DeleteSave();
        Game.SwitchToScreen(new FailScreen(Game));
    }

    private void OnLevelCompleted()
    {
        trainSound?.Stop();
        gameplayContext.State.VictoryLapActive = true;
        endLevelWhiteFilter.FadeIn(4f);
        phase = GameplayPhase.EndOfLevelOutro;
    }
}