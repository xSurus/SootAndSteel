using System;
using System.IO;
using FontStashSharp;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Screens;
using Gamelab.Services.Music;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Systems;
using Gamelab.Utils;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Screens;
using Myra;

namespace Gamelab;

public class GamelabGame : Game
{
    public enum RunMode
    {
        Release,
        Debug,
    }

    public static GamelabGame Instance { get; private set; }

    private readonly Logger logger = new("Game");
    public readonly GraphicsDeviceManager graphics;
    public readonly ScreenManager screenManager;
    public readonly SystemManager systemManager;
    public readonly DebugOverlaySystem debugOverlaySystem;
    public readonly FontSystem fontSystem = new();
    public readonly PlayerManager playerManager = new();

    /// <summary>
    /// The directory where the compiled content files are available during runtime
    /// </summary>
    public readonly string contentDir;

    /// <summary>
    /// ONLY FOR DEBUG: The directory where the uncompiled content files are available.
    /// </summary>
    public readonly string uncompiledContentDir;

    public readonly JsonLoader jsonLoader;
    public GameplayConfig GameplayConfig { get; private set; } = new();
    public int CurrentLevel { get; set; } = 1;

    public RunMode runMode { get; private set; }
    public bool IsDebug => runMode == RunMode.Debug;
    public bool IsRelease => runMode == RunMode.Release;
    public bool IsDebugOverlayEnabled { get; private set; } = true;

    public float MusicVolume { get; private set; } = 0.3f;
    private AbstractGameScreen nextScreen;
    private string screenshotPath;

    public bool IsRunning => screenManager.ActiveScreen != null;

    public GamelabGame(RunMode runMode)
    {
        Instance = this;

        this.runMode = runMode;

        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        contentDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Content.RootDirectory);
        uncompiledContentDir = IsDebug
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", Content.RootDirectory)
            : null;

        jsonLoader = new(this, "Data");
        systemManager = new SystemManager();
        debugOverlaySystem = new DebugOverlaySystem();
        systemManager.Add(debugOverlaySystem);

        screenManager = new ScreenManager();
        Components.Add(screenManager);

        IGameSystem soundService = new SoundService();
        IGameSystem musicService = new MusicService();
        VfxService vfxService = new VfxService();
        Services.AddService((ISoundService)soundService);
        Services.AddService((IMusicService)musicService);
        Services.AddService<IVfxService>(vfxService);
        systemManager.Add(soundService);
        systemManager.Add(musicService);
        systemManager.Add(vfxService);
    }

    protected override void Initialize()
    {
        MyraEnvironment.Game = this;
        base.Initialize();

        //TODO: Replace with actual game name
        Window.Title = "Gamelab Game";
        IsMouseVisible = true;

        if (IsDebug)
        {
            // For debugging, show a smaller window that is resizable
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferWidth = 854;
            graphics.PreferredBackBufferHeight = 480;
        }
        else
        {
            // For release, render in fullscreen
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            graphics.PreferredBackBufferWidth = displayMode.Width;
            graphics.PreferredBackBufferHeight = displayMode.Height;
            graphics.IsFullScreen = true;
        }

        graphics.ApplyChanges();

        var fontPath = Path.Combine(contentDir, "promptfont.ttf");
        var fontBytes = File.ReadAllBytes(fontPath);
        fontSystem.AddFont(fontBytes);
        LoadGameplayConfig();
        PhysicsUtility.Initialize(GameplayConfig.PixelsPerMeter);
        ItemRegistry.Initialize();
        systemManager.InitializeAll(this);
        AssetManager.LoadContent(graphics.GraphicsDevice);
        screenManager.ShowScreen(new JoinScreen(this));
        logger.Info("Game initialized");
    }

    protected override void Update(GameTime gameTime)
    {
        if (nextScreen != null)
        {
            SwitchToScreenImmediately(nextScreen);
            nextScreen = null;
        }

        systemManager.UpdateAll(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
        systemManager.DrawAll();

        if (!string.IsNullOrEmpty(screenshotPath))
        {
            SaveScreenshotImmediately(screenshotPath);
            screenshotPath = null;
        }
    }

    /// <summary>
    /// Switch to the specified screen on the next update. Not transitioning immediately allows the current screen to finish its update and draw cycle, which can help avoid issues with switching screens in the middle of their logic.
    /// </summary>
    /// <param name="screen">The screen to switch to.</param>
    public void SwitchToScreen(AbstractGameScreen screen)
    {
        nextScreen = screen;
    }

    protected void SwitchToScreenImmediately(AbstractGameScreen screen)
    {
        screenManager.ReplaceScreen(screen);
    }

    /// <summary>
    /// Save a screenshot at the end of the next draw cycle.
    /// </summary>
    /// <param name="path">The path where the screenshot will be saved.</param>
    public void SaveScreenshot(string path)
    {
        screenshotPath = path;
    }

    protected void SaveScreenshotImmediately(string path)
    {
        var pp = GraphicsDevice.PresentationParameters;
        var width = pp.BackBufferWidth;
        var height = pp.BackBufferHeight;

        var data = new byte[width * height * 4];
        GraphicsDevice.GetBackBufferData(data);

        var texture = new Texture2D(GraphicsDevice, width, height, false, pp.BackBufferFormat);
        texture.SetData(data);

        var parentDirPath = Path.GetDirectoryName(path);
        Directory.CreateDirectory(parentDirPath);

        using (var stream = File.Create(path))
        {
            texture.SaveAsPng(stream, texture.Width, texture.Height);
        }

        texture.Dispose();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Math.Clamp(volume, 0f, 1f);
        MediaPlayer.Volume = MusicVolume;
    }

    public void LoadGameplayConfig()
    {
        try
        {
            GameplayConfig = jsonLoader.LoadJson<GameplayConfig>("gameplay.json") ?? new GameplayConfig();
            TrainSpeedSetting.Initialize(GameplayConfig);
            logger.Info("Loaded gameplay config from Data/gameplay.json");
        }
        catch (Exception ex)
        {
            GameplayConfig = new GameplayConfig();
            logger.Warning("Failed to load Data/gameplay.json, using defaults.");
            logger.Exception("Gameplay config load error", ex);
        }
    }

    public void ToggleDebugOverlay()
    {
        IsDebugOverlayEnabled = !IsDebugOverlayEnabled;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            systemManager.ShutdownAll();
        }

        AssetManager.UnloadContent();
        base.Dispose(disposing);
    }
}