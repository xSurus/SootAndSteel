using System;
using System.IO;
using FmodForFoxes;
using FmodForFoxes.Studio;
using FontStashSharp;
using Gamelab.Assets;
using Gamelab.Config;
using Gamelab.Items;
using Gamelab.Items.Bullets;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.Players;
using Gamelab.Screens;
using Gamelab.Serialization;
using Gamelab.Services.Animation;
using Gamelab.Services.Bullet;
using Gamelab.Services.Random;
using Gamelab.Services.Shop;
using Gamelab.Services.Sound;
using Gamelab.Services.Vfx;
using Gamelab.Systems;
using Gamelab.Utils;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGameGum;
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
    public StationRegistry StationRegistry { get; private set; } = new();
    public ComponentRegistry ComponentRegistry { get; private set; } = new();
    public RunSession CurrentRun { get; set; }

    public RunMode runMode { get; private set; }
    public bool IsDebug => runMode == RunMode.Debug;
    public bool IsRelease => runMode == RunMode.Release;
    public bool IsDebugOverlayEnabled { get; private set; }

    /// <summary>
    /// Uniform scale applied to Gum so 1920×1080 fits the window (updated every frame in viewport fit).
    /// Layout and bitmap fonts scale with this via <c>Camera.Zoom</c>; use this only if you adjust
    /// <see cref="MonoGameGum.GueDeriving.TextRuntime.FontSize"/> manually and need the same factor.
    /// </summary>
    public float GumViewportScale { get; private set; } = 1f;

    private GamelabGameScreen nextScreen;
    private string screenshotPath;

    public bool IsRunning => screenManager.ActiveScreen != null;

    public readonly INativeFmodLibrary nativeFmodLibrary;
    public EventInstance menuStabInstance;

    public GamelabGame(RunMode runMode)
    {
        Instance = this;

        this.runMode = runMode;
        this.nativeFmodLibrary = new DesktopAndMacNativeFmodLibrary();

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
        VfxService vfxService = new VfxService();
        Services.AddService((ISoundService)soundService);
        Services.AddService<IVfxService>(vfxService);
        systemManager.Add(soundService);
        systemManager.Add(vfxService);

        IGameSystem bulletService = new BulletService();
        Services.AddService((IBulletService)bulletService);
        systemManager.Add(bulletService);

        IGameSystem animationService = new AnimationService();
        Services.AddService((IAnimationService)animationService);
        systemManager.Add(animationService);

        IRandomService randomService = new RandomService();
        Services.AddService(randomService);
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

        InitializeGum();

        Window.ClientSizeChanged += OnWindowClientSizeChanged;

        var fontPath = Path.Combine(contentDir, "promptfont.ttf");
        var fontBytes = File.ReadAllBytes(fontPath);
        fontSystem.AddFont(fontBytes);
        LoadConfigs();
        Services.AddService(typeof(IShopService), new ShopManager());
        WorldUtility.Initialize();
        ItemRegistry.Initialize();
        systemManager.InitializeAll(this);
        AssetManager.LoadContent(Content, graphics.GraphicsDevice);
        CurrentRun = new RunSession();
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
        ApplyGumViewportFit();
        GumService.Default.Update(gameTime);
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
    public void SwitchToScreen(GamelabGameScreen screen)
    {
        nextScreen = screen;
    }

    protected void SwitchToScreenImmediately(GamelabGameScreen screen)
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

    public void LoadConfigs()
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

        try
        {
            StationRegistry.Load(jsonLoader);
            logger.Info("Loaded station configs from Data/StationConfig.json");
        }
        catch (Exception ex)
        {
            logger.Exception("Failed to load Data/StationConfig.json", ex);
        }

        try
        {
            ComponentRegistry.Load(jsonLoader);
            logger.Info("Loaded config from Data/ComponentConfig.json");
        }
        catch (Exception ex)
        {
            logger.Exception("Failed to load Data/ComponentConfig.json", ex);
        }
    }

    /// <summary>Gum UI is authored at this resolution (same as <see cref="Screens.GamelabGameScreen"/> virtual size).</summary>
    private const float GumDesignWidth = 1920f;

    private const float GumDesignHeight = 1080f;

    private void InitializeGum()
    {
        var gumProjectPath = Path.Combine(contentDir, "GumProject", "SootAndSteelGum.gumx");
        GumService.Default.Initialize(this, gumProjectPath);
#pragma warning disable CS0618 // Gum marks LoadAnimations experimental; required for .ganx playback
        GumService.Default.LoadAnimations();
#pragma warning restore CS0618
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
        ApplyGumViewportFit();
    }

    private void ApplyGumViewportFit()
    {
        var gum = GumService.Default;
        if (!gum.IsInitialized) return;

        var vp = GraphicsDevice.Viewport;
        if (vp.Width <= 0 || vp.Height <= 0) return;

        gum.CanvasWidth = GumDesignWidth;
        gum.CanvasHeight = GumDesignHeight;

        float scale = Math.Min(vp.Width / GumDesignWidth, vp.Height / GumDesignHeight);
        GumViewportScale = scale;
        gum.Renderer.Camera.Zoom = scale;
    }

    private void OnWindowClientSizeChanged(object sender, EventArgs e)
    {
        ApplyGumViewportFit();
        GumService.Default.Root.UpdateLayout();
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