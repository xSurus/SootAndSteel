using System;
using System.Collections.Generic;
using Gamelab.Enemies;
using Gamelab.Input;
using Gamelab.Levels;
using Gamelab.Map;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.Particles;
using Gamelab.PhysicalEntities.Projectiles;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Stations.Workbenches;
using Gamelab.Players;
using Gamelab.Services.Music;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class GameplayScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private List<Player> players;
    private TrainMap trainMap;
    private WorldScroller worldScroller;
    private GameplayContext gameplayContext;
    private TrainSound trainSound;
    private EnemyManager enemyManager;
    private LevelManager levelManager;
    private SoundHandle menuSelectSound;
    private LevelDefinition currentLevelDef;
    private Desktop desktop;
    private Label coalLabel;
    private Label speedLabel;
    private Label temperatureLabel;
    private Label cannonLabel;
    private Label distanceLabel;
    private ProjectileManager projectileManager;
    private Panel pauseOverlay;
    private Label continueLabel;
    private Label exitLabel;

    private float screenShakeTimer;
    private float screenShakeIntensity;
    private Vector2 screenShakeOffset;
    private readonly Random random = new Random();
    private bool isPaused;
    private int pauseSelectionIndex;
    private bool wasEscapeDown;
    private bool isFailureTriggered;

    public override void LoadContent()
    {
        base.LoadContent();
        gameplayContext = new GameplayContext(virtualScreenSize);
        Services.AddService(gameplayContext);
        currentLevelDef = LevelLoader.Load(Game.CurrentLevel);
        trainMap = new TrainMap(GraphicsDevice);
        gameplayContext.Map = trainMap;
        menuSelectSound = Services.GetService<ISoundService>().RegisterSound("menu_stab", 4);
        trainSound = new TrainSound(Services.GetService<ISoundService>());
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm(random));

        trainMap.MapObjects.Add(new CoalResource(trainMap.GetTileCenterPixels(0, 2)));
        trainMap.MapObjects.Add(new CoalOven(trainMap.GetTileCenterPixels(7, 2)));
        trainMap.MapObjects.Add(new SpeedLever(trainMap.GetTileCenterPixels(7, 3)));
        trainMap.MapObjects.Add(new CannonStation(trainMap.GetTileCenterPixels(5, 2)));

        // Two left-side work zones: top-left and bottom-left, each with an anvil.
        trainMap.MapObjects.Add(new CopperResource(trainMap.GetTileCenterPixels(2, 0)));
        trainMap.MapObjects.Add(new GunpowderResource(trainMap.GetTileCenterPixels(2, 4)));
        trainMap.MapObjects.Add(new Anvil(trainMap.GetTileCenterPixels(1, 0))); // top-left crafting
        trainMap.MapObjects.Add(new Anvil(trainMap.GetTileCenterPixels(1, 4))); // bottom-left crafting
        trainMap.MapObjects.Add(new Counter(trainMap.GetTileCenterPixels(0, 0)));
        trainMap.MapObjects.Add(new Counter(trainMap.GetTileCenterPixels(3, 0)));
        trainMap.MapObjects.Add(new Counter(trainMap.GetTileCenterPixels(0, 4)));
        trainMap.MapObjects.Add(new Counter(trainMap.GetTileCenterPixels(3, 4)));

        worldScroller = new WorldScroller(GraphicsDevice);
        projectileManager = new ProjectileManager();
        enemyManager = new EnemyManager(random, currentLevelDef, projectileManager);
        levelManager = new LevelManager(currentLevelDef);

        gameplayContext.Events.OnWallBreached += OnWallBreached;
        gameplayContext.Events.OnWallRepaired += OnWallRepaired;
        gameplayContext.State.OnTrainFrozen += OnTrainFrozen;

        players = [];
        foreach (var playerConfig in Game.playerManager.Configs)
        {
            players.Add(new Player(trainMap.GetTileCenterPixels(playerConfig.PlayerIndex, 1), playerConfig));
        }

        // Initialize Myra UI
        desktop = new Desktop();
        var mainPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        coalLabel = new Label
        {
            Text = "Coal: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20)
        };

        speedLabel = new Label
        {
            Text = "Speed: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 80, 20, 20)
        };

        temperatureLabel = new Label
        {
            Text = "Temperature: 100",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 140, 20, 20)
        };

        cannonLabel = new Label
        {
            Text = "Cannon: Empty",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 200, 20, 20)
        };

        distanceLabel = new Label
        {
            Text = "Distance: 0",
            Font = Game.fontSystem.GetFont(48),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 260, 20, 20)
        };

        mainPanel.Widgets.Add(coalLabel);
        mainPanel.Widgets.Add(speedLabel);
        mainPanel.Widgets.Add(temperatureLabel);
        mainPanel.Widgets.Add(cannonLabel);
        mainPanel.Widgets.Add(distanceLabel);
        pauseOverlay = CreatePauseOverlay();
        mainPanel.Widgets.Add(pauseOverlay);
        desktop.Root = mainPanel;

        Services.GetService<IMusicService>()
            .FadeOutAndPlay("tmp_ambient", 2, repeating: true, volume: Game.MusicVolume);
    }

    private Panel CreatePauseOverlay()
    {
        var overlay = new Panel
        {
            Background = new SolidBrush(new Color(0, 0, 0, 180)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visible = false
        };

        var modal = new Panel
        {
            Width = 420,
            Height = 280,
            Background = new SolidBrush(new Color(25, 25, 30, 230)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var stack = new VerticalStackPanel
        {
            Spacing = 24,
            Padding = new Thickness(40),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        stack.Widgets.Add(new Label
        {
            Text = "PAUSED",
            Font = Game.fontSystem.GetFont(64),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.White
        });

        continueLabel = new Label
        {
            Text = "CONTINUE",
            Font = Game.fontSystem.GetFont(40),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        exitLabel = new Label
        {
            Text = "EXIT",
            Font = Game.fontSystem.GetFont(40),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stack.Widgets.Add(continueLabel);
        stack.Widgets.Add(exitLabel);
        modal.Widgets.Add(stack);
        overlay.Widgets.Add(modal);

        UpdatePauseSelectionVisuals();
        return overlay;
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

        if (IsPauseToggleRequested())
        {
            TogglePause();
        }

        if (isPaused)
        {
            UpdatePauseMenu();
            return;
        }

        worldScroller.Update(dt);
        enemyManager.Update(dt);
        projectileManager.Update(dt);

        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        while (accumulator >= Game.GameplayConfig.FixedTimeStep)
        {
            foreach (Player player in players)
            {
                player.Update(Game.GameplayConfig.FixedTimeStep);
            }

            gameplayContext.State.Update(Game.GameplayConfig.FixedTimeStep);
            if (isFailureTriggered) return;

            // Failure: ran out of coal while still trying to move.
            if (gameplayContext.State.CoalAmount <= 0 && gameplayContext.State.actualSpeed > 0f)
            {
                TriggerFailure();
                return;
            }

            trainMap.Update(Game.GameplayConfig.FixedTimeStep);
            gameplayContext.PhysicsWorld.Step(Game.GameplayConfig.FixedTimeStep);
            accumulator -= Game.GameplayConfig.FixedTimeStep;
        }

        trainSound.Update(gameTime);

        coalLabel.Text = $"Coal: {gameplayContext.State.CoalAmount}";
        speedLabel.Text = $"Speed: {gameplayContext.State.actualSpeed:F0}";
        temperatureLabel.Text = $"Temperature: {gameplayContext.State.Temperature:F0}";
        distanceLabel.Text =
            $"Distance: {gameplayContext.State.DistanceTraveled:F0} / {currentLevelDef?.LevelDistance ?? 0:F0}";

        if (currentLevelDef != null &&
            gameplayContext.State.DistanceTraveled >= currentLevelDef.LevelDistance &&
            enemyManager.Enemies.Count == 0)
        {
            Game.CurrentLevel++;
            Game.SwitchToScreen(new MainMenuScreen(Game));
        }

        UpdateScreenShake(dt);
    }

    private bool IsPauseToggleRequested()
    {
        bool isEscapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);
        bool escapePressed = isEscapeDown && !wasEscapeDown;
        wasEscapeDown = isEscapeDown;
        bool startPressed = false;

        foreach (PlayerConfiguration player in Game.playerManager.Configs)
        {
            if (player.Input is GamePadInputProvider && player.Input.IsStartJustPressed())
            {
                startPressed = true;
                break;
            }
        }

        return escapePressed || startPressed;
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseSelectionIndex = 0;
        pauseOverlay.Visible = isPaused;
        UpdatePauseSelectionVisuals();
    }

    private void UpdatePauseMenu()
    {
        bool moveUp = false;
        bool moveDown = false;
        bool confirm = false;

        foreach (PlayerConfiguration player in Game.playerManager.Configs)
        {
            moveUp |= player.Input.IsUpJustPressed();
            moveDown |= player.Input.IsDownJustPressed();
            confirm |= player.Input.IsPickupJustPressed();
        }

        if (moveUp || moveDown)
        {
            pauseSelectionIndex = 1 - pauseSelectionIndex;
            UpdatePauseSelectionVisuals();
            menuSelectSound?.Play();
        }

        if (!confirm)
        {
            return;
        }

        menuSelectSound?.Play();

        if (pauseSelectionIndex == 0)
        {
            TogglePause();
            return;
        }

        Game.SwitchToScreen(new JoinScreen(Game));
    }

    private void UpdatePauseSelectionVisuals()
    {
        if (continueLabel == null || exitLabel == null)
        {
            return;
        }

        continueLabel.TextColor = pauseSelectionIndex == 0 ? Color.LightBlue : Color.White;
        exitLabel.TextColor = pauseSelectionIndex == 1 ? Color.LightBlue : Color.White;
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

    public override void Draw(GameTime gameTime)
    {
        Matrix shakeMatrix = Matrix.CreateTranslation(screenShakeOffset.X, screenShakeOffset.Y, 0);
        Matrix finalTransform = shakeMatrix * viewportAdapter.GetScaleMatrix();

        spriteBatch.Begin(transformMatrix: finalTransform);
        worldScroller.Draw(spriteBatch);
        trainMap.Draw(spriteBatch);
        enemyManager.Draw(spriteBatch);
        projectileManager.Draw(spriteBatch);

        foreach (Player player in players)
        {
            player.Draw(spriteBatch);
        }

        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop.Render();
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

        if (gameplayContext?.State != null)
        {
            gameplayContext.State.OnTrainFrozen -= OnTrainFrozen;
        }

        Game.Services.RemoveService(typeof(GameplayContext));
        Services.GetService<IVfxService>().ClearAll();
        trainMap?.Dispose();
        worldScroller?.Dispose();
        projectileManager?.Clear();
        base.UnloadContent();
    }
}