using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Input;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.Levels;

/// <summary>
/// Manages the intermission phase between levels.
/// Owns the overlay UI and input handling for advancing to the next level.
/// Replace the internals of this class to implement animations, shop screens, etc.
/// </summary>
public class IntermissionController
{
    private readonly RunManager runManager;
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly ISoundService soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
    private readonly Panel overlay;
    private bool wasEnterDown;

    /// <summary>Fired after the player confirms and the next level has been loaded.</summary>
    public event Action OnIntermissionComplete;

    public Panel Overlay => overlay;
    public bool IsActive => runManager.CurrentPhase == RunPhase.Intermission;

    public IntermissionController(RunManager runManager)
    {
        this.runManager = runManager;

        overlay = BuildOverlay(GamelabGame.Instance.fontSystem);
        runManager.OnIntermissionStarted += OnIntermissionStarted;
    }

    public void Update(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (!IsActive)
        {
            return;
        }

        bool confirm = false;
        foreach (PlayerConfiguration player in playerConfigs)
        {
            confirm |= player.Input.IsPickupJustPressed();
        }

        bool isEnterDown = Keyboard.GetState().IsKeyDown(Keys.Enter);
        bool enterPressed = isEnterDown && !wasEnterDown;
        wasEnterDown = isEnterDown;
        confirm |= enterPressed;

        if (!confirm)
        {
            return;
        }

        if (runManager.TryAdvanceToNextLevel(gameplayContext.State.DistanceTraveled))
        {
            overlay.Visible = false;
            gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Default;
            soundService.PlayOnce(Sounds.MenuSelect);
            OnIntermissionComplete?.Invoke();
        }
    }

    public void Unsubscribe()
    {
        runManager.OnIntermissionStarted -= OnIntermissionStarted;
    }

    private void OnIntermissionStarted(int _)
    {
        gameplayContext.State.CurrentSpeed = TrainSpeedSetting.Stopped;
        gameplayContext.State.actualSpeed = 0f;
        overlay.Visible = true;
    }

    private static Panel BuildOverlay(FontSystem fontSystem)
    {
        var panel = new Panel
        {
            Background = new SolidBrush(new Color(0, 0, 0, 180)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visible = false
        };

        var modal = new Panel
        {
            Width = 500,
            Height = 220,
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
            Text = "LEVEL COMPLETE",
            Font = fontSystem.GetFont(56),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.Gold
        });

        stack.Widgets.Add(new Label
        {
            Text = "Press ENTER to continue",
            Font = fontSystem.GetFont(32),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.LightGray
        });

        modal.Widgets.Add(stack);
        panel.Widgets.Add(modal);
        return panel;
    }
}
