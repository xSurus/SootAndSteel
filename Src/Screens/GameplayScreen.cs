using System;
using System.Collections.Generic;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Levels;
using Gamelab.Map;
using Gamelab.Map.Hub;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.Players;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class GameplayScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private enum GameplayPhase
    {
        Running,
        EndOfLevelOutro,
    }

    private List<Player> players;
    private TrainMap trainMap;
    private WorldScroller worldScroller;
    private GameplayContext gameplayContext;
    private EnemyManager enemyManager;
    private RunManager runManager;
    private LevelDefinition currentLevelDef;
    private Desktop desktop;
    private ProjectileManager projectileManager;
    private GameplayHud hud;
    private PauseMenuController pauseMenu;

    private ISoundService soundService;
    private EventInstance trainSound;
    private EventInstance ambientMusic;

    private float screenShakeTimer;
    private float screenShakeIntensity;
    private Vector2 screenShakeOffset;
    private readonly Random random = Random.Shared;
    private bool isFailureTriggered;
    private float levelStartDistance;
    private float allPlayersStunnedTimer;
    private GameplayPhase phase = GameplayPhase.Running;
    private readonly WhiteFilterTransition endLevelWhiteFilter = new WhiteFilterTransition();

    public override void LoadContent()
    {
        base.LoadContent();
        int playerCount = Math.Max(1, Game.playerManager.Configs.Count);
        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);
        gameplayContext.State.ConfigurePlayerScaling(playerCount);
        runManager = new RunManager(Game.CurrentLevel, new ProgressiveRunLevelProvider(playerCount));
        runManager.OnIntermissionStarted += OnIntermissionStarted;
        currentLevelDef = runManager.CurrentLevelDefinition;
        levelStartDistance = 0f;
        trainMap = new TrainMap();
        gameplayContext.Map = trainMap;
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm());

        // Sounds
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        trainSound = soundService.GetSoundInstance(Sounds.Train);
        ambientMusic = soundService.GetSoundInstance(Sounds.AmbientSong);
        soundService.RegisterParameter(trainSound, "Train Velocity", () => gameplayContext.State.actualSpeed);
        trainSound?.Start();
        ambientMusic?.Start();

        trainMap.AddDefaultStructures();
        PrepTrainLayout.ApplyFromPendingOrDefault(Game, trainMap);

        worldScroller = new WorldScroller(GraphicsDevice);
        projectileManager = new ProjectileManager();
        enemyManager = new EnemyManager(currentLevelDef);

        gameplayContext.Events.OnWallBreached += OnWallBreached;
        gameplayContext.Events.OnWallRepaired += OnWallRepaired;
        gameplayContext.State.OnTrainFrozen += OnTrainFrozen;

        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(trainMap.GetTileCenterPixels(playerConfig.PlayerIndex, 1), playerConfig));
        }

        hud = new GameplayHud();
        pauseMenu = new PauseMenuController();
        pauseMenu.OnExitRequested += () => Game.SwitchToScreen(new JoinScreen(Game));

        desktop = new Desktop();
        var mainPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        mainPanel.Widgets.Add(pauseMenu.Overlay);
        desktop.Root = mainPanel;
    }

    private void OnWallBreached()
    {
        gameplayContext.State.numberBreachedWalls++;
        screenShakeTimer = Game.GameplayConfig.ScreenShakeDuration;
        screenShakeIntensity = Game.GameplayConfig.ScreenShakeIntensity;
    }

    private void OnWallRepaired()
    {
        gameplayContext.State.numberBreachedWalls--;
    }

    private void OnTrainFrozen()
    {
        if (gameplayContext.State.VictoryLapActive)
        {
            return;
        }

        TriggerFailure();
    }

    private void TriggerFailure()
    {
        if (isFailureTriggered) return;
        isFailureTriggered = true;
        Game.SwitchToScreen(new FailScreen(Game));
    }

    private float accumulator;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (pauseMenu.IsToggleRequested(Game.playerManager.Configs))
        {
            pauseMenu.Toggle();
        }

        if (pauseMenu.IsPaused)
        {
            pauseMenu.Update(Game.playerManager.Configs);
            return;
        }

        bool isEndOfLevelOutro = phase == GameplayPhase.EndOfLevelOutro;
        if (isEndOfLevelOutro)
        {
            endLevelWhiteFilter.Update(dt);
        }

        worldScroller.Update(dt);

        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        float fixedDt = Game.GameplayConfig.FixedTimeStep;
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update(fixedDt);
            }

            if (!isEndOfLevelOutro)
            {
                enemyManager.Update(fixedDt);
                projectileManager.Update(fixedDt);
                gameplayContext.State.Update(fixedDt);
                if (isFailureTriggered) return;
            }

            trainMap.Update(fixedDt);
            gameplayContext.PhysicsWorld.Step(fixedDt);
            gameplayContext.FlushDeferredPhysicsActions();
            UpdateAllPlayersStunnedFailure(fixedDt);
            if (isFailureTriggered) return;
            accumulator -= fixedDt;
        }

        hud.Update(currentLevelDef, levelStartDistance);

        if (!isEndOfLevelOutro)
        {
            runManager.Update(gameplayContext, enemyManager);
        }

        UpdateScreenShake(dt);

        if (isEndOfLevelOutro && endLevelWhiteFilter.IsDone)
        {
            Game.TrainLayoutSeedForHub = PrepTrainLayout.Capture(trainMap);
            Game.SwitchToScreen(new PostLevelStatsScreen(Game));
        }
    }

    private void UpdateAllPlayersStunnedFailure(float dt)
    {
        bool allPlayersStunned = players.Count > 0;
        foreach (Player player in players)
        {
            if (!player.IsStunned)
            {
                allPlayersStunned = false;
                break;
            }
        }

        if (!allPlayersStunned)
        {
            allPlayersStunnedTimer = 0f;
            return;
        }

        allPlayersStunnedTimer += dt;
        if (allPlayersStunnedTimer >= Game.GameplayConfig.AllPlayersStunnedFailDelaySeconds)
        {
            TriggerFailure();
        }
    }

    private void UpdateScreenShake(float dt)
    {
        if (screenShakeTimer > 0)
        {
            screenShakeTimer -= dt;

            float shakeX = (float)(random.NextDouble() * 2 - 1) * screenShakeIntensity;
            float shakeY = (float)(random.NextDouble() * 2 - 1) * screenShakeIntensity;
            screenShakeOffset = new Vector2(shakeX, shakeY);

            screenShakeIntensity *= 1f - Game.GameplayConfig.ScreenShakeDecay * dt;

            if (screenShakeTimer <= 0)
            {
                screenShakeOffset = Vector2.Zero;
                screenShakeIntensity = 0;
            }
        }
    }

    private void OnIntermissionStarted(int _)
    {
        trainSound?.Stop();
        gameplayContext.State.VictoryLapActive = true;
        ScreenPayloads.LastPostLevelResults = new ScreenPayloads.PostLevelResults
        {
            CompletedLevelNumber = Game.CurrentLevel,
            CoalRemaining = gameplayContext.State.CoalAmount
        };
        endLevelWhiteFilter.FadeIn(4f);
        phase = GameplayPhase.EndOfLevelOutro;
    }

    public override void Draw(GameTime gameTime)
    {
        Matrix shakeMatrix = Matrix.CreateTranslation(screenShakeOffset.X, screenShakeOffset.Y, 0);
        Matrix finalTransform = shakeMatrix * viewportAdapter.GetScaleMatrix();

        spriteBatch.Begin(transformMatrix: finalTransform);
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);
        enemyManager.Draw(spriteBatch);

        foreach (Player player in players)
        {
            player.Draw(spriteBatch);
        }

        Services.GetService<IVfxService>().Render(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        hud.Draw(spriteBatch, virtualScreenSize);
        desktop.Render();
        float w = endLevelWhiteFilter.Opacity;
        if (w > 0.001f)
        {
            spriteBatch.Draw(
                AssetManager.BlankTexture,
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * w);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    public override void UnloadContent()
    {
        if (gameplayContext.Events != null)
        {
            gameplayContext.Events.OnWallBreached -= OnWallBreached;
            gameplayContext.Events.OnWallRepaired -= OnWallRepaired;
        }

        if (runManager != null)
        {
            runManager.OnIntermissionStarted -= OnIntermissionStarted;
        }

        if (gameplayContext?.State != null)
        {
            gameplayContext.State.OnTrainFrozen -= OnTrainFrozen;
        }

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        trainMap?.Dispose();
        worldScroller?.Dispose();

        trainSound?.Stop();
        ambientMusic?.Stop();
        trainSound?.Dispose();
        ambientMusic?.Dispose();

        base.UnloadContent();
    }
}