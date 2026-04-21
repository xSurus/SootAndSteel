using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Gamelab.UI;

public class PauseMenuController
{
    private readonly ISoundService soundService = GamelabGame.Instance.Services.GetService<ISoundService>();

    private readonly Panel overlay;
    private readonly Label continueLabel;
    private readonly Label optionsLabel;
    private readonly Label exitLabel;
    private readonly Label[] selectableLabels;
    private int selectionIndex;

    public bool IsPaused { get; private set; }
    public Panel Overlay => overlay;
    public OptionsPanel OptionsPanel { get; }

    public event Action OnExitRequested;

    public PauseMenuController()
    {
        overlay = BuildOverlay(out continueLabel, out optionsLabel, out exitLabel);
        selectableLabels = [continueLabel, optionsLabel, exitLabel];
        OptionsPanel = new OptionsPanel(GamelabGame.Instance);
        OptionsPanel.OnClosed += () => overlay.Visible = IsPaused;
        UpdateSelectionVisuals();
    }

    public bool IsToggleRequested(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (OptionsPanel.IsOpen) return false;
        return playerConfigs.Any(player => player.Input.IsPauseJustPressed());
    }

    public void Toggle()
    {
        IsPaused = !IsPaused;
        selectionIndex = 0;
        overlay.Visible = IsPaused;
        if (!IsPaused && OptionsPanel.IsOpen)
        {
            OptionsPanel.Close();
        }
        UpdateSelectionVisuals();
    }

    public void Update(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (!IsPaused) return;

        if (OptionsPanel.IsOpen)
        {
            overlay.Visible = false;
            OptionsPanel.Update(playerConfigs);
            if (!OptionsPanel.IsOpen) overlay.Visible = IsPaused;
            return;
        }

        bool moveUp = false;
        bool moveDown = false;
        bool confirm = false;

        foreach (PlayerConfiguration player in playerConfigs)
        {
            moveUp |= player.Input.IsUpJustPressed();
            moveDown |= player.Input.IsDownJustPressed();
            confirm |= player.Input.IsPickupJustPressed();
        }

        if (moveUp)
        {
            selectionIndex = (selectionIndex - 1 + selectableLabels.Length) % selectableLabels.Length;
            UpdateSelectionVisuals();
            soundService.PlayOnce(Sounds.MenuSelect);
        }
        else if (moveDown)
        {
            selectionIndex = (selectionIndex + 1) % selectableLabels.Length;
            UpdateSelectionVisuals();
            soundService.PlayOnce(Sounds.MenuSelect);
        }

        if (!confirm) return;

        soundService.PlayOnce(Sounds.MenuSelect);

        switch (selectionIndex)
        {
            case 0:
                Toggle();
                break;
            case 1:
                OptionsPanel.Open();
                overlay.Visible = false;
                break;
            case 2:
                OnExitRequested?.Invoke();
                break;
        }
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < selectableLabels.Length; i++)
        {
            selectableLabels[i].TextColor = i == selectionIndex ? Color.LightBlue : Color.White;
        }
    }

    private static Panel BuildOverlay(out Label continueLabel, out Label optionsLabel, out Label exitLabel)
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
            Height = 340,
            Background = new SolidBrush(new Color(25, 25, 30, 230)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var font = GamelabGame.Instance.fontSystem;

        var stack = new VerticalStackPanel
        {
            Spacing = 20,
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

        optionsLabel = new Label
        {
            Text = "OPTIONS",
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
        stack.Widgets.Add(optionsLabel);
        stack.Widgets.Add(exitLabel);
        modal.Widgets.Add(stack);
        overlay.Widgets.Add(modal);
        return overlay;
    }
}
