using System;
using System.Collections.Generic;
using System.Linq;
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
using Gamelab.Utils;
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
        Intro,
        Running,
        EndOfLevelOutro,
        FailureOutro
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
    private EventInstance battleTheme;

    private FootprintSystem footprintSystem;

    private GameplayPhase phase = GameplayPhase.Intro;
    private readonly FilterTransition screenTransitionFilter = new();
    private bool endOutroToHub;
    private ParticleEmitter snowstormEmitter;
    private SnowstormTransition.Baseline snowstormBaseline;
    private FailureReason? failureReason;
    private bool IsFailureTriggered => failureReason != null;
    private bool levelStatsCommitted;
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
        InitializeVfxIntro();
    }

    private void InitializeVfxIntro()
    {
        if (director != null)
        {
            phase = GameplayPhase.Running;
            screenTransitionFilter.SnapTo(0f);
            return;
        }

        screenTransitionFilter.SnapTo(1f);
        screenTransitionFilter.FadeOut(1);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (UpdatePauseMenu()) return;
        if (phase == GameplayPhase.Intro)
        {
            HandleIntroTransition(dt);
        }
        else if (phase == GameplayPhase.EndOfLevelOutro)
        {
            HandleOutroTransition(dt);
        }
        else if (phase == GameplayPhase.FailureOutro)
        {
            HandleFailureTransition(dt);
        }

        if (phase != GameplayPhase.EndOfLevelOutro)
        {
            levelTimer += dt;
        }

        UpdatePhysics(dt);
        if (IsFailureTriggered) return;

        UpdateSystems(gameTime, dt);

        if (director != null)
        {
            if (phase != GameplayPhase.EndOfLevelOutro)
                director.UpdateSkipProgress(dt, Game.playerManager.Configs.Any(c => c.Input.IsBackButtonHeld()));

            if (director.ConsumePendingHubOutroRequest())
                BeginTutorialHubOutro();
        }
    }

    private void BeginFailureOutro()
    {
        if (phase == GameplayPhase.FailureOutro) return;
        
        trainSound?.Stop();
        battleTheme?.Stop();
        gameplayContext?.State.freezeSound?.Stop();
        soundService?.PlayOnce(Sounds.GameOver);

        foreach (var player in players)
        {
            if (player.IsStunned) continue;
            player.TakeDamage(0f); // Stun all players
        }
        
        screenTransitionFilter.FadeIn(4f, 0f, Color.Black);
        phase = GameplayPhase.FailureOutro;
    }

    private void BeginTutorialHubOutro()
    {
        trainSound?.Stop();
        endOutroToHub = true;
        screenTransitionFilter.FadeIn(2f, 1.5f);
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

        gameplayContext?.Dispose();
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
        battleTheme?.Stop();
        battleTheme?.Dispose();
        hud?.Dispose();
        pauseMenu?.Dispose();
        
        enemyManager?.Dispose();

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

        footprintSystem = new FootprintSystem(Services.GetService<IVfxService>(), players.Count);
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
            phase == GameplayPhase.EndOfLevelOutro ? screenTransitionFilter.EffectIntensity : 0f));
        snowstormBaseline = SnowstormTransition.Capture(snowstormEmitter);
        Services.GetService<IVfxService>().AddContinuous(snowstormEmitter);
    }

    private void InitializeAudio()
    {
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        soundService.LoadSound(Sounds.GameOver);

        trainSound = soundService.GetSoundInstance(Sounds.Train);
        battleTheme = soundService.GetSoundInstance(Sounds.BattleTheme);

        soundService.RegisterParameter(trainSound, "Train Velocity", () => gameplayContext.State.actualSpeed);
        soundService.RegisterParameter(battleTheme, "Intensity", () => enemyManager.intensity);
        trainSound?.Start();
        battleTheme?.Start();
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
        bool levelNotCompleted = phase != GameplayPhase.EndOfLevelOutro;

        while (accumulator >= fixedDt)
        {
            foreach (Player player in players) player.Update(fixedDt);
            if (levelNotCompleted)
            {
                gameplayContext.State.Update(fixedDt);
                gameplayContext.PatchManager.Update(fixedDt, gameplayContext.State);
            }
            
            if (IsFailureTriggered)
            {
                BeginFailureOutro();
                return;
            }

            enemyManager.Update(fixedDt);
            footprintSystem.Update(fixedDt, players, _ => true);
            footprintSystem.UpdateHorses(fixedDt, enemyManager.ActiveEnemies);
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

            if (levelNotCompleted)
            {
                UpdateAllPlayersStunnedFailure(fixedDt);
            }

            accumulator -= fixedDt;
        }
    }

    private void UpdateSystems(GameTime gameTime, float dt)
    {
        worldScroller.Update(dt);
        cameraDirector.Update(camera, dt, players, trainMap.GetBounds(), virtualScreenSize.X, virtualScreenSize.Y,
            allowOffWorldOverflow: true, maxZoom: Game.GameplayConfig.CameraMaxZoom);

        if (phase != GameplayPhase.EndOfLevelOutro)
        {
            hud.Update(currentLevelDef, dt);
            levelWatcher.Update(enemyManager);
            director?.Update(dt, gameplayContext, enemyManager);

            if (director != null && Game.Services.GetService<IDialogueService>() is DialogueManager dm)
                dm.Update(gameTime, Game.playerManager.Configs);
        }
    }

    private void HandleIntroTransition(float dt)
    {
        screenTransitionFilter.Update(dt);
        if (screenTransitionFilter.IsDone)
        {
            phase = GameplayPhase.Running;
        }
    }

    private void HandleOutroTransition(float dt)
    {
        screenTransitionFilter.Update(dt);
        SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline,
            screenTransitionFilter.EffectIntensity);

        if (screenTransitionFilter.IsDone)
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

    private void HandleFailureTransition(float dt)
    {
        screenTransitionFilter.Update(dt);
        SnowstormTransition.ApplyBlizzardIntensity(snowstormEmitter, snowstormBaseline,
            screenTransitionFilter.EffectIntensity);

        if (screenTransitionFilter.IsDone)
        {
            float referenceTime = currentLevelDef.LevelDistance / Game.GameplayConfig.TrainSpeedDefault;
            TriggerFailure();
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
                failureReason = FailureReason.AllPlayersKnockedOut;
        }
        else
        {
            allPlayersStunnedTimer = 0f;
        }
    }

    private void DrawWorld()
    {
        Matrix view = camera.GetViewMatrix();
        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: view
        );
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);
        gameplayContext.PatchManager.Draw(spriteBatch, trainMap);
        enemyManager.Draw(spriteBatch);

        foreach (Player player in players) player.Draw(spriteBatch);

        Services.GetService<IVfxService>().Render(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: RenderUtility.AdditiveBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: view
        );

        trainMap.DrawLightBatch(spriteBatch);
        gameplayContext.PatchManager.DrawLightBatch(spriteBatch, trainMap);
        foreach (Player player in players) player.DrawLightBatch(spriteBatch);
        spriteBatch.End();
    }

    private void DrawUi()
    {
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        hud?.Draw(spriteBatch, virtualScreenSize);
        spriteBatch.End();
        
        GumService.Default.Draw();
        
        float w = screenTransitionFilter.Opacity;
        if (w > 0.001f)
        {
            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
            spriteBatch.Draw(
                AssetManager.BlankTexture, 
                new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                screenTransitionFilter.FilterColor * w
            );
            spriteBatch.End();
        }
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

        failureReason = ResolveTrainFreezeFailureReason(gameplayContext.State);
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

    private void TriggerFailure()
    {
        if (failureReason == null) return;
        if (director != null && failureReason == FailureReason.AllPlayersKnockedOut)
        {
            director.OnAllPlayersKnockedOut();
            return;
        }
        
        CommitLevelStatsToRunSession();
        var snapshot = CreatePostDeathStatsSnapshot();
        SaveManager.DeleteSave();
        Game.SwitchToScreen(new FailScreen(Game, (FailureReason)failureReason, snapshot));
    }

    private void OnLevelCompleted()
    {
        CommitLevelStatsToRunSession();
        completionTime = levelTimer;
        if (director != null)
        {
            gameplayContext.State.VictoryLapActive = true;
            director.OnLevelCompleted();
            return;
        }

        trainSound?.Stop();
        gameplayContext.State.VictoryLapActive = true;
        screenTransitionFilter.FadeIn(2f, 1.5f);
        phase = GameplayPhase.EndOfLevelOutro;
    }
    private void CommitLevelStatsToRunSession()
    {
        if (levelStatsCommitted)
            return;

        levelStatsCommitted = true;
        Game.CurrentRun.AddEnemiesNeutralized(enemyManager?.DefeatedEnemiesCount ?? 0);
        Game.CurrentRun.AddDistanceTravelled(gameplayContext?.State?.DistanceTraveled ?? 0f);
    }

    private PostDeathStatsSnapshot CreatePostDeathStatsSnapshot()
    {
        int stagesDefeated = Math.Max(0, Game.CurrentRun?.CurrentLevel - 1 ?? 0);
        return new PostDeathStatsSnapshot(
            Game.CurrentRun?.TotalEnemiesNeutralized ?? 0,
            Game.CurrentRun?.TotalDistanceTravelledMeters ?? 0f,
            stagesDefeated,
            Game.CurrentRun?.TotalUpgradesBought ?? 0);
    }
}
