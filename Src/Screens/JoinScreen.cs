using System.Collections.Generic;
using System.Linq;
using Gamelab.Components;
using Gamelab.Input;
using Gamelab.UI;
using Gum.Forms;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;

namespace Gamelab;

/// <summary>
/// Multiplayer join MonoGame screen. Gum Forms UI is <see cref="Screens.JoinScreen"/> (generated); this type
/// stays in namespace <see cref="Gamelab"/> so it does not collide with Gum when you regenerate code.
/// </summary>
public sealed class JoinScreen(GamelabGame game) : Screens.GamelabGameScreen(game)
{
    private readonly Dictionary<int, GamePadState> previousGamePadStates = new();

    /// <summary>Last frame's join occupancy per slot — only then we swap animations.</summary>
    private readonly bool[] previousSlotJoined = new bool[4];

    private Screens.JoinScreen joinUi;

    private static GumService Gum => GumService.Default;

    public override void Initialize()
    {
        base.Initialize();
        Gum.Root.Children.Clear();
        Game.playerManager.Reset();
        for (int i = 0; i < previousSlotJoined.Length; i++)
            previousSlotJoined[i] = false;
    }

    public override void LoadContent()
    {
        base.LoadContent();
        joinUi = new Screens.JoinScreen();
        joinUi.AddToRoot();

        JoinPlayerComponent[] players =
        [
            joinUi.First_Player, joinUi.Second_Player, joinUi.Third_Player, joinUi.Fourth_Player
        ];

        for (int i = 0; i < players.Length; i++)
        {
            SetPlayerFigureState(players[i], isJoined: false, slotNumber: i + 1);
            SetJoinButtonState(players[i], isJoined: false);
        }
    }

    public override void UnloadContent()
    {
        Gum.Root.Children.Clear();
        joinUi = null;
        base.UnloadContent();
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
                Game.playerManager.JoinPlayer(new KeyboardInputProvider(Keys.W, Keys.S, Keys.A, Keys.D, Keys.E,
                    Keys.R, Keys.Space));
            }
        }

        bool anyJoined = Game.playerManager.Configs.Any();
        joinUi.JoinScreenTitleText = anyJoined
            ? "Press start to advance"
            : "Enter the train";

        var players = new[]
        {
            joinUi.First_Player, joinUi.Second_Player, joinUi.Third_Player, joinUi.Fourth_Player
        };
        for (int i = 0; i < 4; i++)
        {
            bool isJoined = Game.playerManager.Configs.Count > i;
            if (isJoined == previousSlotJoined[i])
                continue;

            previousSlotJoined[i] = isJoined;
            JoinPlayerComponent p = players[i];

            if (isJoined)
            {
                SetPlayerFigureState(p, isJoined: true, slotNumber: i + 1);
                SetJoinButtonState(p, isJoined: true);
            }
            else
            {
                SetPlayerFigureState(p, isJoined: false, slotNumber: i + 1);
                SetJoinButtonState(p, isJoined: false);
            }
        }

        if (anyJoined)
        {
            bool startPressed = Game.playerManager.Configs.Any(c => c.Input.IsStartJustPressed());
            if (startPressed)
            {
                Game.SwitchToScreen(new global::Gamelab.MainMenuScreen(Game));
                return;
            }
        }

        previousKeyboardState = keyboard;
        previousGamePadStates.Clear();
        foreach ((int i, GamePadState state) in gamePads) previousGamePadStates[i] = state;
    }

    public override void Draw(GameTime gameTime)
    {
        Gum.Draw();
        base.Draw(gameTime);
    }

    private static void SetJoinButtonState(JoinPlayerComponent playerSlot, bool isJoined)
    {
        if (playerSlot.Join is not { } joinButton)
        {
            return;
        }

        if (isJoined)
        {
            XboxButtonGlyphs.ApplyFaceButton(joinButton, XboxButtonAtlas.Face.Start);
            joinButton.ButtonText = "Joined";
        }
        else
        {
            XboxButtonGlyphs.ApplyFaceButton(joinButton, XboxButtonAtlas.Face.A);
            joinButton.ButtonText = "Join";
        }
    }

    private static void SetPlayerFigureState(JoinPlayerComponent playerSlot, bool isJoined, int slotNumber)
    {
        if (!isJoined)
        {
            playerSlot.JoinedState = JoinPlayerComponent.Joined.empty;
            return;
        }

        playerSlot.JoinedState = slotNumber switch
        {
            1 => JoinPlayerComponent.Joined.joined1,
            2 => JoinPlayerComponent.Joined.joined2,
            3 => JoinPlayerComponent.Joined.joined3,
            4 => JoinPlayerComponent.Joined.joined4,
            _ => JoinPlayerComponent.Joined.empty
        };
    }
}
