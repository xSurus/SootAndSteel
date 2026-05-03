using System;
using System.Collections.Generic;
using System.Linq;
using FmodForFoxes.Studio;
using Gamelab.Assets;
using Gamelab.Dialogue;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.Particles.Modifiers;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.Components;
using Gamelab.Players;
using Gamelab.Screens.Camera;
using Gamelab.Serialization;
using Gamelab.Services.Bullet;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameGum;

namespace Gamelab.Screens;

public class HubScreen(GamelabGame game) : GamelabGameScreen(game)
{
    private List<Player> players;
    private GameplayContext gameplayContext;

    private HubMap hubMap;
    private TrainMap prepTrainMap;
    private readonly HashSet<int> readyPlayers = [];

    private OrthographicCamera camera;
    private CameraDirector cameraDirector;

    private HubOverlay hubOverlay;
    private WorldUiManager worldUiManager;
    private ParticleEmitter hubSnowEmitter;
    private SnowstormTransition.Baseline hubSnowBaseline;
    private WhiteFilterTransition departWhiteFilter;
    private PauseMenuController pauseMenu;
    private CraftingHelp craftingHelp;
    private bool craftingHelpVisible = true;
    private DialogueOverlay departureHintOverlay;
    private string currentDepartureHintKey;

    private struct Footprint
    {
        public Vector2 Position;
        public float Rotation;
        public float Age;
        public bool IsOnTrain;
        public const float MaxAge = 4f;
    }

    private readonly List<Footprint> footprints = [];
    private float[] footstepTimers;
    private bool[] footstepSide;

    private int worldWidth, worldHeight;
    private float accumulator;
    private float departHoldTimer;
    private float hubElapsedSeconds;
    private bool isTransitioningToNextLevel;

    private EventInstance ambientMusic;
    private EventInstance leverSound;
    private const float LeverHintDelaySeconds = 5f;
    private const float HintVisibleSeconds = 8f;
    private const float DepartDecisionInputBlockSeconds = 0.2f;
    private const string HintKeyLever = "lever";

    private float departureHintVisibleTimer;
    private bool isDepartDecisionOpen;
    private int? departDecisionPlayerIndex;
    private int? lastReadyPlayerIndex;
    private bool allowDepartWithPendingItems;
    private int previousPendingShopCount;
    private float departDecisionInputBlockTimer;

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
        hubSnowEmitter.Modifiers.Add(new BlizzardGustModifier(() =>
            isTransitioningToNextLevel && departWhiteFilter != null ? departWhiteFilter.Opacity : 0f));
        hubSnowBaseline = SnowstormTransition.Capture(hubSnowEmitter);
        Services.GetService<IVfxService>().AddContinuous(hubSnowEmitter);

        hubOverlay = new HubOverlay();
        hubOverlay.AddToRoot();
        worldUiManager = new WorldUiManager(Game);
        pauseMenu = new PauseMenuController();
        pauseMenu.OnExitRequested += () => Game.SwitchToScreen(new Gamelab.JoinScreen(Game));

        craftingHelp = new CraftingHelp();
        craftingHelp.AddToRoot();
        craftingHelp.IsVisible = craftingHelpVisible;
        departureHintOverlay = new DialogueOverlay();
        currentDepartureHintKey = null;
        hubElapsedSeconds = 0f;
        departureHintVisibleTimer = 0f;
        isDepartDecisionOpen = false;
        departDecisionPlayerIndex = null;
        lastReadyPlayerIndex = null;
        allowDepartWithPendingItems = false;
        previousPendingShopCount = 0;
        departDecisionInputBlockTimer = 0f;

        var soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.AmbientSong);
        soundService.LoadSound(Sounds.SpeedChange);
        ambientMusic = soundService.GetSoundInstance(Sounds.AmbientSong);
        ambientMusic?.Start();
        leverSound = soundService.GetSoundInstance(Sounds.SpeedChange);
        soundService.RegisterParameter(leverSound, "New Speed Setting", () => readyPlayers.Count - 1);
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

        if (IsCraftingHelpToggleRequested())
        {
            craftingHelpVisible = !craftingHelpVisible;
            if (craftingHelp != null) craftingHelp.IsVisible = craftingHelpVisible;
        }

        if (isTransitioningToNextLevel)
        {
            HandleLevelTransition(dt);
            return;
        }

        UpdatePhysics(dt);
        UpdateFootsteps(dt);

        cameraDirector.Update(camera, dt, players, prepTrainMap.GetBounds(), worldWidth, worldHeight,
            allowOffWorldOverflow: true);
        hubSnowEmitter.Position = camera.Position + camera.Origin;
        worldUiManager.Update(prepTrainMap.MapObjects, camera.GetViewMatrix());
        departureHintOverlay?.SyncFollowCamera(camera.GetViewMatrix());
        departureHintOverlay?.Update(gameTime);

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
        DrawFootprints(spriteBatch);
        Services.GetService<IVfxService>().Render(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        foreach (var player in players) player.Draw(spriteBatch);
        spriteBatch.End();
        GumService.Default.Draw();
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        if (departWhiteFilter != null && departWhiteFilter.Opacity > 0.001f)
        {
            spriteBatch.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y),
                Color.White * departWhiteFilter.Opacity);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private bool IsCraftingHelpToggleRequested() =>
        Game.playerManager.Configs.Any(c => c.Input.IsBackButtonJustPressed());

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

        footstepTimers = new float[players.Count];
        footstepSide = new bool[players.Count];
    }

    private void InitializeSpeedLever()
    {
        foreach (IPhysicalEntity entity in prepTrainMap.MapObjects)
        {
            if (entity is SpeedLever lever)
            {
                lever.OnInteractOverride = player =>
                {
                    if (isDepartDecisionOpen)
                        return;

                    int playerIndex = player.PlayerConfiguration.PlayerIndex;
                    bool justBecameReady = !readyPlayers.Remove(playerIndex);
                    if (justBecameReady)
                    {
                        readyPlayers.Add(playerIndex);
                        lastReadyPlayerIndex = playerIndex;
                    }
                    else if (lastReadyPlayerIndex == playerIndex)
                    {
                        lastReadyPlayerIndex = null;
                    }

                    allowDepartWithPendingItems = false;

                    if (justBecameReady)
                        TryOpenDepartDecisionForLastReady(playerIndex);

                    leverSound.Start();
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

            try
            {
                gameplayContext.BeginPhysicsStep();
                gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            }
            finally
            {
                gameplayContext.EndPhysicsStep();
            }

            prepTrainMap.Update(Game.GameplayConfig.FixedTimeStep);

            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }
    }

    private static readonly Vector2 FootprintOrigin = new(12f, 8f);

    private void DrawFootprints(SpriteBatch spriteBatch)
    {
        foreach (var fp in footprints)
        {
            float t = fp.Age / Footprint.MaxAge;
            float alpha = (1f - t * t) * (fp.IsOnTrain ? 0.7f : 0.5f);
            Texture2D tex = fp.IsOnTrain ? AssetManager.FootprintTrainTexture : AssetManager.FootprintSnowTexture;
            spriteBatch.Draw(tex, fp.Position, null, Color.White * alpha,
                fp.Rotation + MathF.PI / 2f, FootprintOrigin, 2f, SpriteEffects.None, RenderUtility.FloorLayer + 0.01f);
        }
    }

    private void UpdateFootsteps(float dt)
    {
        for (int i = footprints.Count - 1; i >= 0; i--)
        {
            var fp = footprints[i];
            fp.Age += dt;
            if (fp.Age >= Footprint.MaxAge)
                footprints.RemoveAt(i);
            else
                footprints[i] = fp;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Vector2 vel = players[i].PhysicsBody.LinearVelocity;
            if (vel.LengthSquared() < 0.05f)
            {
                footstepTimers[i] = 0f;
                continue;
            }

            footstepTimers[i] -= dt;
            if (footstepTimers[i] <= 0f)
            {
                Vector2 dir = Vector2.Normalize(vel);
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                footstepSide[i] = !footstepSide[i];
                Vector2 pos = players[i].Position + perp * (footstepSide[i] ? 6f : -6f);
                bool onTrain = prepTrainMap.GetBounds().Contains((int)pos.X, (int)pos.Y);
                footprints.Add(new Footprint { Position = pos, Rotation = MathF.Atan2(dir.Y, dir.X), Age = 0f, IsOnTrain = onTrain });
                footstepTimers[i] = 0.2f;
            }
        }
    }

    private void UpdateDepartureLogic(float dt)
    {
        hubElapsedSeconds += dt;
        UpdateDepartDecisionInput(dt);

        List<int> joinedPlayerIndices = players
            .Select(p => p.PlayerConfiguration.PlayerIndex)
            .ToList();

        readyPlayers.RemoveWhere(index => !joinedPlayerIndices.Contains(index));

        bool allReady = joinedPlayerIndices.Count > 0 &&
                        joinedPlayerIndices.All(index => readyPlayers.Contains(index));

        int pendingShopCount = CountPendingShopItemsOffBoard();
        if (allowDepartWithPendingItems && pendingShopCount > previousPendingShopCount)
            allowDepartWithPendingItems = false;
        if (!allReady || pendingShopCount == 0)
            allowDepartWithPendingItems = false;

        hubOverlay.Update(readyPlayers, joinedPlayerIndices);
        UpdateDepartureHints(allReady, dt);
        EnsureDepartDecisionShown(allReady, pendingShopCount, joinedPlayerIndices);

        bool blockedByNotAllReady = !allReady;
        bool blockedByPendingShopItems = pendingShopCount > 0 && !allowDepartWithPendingItems;
        bool blockedByDepartDecision = isDepartDecisionOpen;
        bool canDepart = !blockedByNotAllReady && !blockedByPendingShopItems && !blockedByDepartDecision;
        departHoldTimer = canDepart ? departHoldTimer + dt : 0f;

        if (departHoldTimer >= Game.GameplayConfig.DepartHoldSeconds)
        {
            Game.CurrentRun.TrainLayout = prepTrainMap.CaptureLayout();
            departWhiteFilter ??= new WhiteFilterTransition();
            departWhiteFilter.FadeIn(0.8f);
            isTransitioningToNextLevel = true;
        }

        previousPendingShopCount = pendingShopCount;
    }

    private void HandleLevelTransition(float dt)
    {
        worldUiManager.ClearAll();
        departWhiteFilter?.Update(dt);
        float blizzardT = isTransitioningToNextLevel && departWhiteFilter != null ? departWhiteFilter.Opacity : 0f;
        SnowstormTransition.ApplyBlizzardIntensity(hubSnowEmitter, hubSnowBaseline, blizzardT);
        if (departWhiteFilter is { IsDone: true })
        {
            Game.SwitchToScreen(new NextLevelIntroScreen(Game));
        }
    }

    public override void UnloadContent()
    {
        worldUiManager?.ClearAll();
        departureHintOverlay?.Hide();
        departureHintOverlay = null;
        currentDepartureHintKey = null;
        departureHintVisibleTimer = 0f;
        isDepartDecisionOpen = false;
        departDecisionPlayerIndex = null;
        lastReadyPlayerIndex = null;
        allowDepartWithPendingItems = false;
        previousPendingShopCount = 0;
        departDecisionInputBlockTimer = 0f;
        pauseMenu?.Dispose();
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

    private void UpdateDepartureHints(bool allReady, float dt)
    {
        if (isDepartDecisionOpen)
            return;

        string conditionHintKey = null;
        if (!allReady && hubElapsedSeconds >= LeverHintDelaySeconds)
            conditionHintKey = HintKeyLever;

        if (conditionHintKey != currentDepartureHintKey)
        {
            currentDepartureHintKey = conditionHintKey;
            departureHintVisibleTimer = 0f;

            if (departureHintOverlay == null)
                return;

            if (conditionHintKey == null)
            {
                departureHintOverlay.Hide();
                return;
            }

            ShowDepartureHint(conditionHintKey);
            departureHintVisibleTimer = HintVisibleSeconds;
            return;
        }

        if (conditionHintKey == null || departureHintOverlay == null)
            return;

        if (departureHintVisibleTimer <= 0f)
            return;

        departureHintVisibleTimer -= dt;
        if (departureHintVisibleTimer <= 0f)
            departureHintOverlay.Hide();
    }

    private void ShowDepartureHint(string hintKey)
    {
        if (departureHintOverlay == null)
            return;

        string text = hintKey switch
        {
            HintKeyLever => "To depart, each player should interact with the lever.",
            _ => null,
        };
        if (text == null)
            return;

        departureHintOverlay.SetWorldAnchor(null);
        departureHintOverlay.ShowPassive(new DialogueLine("Hint", text));
    }

    private int CountPendingShopItemsOffBoard()
    {
        Rectangle trainBounds = prepTrainMap.GetBounds();
        return prepTrainMap.MapObjects.Count(entity =>
            entity is AbstractStation && !trainBounds.Contains(entity.Position));
    }

    private void TryOpenDepartDecisionForLastReady(int playerIndex)
    {
        List<int> joinedPlayerIndices = players
            .Select(p => p.PlayerConfiguration.PlayerIndex)
            .ToList();

        bool allReady = joinedPlayerIndices.Count > 0 &&
                        joinedPlayerIndices.All(index => readyPlayers.Contains(index));
        if (!allReady)
            return;

        int pendingShopCount = CountPendingShopItemsOffBoard();
        if (pendingShopCount <= 0)
            return;

        OpenDepartDecision(playerIndex);
    }

    private void UpdateDepartDecisionInput(float dt)
    {
        if (!isDepartDecisionOpen)
            return;

        if (departDecisionInputBlockTimer > 0f)
        {
            departDecisionInputBlockTimer -= dt;
            return;
        }

        bool acceptDepart = Game.playerManager.Configs.Any(config => config.Input.IsInteractJustPressed());
        bool setNotReady = Game.playerManager.Configs.Any(config => config.Input.IsGrabJustPressed());
        if (!acceptDepart && !setNotReady)
            return;

        if (setNotReady && departDecisionPlayerIndex.HasValue)
        {
            readyPlayers.Remove(departDecisionPlayerIndex.Value);
            allowDepartWithPendingItems = false;
        }
        else if (acceptDepart)
        {
            allowDepartWithPendingItems = true;
        }

        isDepartDecisionOpen = false;
        departDecisionPlayerIndex = null;
        departureHintOverlay?.Hide();
    }

    private void EnsureDepartDecisionShown(bool allReady, int pendingShopCount, IReadOnlyList<int> joinedPlayerIndices)
    {
        if (isDepartDecisionOpen || allowDepartWithPendingItems)
            return;
        if (!allReady || pendingShopCount <= 0)
            return;

        int fallbackPlayer = lastReadyPlayerIndex
                             ?? (joinedPlayerIndices.Count > 0 ? joinedPlayerIndices[joinedPlayerIndices.Count - 1] : -1);
        if (fallbackPlayer >= 0)
            OpenDepartDecision(fallbackPlayer);
    }

    private void OpenDepartDecision(int playerIndex)
    {
        if (departureHintOverlay == null)
            return;

        isDepartDecisionOpen = true;
        departDecisionPlayerIndex = playerIndex;
        currentDepartureHintKey = null;
        departureHintVisibleTimer = 0f;
        departDecisionInputBlockTimer = DepartDecisionInputBlockSeconds;
        departureHintOverlay.SetWorldAnchor(null);
        departureHintOverlay.ShowDecision(
            new DialogueLine("Hint", "Bought items are still off-board. Depart anyway?"),
            "No",
            "Yes");
    }
}