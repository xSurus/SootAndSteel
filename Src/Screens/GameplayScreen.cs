using System;
using System.Collections.Generic;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Dialogue;
using Gamelab.Enemies;
using Gamelab.Levels;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Particles.Modifiers;
using Gamelab.Players;
using Gamelab.Screens.Camera;
using Gamelab.Serialization;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Tutorial;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameGum;

namespace Gamelab.Screens;

public class GameplayScreen : GamelabGameScreen
{
    private readonly ITutorialDirector director;

    public GameplayScreen(GamelabGame game) : this(game, null)
    {
    }

    public GameplayScreen(GamelabGame game, ITutorialDirector director) : base(game)
    {
        this.director = director;
    }

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
    private ISoundService soundService;
    private EventInstance trainSound;
    private EventInstance ambientMusic;

    private GameplayPhase phase = GameplayPhase.Running;
    private readonly WhiteFilterTransition endLevelWhiteFilter = new();
    private bool endOutroToHub;
    private ParticleEmitter snowstormEmitter;
    private SnowstormTransition.Baseline snowstormBaseline;
    private bool isFailureTriggered;
    private float allPlayersStunnedTimer;
    private float accumulator;
    private float levelTimer;
    private float completionTime;

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

        levelTimer += dt;
        UpdatePhysics(dt);
        if (isFailureTriggered) return;

        UpdateSystems(gameTime, dt);

        if (director != null && director.ConsumePendingHubOutroRequest())
            BeginTutorialHubOutro();
    }

    private void BeginTutorialHubOutro()
    {
        trainSound?.Stop();
        endOutroToHub = true;
        endLevelWhiteFilter.FadeIn(4f);
        phase = GameplayPhase.EndOfLevelOutro;
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
        if (director != null)
        {
            if (Game.Services.GetService<IDialogueService>() is DialogueManager tutorialDialogue)
                tutorialDialogue.ClearTutorialGuidance();
            Game.Services.RemoveService(typeof(IDialogueService));
        }

        Services.GetService<IVfxService>().ClearAll();

        if (players != null)
        {
            foreach (var player in players)
            {
                player.Dispose();
            }

            players.Clear();
        }

        trainMap?.Dispose();
        worldScroller?.Dispose();
        trainSound?.Stop();
        trainSound?.Dispose();
        ambientMusic?.Stop();
        ambientMusic?.Dispose();
        pauseMenu?.Dispose();

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

        ILevelProvider levelProvider = director != null
            ? new TutorialLevelProvider()
            : new ProceduralLevelProvider(playerCount, Game.CurrentRun);
        currentLevelDef = levelProvider.GetLevel(Game.CurrentRun.CurrentLevel);
        levelWatcher = new LevelCompletionWatcher(currentLevelDef);
        levelWatcher.OnLevelCompleted += OnLevelCompleted;
    }

    private void InitializeMapAndEntities()
    {
        trainMap = new TrainMap();
        trainMap.LoadLayout(Game.CurrentRun.TrainLayout);
        gameplayContext.Map = trainMap;
        gameplayContext.PatchManager = new PatchManager(trainMap);

        worldScroller = new WorldScroller();
        enemyManager = new EnemyManager(currentLevelDef);

        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(trainMap.GetTileCenterPixels(playerConfig.PlayerIndex, 1), playerConfig));
        }

        director?.Initialize(trainMap, gameplayContext);
    }

    private void InitializeCameraAndVfx()
    {
        camera = new OrthographicCamera(viewportAdapter);
        cameraDirector = new CameraDirector(virtualScreenSize);
        cameraDirector.SnapToCenter(camera, trainMap.GetBounds(), virtualScreenSize.X, virtualScreenSize.Y,
            allowOffWorldOverflow: true, maxZoom: Game.GameplayConfig.CameraMaxZoom);
        snowstormEmitter = ParticleFactory.CreateSnowstorm();
        snowstormEmitter.Modifiers.Add(new BlizzardGustModifier(() =>
            phase == GameplayPhase.EndOfLevelOutro ? endLevelWhiteFilter.Opacity : 0f));
        snowstormBaseline = SnowstormTransition.Capture(snowstormEmitter);
        Services.GetService<IVfxService>().AddContinuous(snowstormEmitter);
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
        pauseMenu.OnExitRequested += () => Game.SwitchToScreen(new Gamelab.JoinScreen(Game));

        if (director != null)
        {
            var tutorialOverlay = new DialogueOverlay();
            var dialogueManager = new DialogueManager(tutorialOverlay, () => camera.GetViewMatrix());
            Game.Services.AddService<IDialogueService>(dialogueManager);
        }
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
            gameplayContext.PatchManager.Update(fixedDt, gameplayContext.State);

            if (isFailureTriggered) return;

            trainMap.Update(fixedDt);
            try
            {
                gameplayContext.BeginPhysicsStep();
                gameplayContext.PhysicsWorld.Step(fixedDt);
            }
            finally
            {
                gameplayContext.EndPhysicsStep();
            }

            UpdateAllPlayersStunnedFailure(fixedDt);

            if (isFailureTriggered) return;
            accumulator -= fixedDt;
        }
    }

    private void UpdateSystems(GameTime gameTime, float dt)
    {
        worldScroller.Update(dt);
        hud.Update(currentLevelDef);
        levelWatcher.Update(enemyManager);
        director?.Update(dt, gameplayContext, enemyManager);
        if (director != null && Game.Services.GetService<IDialogueService>() is DialogueManager dm)
            dm.Update(gameTime, Game.playerManager.Configs);
        cameraDirector.Update(camera, dt, players, trainMap.GetBounds(), virtualScreenSize.X, virtualScreenSize.Y,
            allowOffWorldOverflow: true, maxZoom: Game.GameplayConfig.CameraMaxZoom);
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
            try
            {
                gameplayContext.BeginPhysicsStep();
                gameplayContext.PhysicsWorld.Step(fixedDt);
            }
            finally
            {
                gameplayContext.EndPhysicsStep();
            }

            accumulator -= fixedDt;
        }

        cameraDirector.Update(camera, dt, players, trainMap.GetBounds(), virtualScreenSize.X, virtualScreenSize.Y,
            allowOffWorldOverflow: true, maxZoom: Game.GameplayConfig.CameraMaxZoom);
        endLevelWhiteFilter.Update(dt);
        SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline, endLevelWhiteFilter.Opacity);

        if (endLevelWhiteFilter.IsDone)
        {
            Game.CurrentRun.TrainLayout = trainMap.CaptureLayout();
            if (endOutroToHub)
                Game.SwitchToScreen(new HubScreen(Game));
            else
            {
                float referenceTime = currentLevelDef.LevelDistance / Game.GameplayConfig.TrainSpeedDefault;
                Game.SwitchToScreen(new PostLevelStatsScreen(Game, completionTime, referenceTime));
            }
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
                TriggerFailure(FailureReason.AllPlayersKnockedOut);
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
            samplerState: SamplerState.PointClamp,
            transformMatrix: camera.GetViewMatrix()
        );
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);
        gameplayContext.PatchManager.Draw(spriteBatch, trainMap);
        enemyManager.Draw(spriteBatch);

        foreach (Player player in players) player.Draw(spriteBatch);

        Services.GetService<IVfxService>().Render(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        director?.DrawWorld(spriteBatch);
        spriteBatch.End();
    }

    private void DrawUi()
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        hud.Draw(spriteBatch, virtualScreenSize);
        director?.DrawHud(spriteBatch, virtualScreenSize);

        float w = endLevelWhiteFilter.Opacity;
        if (w > 0.001f)
        {
            spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * w);
        }

        spriteBatch.End();
        GumService.Default.Draw();
    }

    private void OnWallBreached()
    {
        gameplayContext.State.numberBreachedWalls++;
        cameraDirector.TriggerShake(Game.GameplayConfig.ScreenShakeIntensity, Game.GameplayConfig.ScreenShakeDuration);
    }

    private void OnWallRepaired() => gameplayContext.State.numberBreachedWalls--;

    private void OnTrainFrozen()
    {
        if (gameplayContext.State.VictoryLapActive) return;
        if (director != null)
        {
            director.OnTrainFrozen();
            return;
        }
        TriggerFailure(ResolveTrainFreezeFailureReason(gameplayContext.State));
    }


    private static FailureReason ResolveTrainFreezeFailureReason(TrainState state)
    {
        bool breaches = state.numberBreachedWalls > 0;
        bool furnaceOut = !state.IsCoalOvenBurning;
        if (breaches && furnaceOut) return FailureReason.TrainFrozenBreachesAndFurnaceOut;
        if (breaches) return FailureReason.TrainFrozenHullBreached;
        if (furnaceOut) return FailureReason.TrainFrozenFurnaceOut;
        return FailureReason.TrainFrozenOther;
    }

    private void TriggerFailure(FailureReason reason)
    {
        if (isFailureTriggered) return;
        if (director != null && reason == FailureReason.AllPlayersKnockedOut)
        {
            isFailureTriggered = true;
            director.OnAllPlayersKnockedOut();
            return;
        }
        isFailureTriggered = true;
        SaveManager.DeleteSave();
        Game.SwitchToScreen(new FailScreen(Game, reason));
    }

    private void OnLevelCompleted()
    {
        completionTime = levelTimer;
        if (director != null)
        {
            gameplayContext.State.VictoryLapActive = true;
            director.OnLevelCompleted();
            return;
        }
        trainSound?.Stop();
        gameplayContext.State.VictoryLapActive = true;
        for (int i = 0; i < 5; i++)
        {
            snowstormEmitter?.Emit();
        }

        endLevelWhiteFilter.FadeIn(4f);
        phase = GameplayPhase.EndOfLevelOutro;
    }
}
