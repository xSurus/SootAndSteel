using System;
using System.Collections.Generic;
using System.Linq;
using FontStashSharp;
using Gamelab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class JoinScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private Desktop desktop;
    private Label instructionLabel;
    private readonly List<Panel> playerPanels = [];
    private readonly List<Label> playerStatusLabels = [];

    private readonly Dictionary<int, GamePadState> previousGamePadStates = new();

    public override void Initialize()
    {
        base.Initialize();
        Game.playerManager.Reset();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        desktop = new Desktop();

        int screenWidth = virtualScreenSize.X;
        int screenHeight = virtualScreenSize.Y;

        float scaleX = screenWidth / 1920f;
        float scaleY = screenHeight / 1080f;
        float minScale = Math.Min(scaleX, scaleY);

        VerticalStackPanel mainStack = new VerticalStackPanel
        {
            Spacing = (int)(50 * scaleY),
            Padding = new Thickness(0, (int)(100 * scaleY), 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        Label titleLabel = new Label
        {
            Text = "Join Game",
            HorizontalAlignment = HorizontalAlignment.Center,
            Font = Game.fontSystem.GetFont((int)(96 * minScale))
        };
        mainStack.Widgets.Add(titleLabel);

        instructionLabel = new Label
        {
            Text = "Press (A) to join.",
            HorizontalAlignment = HorizontalAlignment.Center,
            Font = Game.fontSystem.GetFont((int)(48 * minScale))
        };
        mainStack.Widgets.Add(instructionLabel);

        Grid panelsContainer = new Grid
        {
            ColumnSpacing = (int)(20 * scaleX),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness((int)(50 * scaleX), 0, (int)(50 * scaleX), 0)
        };

        DynamicSpriteFont playerLabelFont = Game.fontSystem.GetFont((int)(36 * minScale));
        DynamicSpriteFont statusLabelFont = Game.fontSystem.GetFont((int)(28 * minScale));

        for (int i = 0; i < 4; i++)
        {
            panelsContainer.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1.0f));
            var panel = new Panel
                { Height = (int)(400 * scaleY), Background = new SolidBrush(new Color(Color.White, 0.5f)) };
            Grid.SetColumn(panel, i);
            var content = new VerticalStackPanel
            {
                Spacing = (int)(100 * scaleY), Padding = new Thickness(0, (int)(20 * scaleY), 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            content.Widgets.Add(new Label
            {
                Text = $"Player {i + 1}",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.Black,
                Font = playerLabelFont
            });
            var statusLabel = new Label
            {
                Text = "Press (A) to Join",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.Black,
                VerticalAlignment = VerticalAlignment.Center,
                Font = statusLabelFont
            };
            content.Widgets.Add(statusLabel);
            panel.Widgets.Add(content);
            panelsContainer.Widgets.Add(panel);
            playerPanels.Add(panel);
            playerStatusLabels.Add(statusLabel);
        }

        mainStack.Widgets.Add(panelsContainer);
        desktop.Root = mainStack;
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            if (!gamePads.TryGetValue(i, out GamePadState currentGamePadState)) continue;
            previousGamePadStates.TryGetValue(i, out GamePadState previousGamePadState);

            if (currentGamePadState.Buttons.A == ButtonState.Pressed &&
                previousGamePadState.Buttons.A == ButtonState.Released)
            {
                if (!Game.playerManager.IsControllerJoined(i))
                {
                    Game.playerManager.JoinPlayer(new GamePadInputProvider(i));
                }
            }
        }

        if (keyboard.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
        {
            bool keyboardExists = Game.playerManager.Configs.Any(c => c.Input is KeyboardInputProvider);
            if (!keyboardExists)
            {
                Game.playerManager.JoinPlayer(new KeyboardInputProvider(Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space));
            }
        }

        bool anyJoined = Game.playerManager.Configs.Any();
        instructionLabel.Text = anyJoined
            ? "Press (A)/[Space] to join. Press (Start)/[Enter] to begin."
            : "Press (A) or [Space] to join.";

        for (int i = 0; i < 4; i++)
        {
            bool isJoined = Game.playerManager.Configs.Count > i;
            playerPanels[i].Background = new SolidBrush(isJoined ? Color.LightGreen : new Color(Color.White, 0.5f));
            playerStatusLabels[i].Text = isJoined ? "Joined!" : "Ready...";
        }

        if (anyJoined)
        {
            bool startPressed = Game.playerManager.Configs.Any(c => c.Input.IsStartJustPressed());
            if (startPressed)
            {
                Game.SwitchToScreen(new MainMenuScreen(Game));
                return;
            }
        }

        previousKeyboardState = keyboard;
        previousGamePadStates.Clear();
        foreach ((int i, GamePadState state) in gamePads) previousGamePadStates[i] = state;
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        desktop.Render();
        spriteBatch.End();
        base.Draw(gameTime);
    }
}