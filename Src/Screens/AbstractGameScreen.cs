using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace Gamelab.Screens;

public abstract class AbstractGameScreen(GamelabGame game) : GameScreen(game)
{
    protected ViewportAdapter viewportAdapter;
    protected SpriteBatch spriteBatch;
    protected Point virtualScreenSize = new(1920, 1080);
    protected KeyboardState previousKeyboardState;
    public int NumFramesDrawn { get; private set; }

    public new GamelabGame Game => (GamelabGame)base.Game;

    public override void Initialize()
    {
        base.Initialize();

        // Simplify rendering on different screen sizes by using a virtual resolution, scaling it to fit the actual window size, and applying letterboxing if needed to maintain the aspect ratio
        viewportAdapter =
            new BoxingViewportAdapter(Game.Window, GraphicsDevice, virtualScreenSize.X, virtualScreenSize.Y);
        viewportAdapter.Reset();

        spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    public override void Update(GameTime gameTime)
    {
        foreach (PlayerConfiguration config in Game.playerManager.Configs)
        {
            config.Input.Update(gameTime);
        }

        var keyboard = Keyboard.GetState();
        bool toggleDebugOverlayRequested = keyboard.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3);
        if (toggleDebugOverlayRequested)
        {
            Game.ToggleDebugOverlay();
        }

        var gamePads = new Dictionary<int, GamePadState>();
        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            var playerIndex = (PlayerIndex)i;
            var capabilities = GamePad.GetCapabilities(playerIndex);
            if (!capabilities.IsConnected)
            {
                continue;
            }

            var gamePad = GamePad.GetState(playerIndex);
            gamePads.Add(i, gamePad);
        }

        Update(gameTime, keyboard, gamePads);
        previousKeyboardState = keyboard;
    }

    protected virtual void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
    }

    public override void Draw(GameTime gameTime)
    {
        NumFramesDrawn++;
    }

    public class Factory
    {
        public string name;
        private readonly Func<GamelabGame, AbstractGameScreen> factory;

        public Factory(string name, Func<GamelabGame, AbstractGameScreen> factory)
        {
            this.name = name;
            this.factory = factory;
        }

        public AbstractGameScreen Instantiate(GamelabGame game) => factory(game);

        public override string ToString() => name;
    }

    /// <summary>
    /// Returns a list of factory functions that create instances of all screens in the game.
    /// The instances created by these factories will be tested in the CICD pipeline.
    /// If you have parameterized screens or have to exclude unused screens, modify this logic.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Factory> GetScreenFactories()
    {
        var screenTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(AbstractGameScreen)) && !t.IsAbstract)
            .ToList();

        return screenTypes.Select(t =>
            new Factory(t.Name, game => { return (AbstractGameScreen)Activator.CreateInstance(t, game); }));
    }
}