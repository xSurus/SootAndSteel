using System;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.UI;

public class PauseMenuController
{
    private readonly ISoundService soundService = GamelabGame.Instance.Services.GetService<ISoundService>();

    private readonly Panel overlay;
    private readonly Label continueLabel;
    private readonly Label exitLabel;
    private int selectionIndex;
    private bool wasEscapeDown;

    public bool IsPaused { get; private set; }
    public Panel Overlay => overlay;

    public event Action OnExitRequested;

    public PauseMenuController()
    {
        overlay = BuildOverlay(out continueLabel, out exitLabel);
        UpdateSelectionVisuals();
    }

    public bool IsToggleRequested(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        bool isEscapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);
        bool escapePressed = isEscapeDown && !wasEscapeDown;
        wasEscapeDown = isEscapeDown;

        bool startPressed = false;
        foreach (PlayerConfiguration player in playerConfigs)
        {
            if (player.Input is GamePadInputProvider && player.Input.IsStartJustPressed())
            {
                startPressed = true;
                break;
            }
        }

        return escapePressed || startPressed;
    }

    public void Toggle()
    {
        IsPaused = !IsPaused;
        selectionIndex = 0;
        overlay.Visible = IsPaused;
        UpdateSelectionVisuals();
    }

    public void Update(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (!IsPaused) return;

        bool moveUp = false;
        bool moveDown = false;
        bool confirm = false;

        foreach (PlayerConfiguration player in playerConfigs)
        {
            moveUp |= player.Input.IsUpJustPressed();
            moveDown |= player.Input.IsDownJustPressed();
            confirm |= player.Input.IsPickupJustPressed();
        }

        if (moveUp || moveDown)
        {
            selectionIndex = 1 - selectionIndex;
            UpdateSelectionVisuals();
            soundService.PlayOnce(Sounds.MenuSelect);
        }

        if (!confirm) return;

        soundService.PlayOnce(Sounds.MenuSelect);

        if (selectionIndex == 0)
        {
            Toggle();
        }
        else
        {
            OnExitRequested?.Invoke();
        }
    }

    private void UpdateSelectionVisuals()
    {
        continueLabel.TextColor = selectionIndex == 0 ? Color.LightBlue : Color.White;
        exitLabel.TextColor = selectionIndex == 1 ? Color.LightBlue : Color.White;
    }

    private static Panel BuildOverlay(out Label continueLabel, out Label exitLabel)
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

        var font = GamelabGame.Instance.fontSystem;

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
            Font = font.GetFont(64),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.White
        });

        continueLabel = new Label
        {
            Text = "CONTINUE",
            Font = font.GetFont(40),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        exitLabel = new Label
        {
            Text = "EXIT",
            Font = font.GetFont(40),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stack.Widgets.Add(continueLabel);
        stack.Widgets.Add(exitLabel);
        modal.Widgets.Add(stack);
        overlay.Widgets.Add(modal);
        return overlay;
    }
}
